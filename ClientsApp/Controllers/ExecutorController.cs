// Контролер ExecutorController обробляє HTTP-запити цього розділу UI.
// Дії нижче читають параметри запиту, викликають сервіси й повертають View/Redirect/JSON.
﻿using ClientsApp.BLL.Interfaces;
using ClientsApp.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ClientsApp.Controllers
{
// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
    [Authorize]
// ExecutorController: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
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
            var hasFilters = !string.IsNullOrWhiteSpace(fullName) || hourlyRate.HasValue;
            var executors = hasFilters
                ? await _executorService.SearchAsync(fullName, hourlyRate)
                : await _executorService.GetAllAsync();

            var today = DateTime.Today;
            var normalizedStatus = string.IsNullOrWhiteSpace(statusFilter) ? "all" : statusFilter.ToLowerInvariant();
            executors = normalizedStatus switch
            {
// Фільтр Where залишає лише записи, що відповідають умові, тому у View не потрапляють зайві дані.
                "working" => executors.Where(e => !e.DismissedFrom.HasValue || e.DismissedFrom.Value.Date > today),
// Фільтр Where залишає лише записи, що відповідають умові, тому у View не потрапляють зайві дані.
                "dismissed" => executors.Where(e => e.DismissedFrom.HasValue && e.DismissedFrom.Value.Date <= today),
                _ => executors
            };

            var normalizedSortBy = string.IsNullOrWhiteSpace(sortBy) ? "id" : sortBy.ToLowerInvariant();
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (normalizedSortBy != "name" && normalizedSortBy != "id")
            {
                normalizedSortBy = "id";
            }

            var normalizedSortDirection = string.IsNullOrWhiteSpace(sortDirection)
                ? "desc"
                : sortDirection.ToLowerInvariant();
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (normalizedSortDirection != "asc" && normalizedSortDirection != "desc")
            {
                normalizedSortDirection = normalizedSortBy == "name" ? "asc" : "desc";
            }

            executors = normalizedSortBy switch
            {
// Це сортування формує передбачуваний порядок рядків у таблиці на сторінці.
                "name" when normalizedSortDirection == "desc" => executors.OrderByDescending(e => e.FullName),
// Це сортування формує передбачуваний порядок рядків у таблиці на сторінці.
                "name" => executors.OrderBy(e => e.FullName),
// Це сортування формує передбачуваний порядок рядків у таблиці на сторінці.
                "id" when normalizedSortDirection == "asc" => executors.OrderBy(e => e.ExecutorId),
// Це сортування формує передбачуваний порядок рядків у таблиці на сторінці.
                _ => executors.OrderByDescending(e => e.ExecutorId)
            };

            ViewData["FullName"] = fullName;
            ViewData["HourlyRate"] = hourlyRate.HasValue
                ? hourlyRate.Value.ToString(CultureInfo.InvariantCulture)
                : null;
            ViewData["StatusFilter"] = normalizedStatus;
            ViewData["SortBy"] = normalizedSortBy;
            ViewData["SortDirection"] = normalizedSortDirection;

            return View(executors);
        }

// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager")]
// Метод Create реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
        public IActionResult Create() => View();

// HTTP POST приймає дані форми та запускає операцію створення/оновлення/видалення.
        [HttpPost]
// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager")]
// Метод Create реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: Executor executor.
        public async Task<IActionResult> Create(Executor executor)
        {
            ValidateUnavailablePeriod(executor);
// Якщо валідація моделі не пройдена, зупиняємо запис у БД і повертаємо форму з помилками користувачу.
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (!ModelState.IsValid) return View(executor);

            await _executorService.AddAsync(executor);
            return RedirectToAction(nameof(Index));
        }

// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager")]
// Метод Edit реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int id.
        public async Task<IActionResult> Edit(int id)
        {
            var executor = await _executorService.GetByIdAsync(id);
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (executor == null) return NotFound();

            return View(executor);
        }

// HTTP POST приймає дані форми та запускає операцію створення/оновлення/видалення.
        [HttpPost]
// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager")]
// Метод Edit реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: Executor executor.
        public async Task<IActionResult> Edit(Executor executor)
        {
            ValidateUnavailablePeriod(executor);
// Якщо валідація моделі не пройдена, зупиняємо запис у БД і повертаємо форму з помилками користувачу.
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (!ModelState.IsValid) return View(executor);

            await _executorService.UpdateAsync(executor);
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
            var executor = await _executorService.GetByIdAsync(id);
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (executor == null) return NotFound();
            return View(executor);
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
            await _executorService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private void ValidateUnavailablePeriod(Executor executor)
        {
            var today = DateTime.Today;

// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (executor.UnavailableFrom.HasValue && executor.UnavailableFrom.Value.Date < today)
            {
                ModelState.AddModelError(nameof(Executor.UnavailableFrom), "Дата \"Недоступний з\" не може бути раніше поточної дати.");
            }

// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (executor.UnavailableFrom.HasValue
                && executor.UnavailableTo.HasValue
                && executor.UnavailableTo.Value.Date < executor.UnavailableFrom.Value.Date)
            {
                ModelState.AddModelError(nameof(Executor.UnavailableTo), "Дата \"Недоступний до\" не може бути раніше дати \"Недоступний з\".");
            }

// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (executor.DismissedFrom.HasValue && executor.DismissedFrom.Value.Date < today)
            {
                ModelState.AddModelError(nameof(Executor.DismissedFrom), "Дата \"Звільнений з дати\" не може бути раніше поточної дати.");
            }
        }
    }
}
