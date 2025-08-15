using ClientsApp.BLL.Interfaces;
using ClientsApp.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ClientsApp.Controllers
{
    public class ExecutorController : Controller
    {
        private readonly IExecutorService _executorService;

        public ExecutorController(IExecutorService executorService)
        {
            _executorService = executorService;
        }

        public async Task<IActionResult> Index()
        {
            var executors = await _executorService.GetAllAsync();
            return View(executors);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Executor executor)
        {
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
            if (!ModelState.IsValid) return View(executor);

            await _executorService.UpdateAsync(executor);
            return RedirectToAction(nameof(Index));
        }

        //public async Task<IActionResult> Delete(int id)
        //{
        //    var executor = await _executorService.GetByIdAsync(id);
        //    if (executor == null) return NotFound();

        //    await _executorService.DeleteAsync(id);
        //    return RedirectToAction(nameof(Index));
        //}

        // GET: показати сторінку підтвердження видалення
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var executor = await _executorService.GetByIdAsync(id);
            if (executor == null) return NotFound();

            return View(executor); // повертає Delete.cshtml
        }

        // POST: власне видалення після підтвердження
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _executorService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
