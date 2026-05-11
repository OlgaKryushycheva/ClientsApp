using ClientsApp.BLL.Interfaces;
using ClientsApp.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ClientsApp.Controllers
{
    // Доступ до списку виконавців дозволений лише авторизованим користувачам,
    // оскільки тут відображаються кадрові дані (ставка, статус зайнятості, дати недоступності/звільнення).
    [Authorize]
    public class ExecutorController : Controller
    {
        private readonly IExecutorService _executorService;

        public ExecutorController(IExecutorService executorService)
        {
            _executorService = executorService;
        }

        public async Task<IActionResult> Index(
            string? fullName,
            decimal? hourlyRate,
            string? statusFilter,
            string? sortBy,
            string? sortDirection)
        {
            // Якщо користувач ввів ПІБ або погодинну ставку, виконуємо пошук по цих полях;
            // інакше показуємо повний перелік виконавців.
            var hasFilters = !string.IsNullOrWhiteSpace(fullName) || hourlyRate.HasValue;
            var executors = hasFilters
                ? await _executorService.SearchAsync(fullName, hourlyRate)
                : await _executorService.GetAllAsync();

            var today = DateTime.Today;
            // Нормалізуємо значення фільтра статусу, щоб однаково обробляти "Working", "working" тощо.
            var normalizedStatus = string.IsNullOrWhiteSpace(statusFilter) ? "all" : statusFilter.ToLowerInvariant();
            executors = normalizedStatus switch
            {
                // "working": у списку тільки активні виконавці — без дати звільнення або зі звільненням у майбутньому.
                "working" => executors.Where(e => !e.DismissedFrom.HasValue || e.DismissedFrom.Value.Date > today),
                // "dismissed": виконавці, чия дата звільнення вже настала (сьогодні або раніше).
                "dismissed" => executors.Where(e => e.DismissedFrom.HasValue && e.DismissedFrom.Value.Date <= today),
                _ => executors
            };

            // Дозволяємо сортування лише за підтримуваними полями,
            // щоб випадкове/некоректне значення query-параметра не ламало порядок у таблиці.
            var normalizedSortBy = string.IsNullOrWhiteSpace(sortBy) ? "id" : sortBy.ToLowerInvariant();
            if (normalizedSortBy != "name" && normalizedSortBy != "id")
            {
                normalizedSortBy = "id";
            }

            // Напрямок сортування також приводимо до очікуваних "asc"/"desc";
            // для ПІБ за замовчуванням зручно показувати А-Я, для ID — новіші записи першими.
            var normalizedSortDirection = string.IsNullOrWhiteSpace(sortDirection)
                ? "desc"
                : sortDirection.ToLowerInvariant();
            if (normalizedSortDirection != "asc" && normalizedSortDirection != "desc")
            {
                normalizedSortDirection = normalizedSortBy == "name" ? "asc" : "desc";
            }

            // LINQ-сортування застосовується після фільтрації, тому користувач бачить
            // вже відфільтрований набір у потрібному порядку (за ПІБ або за ID).
            executors = normalizedSortBy switch
            {
                "name" when normalizedSortDirection == "desc" => executors.OrderByDescending(e => e.FullName),
                "name" => executors.OrderBy(e => e.FullName),
                "id" when normalizedSortDirection == "asc" => executors.OrderBy(e => e.ExecutorId),
                _ => executors.OrderByDescending(e => e.ExecutorId)
            };

            // Зберігаємо значення пошуку/сортування у ViewData,
            // щоб форма фільтрів у поданні залишалась заповненою після перезавантаження сторінки.
            ViewData["FullName"] = fullName;
            ViewData["HourlyRate"] = hourlyRate.HasValue
                ? hourlyRate.Value.ToString(CultureInfo.InvariantCulture)
                : null;
            ViewData["StatusFilter"] = normalizedStatus;
            ViewData["SortBy"] = normalizedSortBy;
            ViewData["SortDirection"] = normalizedSortDirection;

            return View(executors);
        }

        // Створювати виконавців може лише менеджер,
        // бо саме він керує складом команди та доступністю людей для задач.
        [Authorize(Roles = "Manager")]
        public IActionResult Create() => View();

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Create(Executor executor)
        {
            // Перевіряємо реалістичність дат перед збереженням:
            // період недоступності не може починатися в минулому і завершуватися раніше початку.
            ValidateUnavailablePeriod(executor);
            // Якщо валідація моделі або дат не пройшла — повертаємо ту саму форму з повідомленнями про помилки.
            if (!ModelState.IsValid) return View(executor);

            await _executorService.AddAsync(executor);
            // Після додавання повертаємося до загального списку виконавців.
            return RedirectToAction(nameof(Index));
        }

        // Редагування виконавців обмежене роллю Manager,
        // щоб лише відповідальний за команду змінював ставку, статус і дати.
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var executor = await _executorService.GetByIdAsync(id);
            // Якщо виконавця з таким ID немає в базі — показуємо 404 замість порожньої форми.
            if (executor == null) return NotFound();

            return View(executor);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(Executor executor)
        {
            // Перед оновленням застосовуємо ті ж перевірки дат,
            // щоб у запис не потрапив некоректний період недоступності чи звільнення.
            ValidateUnavailablePeriod(executor);
            if (!ModelState.IsValid) return View(executor);

            await _executorService.UpdateAsync(executor);
            // Після збереження змін повертаємося до списку виконавців.
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var executor = await _executorService.GetByIdAsync(id);
            // Якщо запис уже видалений або ID помилковий — віддаємо 404.
            if (executor == null) return NotFound();
            return View(executor);
        }

        [HttpPost, ActionName("Delete")]
        // Перевіряє, що підтвердження видалення виконавця надійшло саме з форми цього сайту.
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _executorService.DeleteAsync(id);
            // Після видалення повертаємо менеджера до таблиці виконавців.
            return RedirectToAction(nameof(Index));
        }

        private void ValidateUnavailablePeriod(Executor executor)
        {
            var today = DateTime.Today;

            // Nullable-перевірка HasValue потрібна, бо дата "Недоступний з" необов'язкова:
            // порівняння виконуємо лише коли користувач дійсно вказав це поле.
            if (executor.UnavailableFrom.HasValue && executor.UnavailableFrom.Value.Date < today)
            {
                ModelState.AddModelError(nameof(Executor.UnavailableFrom), "Дата \"Недоступний з\" не може бути раніше поточної дати.");
            }

            // Якщо задані обидві межі періоду, кінець недоступності не може бути раніше початку.
            if (executor.UnavailableFrom.HasValue
                && executor.UnavailableTo.HasValue
                && executor.UnavailableTo.Value.Date < executor.UnavailableFrom.Value.Date)
            {
                ModelState.AddModelError(nameof(Executor.UnavailableTo), "Дата \"Недоступний до\" не може бути раніше дати \"Недоступний з\".");
            }

            // Дата звільнення не приймається в минулому:
            // це захищає від помилкового "заднім числом" переведення активного виконавця в звільнені.
            if (executor.DismissedFrom.HasValue && executor.DismissedFrom.Value.Date < today)
            {
                ModelState.AddModelError(nameof(Executor.DismissedFrom), "Дата \"Звільнений з дати\" не може бути раніше поточної дати.");
            }
        }
    }
}
