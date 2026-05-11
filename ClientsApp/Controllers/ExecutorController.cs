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
            var hasFilters = !string.IsNullOrWhiteSpace(fullName) || hourlyRate.HasValue;
            var executors = hasFilters
                ? await _executorService.SearchAsync(fullName, hourlyRate)
                : await _executorService.GetAllAsync();

            var today = DateTime.Today;
            var normalizedStatus = string.IsNullOrWhiteSpace(statusFilter) ? "all" : statusFilter.ToLowerInvariant();
            executors = normalizedStatus switch
            {
                "working" => executors.Where(e => !e.DismissedFrom.HasValue || e.DismissedFrom.Value.Date > today),
                "dismissed" => executors.Where(e => e.DismissedFrom.HasValue && e.DismissedFrom.Value.Date <= today),
                _ => executors
            };

            var normalizedSortBy = string.IsNullOrWhiteSpace(sortBy) ? "id" : sortBy.ToLowerInvariant();
            if (normalizedSortBy != "name" && normalizedSortBy != "id")
            {
                normalizedSortBy = "id";
            }

            var normalizedSortDirection = string.IsNullOrWhiteSpace(sortDirection)
                ? "desc"
                : sortDirection.ToLowerInvariant();
            if (normalizedSortDirection != "asc" && normalizedSortDirection != "desc")
            {
                normalizedSortDirection = normalizedSortBy == "name" ? "asc" : "desc";
            }

            executors = normalizedSortBy switch
            {
                "name" when normalizedSortDirection == "desc" => executors.OrderByDescending(e => e.FullName),
                "name" => executors.OrderBy(e => e.FullName),
                "id" when normalizedSortDirection == "asc" => executors.OrderBy(e => e.ExecutorId),
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

        [Authorize(Roles = "Manager")]
        public IActionResult Create() => View();

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Create(Executor executor)
        {
            ValidateUnavailablePeriod(executor);
            if (!ModelState.IsValid) return View(executor);

            await _executorService.AddAsync(executor);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var executor = await _executorService.GetByIdAsync(id);
            if (executor == null) return NotFound();

            return View(executor);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(Executor executor)
        {
            ValidateUnavailablePeriod(executor);
            if (!ModelState.IsValid) return View(executor);

            await _executorService.UpdateAsync(executor);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var executor = await _executorService.GetByIdAsync(id);
            if (executor == null) return NotFound();
            return View(executor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _executorService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private void ValidateUnavailablePeriod(Executor executor)
        {
            var today = DateTime.Today;

            if (executor.UnavailableFrom.HasValue && executor.UnavailableFrom.Value.Date < today)
            {
                ModelState.AddModelError(nameof(Executor.UnavailableFrom), "Дата \"Недоступний з\" не може бути раніше поточної дати.");
            }

            if (executor.UnavailableFrom.HasValue
                && executor.UnavailableTo.HasValue
                && executor.UnavailableTo.Value.Date < executor.UnavailableFrom.Value.Date)
            {
                ModelState.AddModelError(nameof(Executor.UnavailableTo), "Дата \"Недоступний до\" не може бути раніше дати \"Недоступний з\".");
            }

            if (executor.DismissedFrom.HasValue && executor.DismissedFrom.Value.Date < today)
            {
                ModelState.AddModelError(nameof(Executor.DismissedFrom), "Дата \"Звільнений з дати\" не може бути раніше поточної дати.");
            }
        }
    }
}
