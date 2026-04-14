using ClientsApp.BLL.Interfaces;
using ClientsApp.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace ClientsApp.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ExecutorController : Controller
    {
        private readonly IExecutorService _executorService;

        public ExecutorController(IExecutorService executorService)
        {
            _executorService = executorService;
        }

        public async Task<IActionResult> Index(string? fullName, decimal? hourlyRate)
        {
            var hasFilters = !string.IsNullOrWhiteSpace(fullName) || hourlyRate.HasValue;
            var executors = hasFilters
                ? await _executorService.SearchAsync(fullName, hourlyRate)
                : await _executorService.GetAllAsync();

            ViewData["FullName"] = fullName;
            ViewData["HourlyRate"] = hourlyRate.HasValue
                ? hourlyRate.Value.ToString(CultureInfo.InvariantCulture)
                : null;

            return View(executors);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Executor executor)
        {
            ValidateUnavailablePeriod(executor);
            if (!ModelState.IsValid) return View(executor);

            await _executorService.AddAsync(executor);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var executor = await _executorService.GetByIdAsync(id);
            if (executor == null) return NotFound();

            return View(executor);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Executor executor)
        {
            ValidateUnavailablePeriod(executor);
            if (!ModelState.IsValid) return View(executor);

            await _executorService.UpdateAsync(executor);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var executor = await _executorService.GetByIdAsync(id);
            if (executor == null) return NotFound();
            return View(executor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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
        }
    }
}
