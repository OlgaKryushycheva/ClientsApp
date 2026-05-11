using ClientsApp.BLL.Interfaces;
using ClientsApp.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientsApp.Controllers
{
    [Authorize]
    public class ClientController : Controller
    {
        private readonly IClientService _clientService;

        public ClientController(IClientService clientService)
        {
            _clientService = clientService;
        }

        // searchString — текст із поля пошуку у таблиці клієнтів.
        // Пошук запускаємо лише від 3 символів, щоб короткі запити ("а", "ан")
        // не повертали майже весь список і не навантажували БД зайвими фільтраціями.
        // sortBy керує полем сортування (id або name), sortDirection — напрямком (asc/desc).
        public async Task<IActionResult> Index(string searchString, string? sortBy, string? sortDirection)
        {
            IEnumerable<Client> clients;
            var hasSearch = !string.IsNullOrWhiteSpace(searchString) && searchString.Length >= 3;
            clients = hasSearch
                ? await _clientService.SearchByNameAsync(searchString)
                : await _clientService.GetAllAsync();

            // Нормалізуємо параметри з query-string до нижнього регістру,
            // щоб "Name"/"NAME" оброблялись так само, як "name".
            var normalizedSortBy = string.IsNullOrWhiteSpace(sortBy) ? "id" : sortBy.ToLowerInvariant();
            // Якщо прийшло невідоме поле сортування, використовуємо id,
            // щоб уникнути непередбачуваної поведінки на сторінці списку.
            if (normalizedSortBy != "name" && normalizedSortBy != "id")
            {
                normalizedSortBy = "id";
            }

            var normalizedSortDirection = string.IsNullOrWhiteSpace(sortDirection)
                ? "asc"
                : sortDirection.ToLowerInvariant();
            // Некоректний напрямок теж приводимо до значення за замовчуванням.
            if (normalizedSortDirection != "asc" && normalizedSortDirection != "desc")
            {
                normalizedSortDirection = "asc";
            }

            // switch формує остаточний порядок рядків у таблиці
            // залежно від обраного поля й напряму сортування.
            clients = normalizedSortBy switch
            {
                "name" when normalizedSortDirection == "desc" => clients.OrderByDescending(c => c.Name),
                "name" => clients.OrderBy(c => c.Name),
                "id" when normalizedSortDirection == "desc" => clients.OrderByDescending(c => c.ClientId),
                _ => clients.OrderBy(c => c.ClientId)
            };

            // Зберігаємо поточні фільтри/сортування у ViewData,
            // щоб після рендеру сторінка показувала актуальний стан елементів керування.
            ViewData["SearchString"] = searchString;
            ViewData["SortBy"] = normalizedSortBy;
            ViewData["SortDirection"] = normalizedSortDirection;
            return View(clients);
        }

        // Тільки менеджер може створювати нового клієнта.
        [Authorize(Roles = "Manager")]
        public IActionResult Create() => View();

        [HttpPost]
        // Захищає POST-форму від CSRF-атак через підроблені запити.
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Create(Client client)
        {
            // Якщо модель невалідна, не виконуємо INSERT і повертаємо форму з помилками.
            if (!ModelState.IsValid) return View(client);

            // Після AddAsync сервіс викликає SaveChangesAsync, і в таблиці Clients з'являється новий рядок.
            await _clientService.AddAsync(client);
            return RedirectToAction(nameof(Index));
        }

        // Редагування клієнта доступне лише менеджеру.
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            // Якщо клієнт з таким ID не існує, повертаємо 404 замість порожньої форми.
            if (client == null) return NotFound();
            return View(client);
        }

        [HttpPost]
        // Захищає POST-форму редагування від CSRF-атак.
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(Client client)
        {
            // Якщо валідація не пройдена, не виконуємо UPDATE.
            if (!ModelState.IsValid) return View(client);

            await _clientService.UpdateAsync(client);
            return RedirectToAction(nameof(Index));
        }

        // Спочатку показуємо сторінку підтвердження, щоб користувач не видаляв клієнта випадково.
        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            // Якщо запис уже відсутній, показуємо 404.
            if (client == null) return NotFound();
            return View(client);
        }

        [HttpPost, ActionName("Delete")]
        // Захищає підтвердження видалення від CSRF-запитів.
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _clientService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
