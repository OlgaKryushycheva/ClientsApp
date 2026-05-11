// Контролер ClientController обробляє HTTP-запити цього розділу UI.
// Дії нижче читають параметри запиту, викликають сервіси й повертають View/Redirect/JSON.
﻿using ClientsApp.BLL.Interfaces;
using ClientsApp.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientsApp.Controllers
{
// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
    [Authorize]
// ClientController: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public class ClientController : Controller
    {
        private readonly IClientService _clientService;

        public ClientController(IClientService clientService)
        {
            _clientService = clientService;
        }

// Метод Index реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: string searchString, string? sortBy, string? sortDirection.
        public async Task<IActionResult> Index(string searchString, string? sortBy, string? sortDirection)
        {
            IEnumerable<Client> clients;
            var hasSearch = !string.IsNullOrWhiteSpace(searchString) && searchString.Length >= 3;
            clients = hasSearch
                ? await _clientService.SearchByNameAsync(searchString)
                : await _clientService.GetAllAsync();

            var normalizedSortBy = string.IsNullOrWhiteSpace(sortBy) ? "id" : sortBy.ToLowerInvariant();
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (normalizedSortBy != "name" && normalizedSortBy != "id")
            {
                normalizedSortBy = "id";
            }

            var normalizedSortDirection = string.IsNullOrWhiteSpace(sortDirection)
                ? "asc"
                : sortDirection.ToLowerInvariant();
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (normalizedSortDirection != "asc" && normalizedSortDirection != "desc")
            {
                normalizedSortDirection = "asc";
            }

            clients = normalizedSortBy switch
            {
// Це сортування формує передбачуваний порядок рядків у таблиці на сторінці.
                "name" when normalizedSortDirection == "desc" => clients.OrderByDescending(c => c.Name),
// Це сортування формує передбачуваний порядок рядків у таблиці на сторінці.
                "name" => clients.OrderBy(c => c.Name),
// Це сортування формує передбачуваний порядок рядків у таблиці на сторінці.
                "id" when normalizedSortDirection == "desc" => clients.OrderByDescending(c => c.ClientId),
// Це сортування формує передбачуваний порядок рядків у таблиці на сторінці.
                _ => clients.OrderBy(c => c.ClientId)
            };

            ViewData["SearchString"] = searchString;
            ViewData["SortBy"] = normalizedSortBy;
            ViewData["SortDirection"] = normalizedSortDirection;
            return View(clients);
        }

// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager")]
// Метод Create реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
        public IActionResult Create() => View();

// HTTP POST приймає дані форми та запускає операцію створення/оновлення/видалення.
        [HttpPost]
// Anti-forgery токен блокує CSRF: сторонній сайт не зможе відправити форму від імені користувача.
        [ValidateAntiForgeryToken]
// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager")]
// Метод Create реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: Client client.
        public async Task<IActionResult> Create(Client client)
        {
// Якщо валідація моделі не пройдена, зупиняємо запис у БД і повертаємо форму з помилками користувачу.
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (!ModelState.IsValid) return View(client);

            await _clientService.AddAsync(client);
            return RedirectToAction(nameof(Index));
        }

// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager")]
// Метод Edit реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int id.
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (client == null) return NotFound();
            return View(client);
        }

// HTTP POST приймає дані форми та запускає операцію створення/оновлення/видалення.
        [HttpPost]
// Anti-forgery токен блокує CSRF: сторонній сайт не зможе відправити форму від імені користувача.
        [ValidateAntiForgeryToken]
// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager")]
// Метод Edit реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: Client client.
        public async Task<IActionResult> Edit(Client client)
        {
// Якщо валідація моделі не пройдена, зупиняємо запис у БД і повертаємо форму з помилками користувачу.
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (!ModelState.IsValid) return View(client);

            await _clientService.UpdateAsync(client);
            return RedirectToAction(nameof(Index));
        }

// HTTP GET використовується для читання даних і відкриття сторінки без зміни стану БД.
        [HttpGet]
// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager")]
// Метод Delete реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int id.
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (client == null) return NotFound();
            return View(client);
        }

// HTTP POST приймає дані форми та запускає операцію створення/оновлення/видалення.
        [HttpPost, ActionName("Delete")]
// Anti-forgery токен блокує CSRF: сторонній сайт не зможе відправити форму від імені користувача.
        [ValidateAntiForgeryToken]
// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager")]
// Метод DeleteConfirmed реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int id.
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _clientService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
