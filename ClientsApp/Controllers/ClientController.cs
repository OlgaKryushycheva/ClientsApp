using ClientsApp.BLL.Interfaces;
using ClientsApp.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientsApp.Controllers
{
    // Список клієнтів доступний лише авторизованим користувачам,
    // щоб анонімні відвідувачі не бачили персональні дані.
    [Authorize]
    public class ClientController : Controller
    {
        private readonly IClientService _clientService;

        public ClientController(IClientService clientService)
        {
            _clientService = clientService;
        }

        // Відкриває сторінку списку клієнтів.
        // searchString — текст із поля пошуку у таблиці клієнтів.
        // sortBy визначає поле сортування ("id" або "name"), sortDirection — напрямок ("asc"/"desc").
        // Метод повертає колекцію клієнтів у View.
        public async Task<IActionResult> Index(string searchString, string? sortBy, string? sortDirection)
        {
            IEnumerable<Client> clients;
            // Пошук запускаємо тільки з 3 символів, щоб уникнути дуже широких запитів
            // на 1-2 символи, які зазвичай повертають майже всю таблицю.
            var hasSearch = !string.IsNullOrWhiteSpace(searchString) && searchString.Length >= 3;
            clients = hasSearch
                ? await _clientService.SearchByNameAsync(searchString)
                : await _clientService.GetAllAsync();

            // Нормалізуємо sortBy до нижнього регістру і задаємо "id" за замовчуванням,
            // щоб параметр із query-string оброблявся стабільно.
            var normalizedSortBy = string.IsNullOrWhiteSpace(sortBy) ? "id" : sortBy.ToLowerInvariant();
            // Якщо в URL передали невідоме поле, повертаємось до безпечного сортування за ID.
            if (normalizedSortBy != "name" && normalizedSortBy != "id")
            {
                normalizedSortBy = "id";
            }

            // Те саме для напряму сортування: за замовчуванням сортуємо по зростанню.
            var normalizedSortDirection = string.IsNullOrWhiteSpace(sortDirection)
                ? "asc"
                : sortDirection.ToLowerInvariant();
            // Некоректний напрямок замінюємо на "asc", щоб не ламати логіку switch нижче.
            if (normalizedSortDirection != "asc" && normalizedSortDirection != "desc")
            {
                normalizedSortDirection = "asc";
            }

            // switch будує кінцевий порядок:
            // - за ім'ям у прямому/зворотному напрямку;
            // - за ID у прямому/зворотному напрямку.
            // Це дає передбачувану поведінку при кліках по заголовках таблиці.
            clients = normalizedSortBy switch
            {
                "name" when normalizedSortDirection == "desc" => clients.OrderByDescending(c => c.Name),
                "name" => clients.OrderBy(c => c.Name),
                "id" when normalizedSortDirection == "desc" => clients.OrderByDescending(c => c.ClientId),
                _ => clients.OrderBy(c => c.ClientId)
            };

            // Зберігаємо поточні параметри у ViewData, щоб форма пошуку й індикатор сортування
            // на сторінці показували той самий стан після перезавантаження.
            ViewData["SearchString"] = searchString;
            ViewData["SortBy"] = normalizedSortBy;
            ViewData["SortDirection"] = normalizedSortDirection;
            return View(clients);
        }

        // Створення клієнта дозволено тільки менеджеру.
        [Authorize(Roles = "Manager")]
        public IActionResult Create() => View();

        // POST-версія Create зберігає нового клієнта в таблицю Clients.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Create(Client client)
        {
            // Якщо користувач не заповнив обов'язкові поля, повертаємо ту саму форму з помилками валідації.
            if (!ModelState.IsValid) return View(client);

            // Сервіс додає запис до DbSet, а потім виконує SQL INSERT під час SaveChangesAsync.
            await _clientService.AddAsync(client);
            return RedirectToAction(nameof(Index));
        }

        // Відкриває форму редагування конкретного клієнта за його ID.
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            // Якщо запис уже видалено або ID не існує, повертаємо 404.
            if (client == null) return NotFound();
            return View(client);
        }

        // Приймає змінені дані клієнта та виконує SQL UPDATE через сервіс.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(Client client)
        {
            // Не зберігаємо невалідні значення (порожнє ім'я, некоректні обмеження тощо).
            if (!ModelState.IsValid) return View(client);

            await _clientService.UpdateAsync(client);
            return RedirectToAction(nameof(Index));
        }

        // GET-сторінка підтвердження видалення, щоб користувач бачив, кого саме видаляє.
        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            // Захист від прямого переходу по неіснуючому ID.
            if (client == null) return NotFound();
            return View(client);
        }

        // Після підтвердження видаляє клієнта та повертає користувача до списку.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _clientService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
