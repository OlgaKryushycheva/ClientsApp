using ClientsApp.BLL.Interfaces;
using ClientsApp.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace ClientsApp.Controllers
{
    public class ExecutorTaskController : Controller
    {
        private readonly IExecutorTaskService _executorTaskService;
        private readonly IExecutorService _executorService;

        public ExecutorTaskController(IExecutorTaskService executorTaskService, IExecutorService executorService)
        {
            _executorTaskService = executorTaskService;
            _executorService = executorService;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _executorTaskService.GetAllAsync();
            return View(items);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Executors = await _executorService.GetAllAsync();
            return View(new ExecutorTask());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExecutorTask executorTask)
        {
            if (!ModelState.IsValid)
            {
                // Повертаємо повідомлення про помилки 
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage); return Content("ModelState invalid: " + string.Join(", ", errors));
            }
            
            if (ModelState.IsValid)
            {
                await _executorTaskService.AddAsync(executorTask);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Executors = await _executorService.GetAllAsync();
            return View(executorTask);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var executorTask = await _executorTaskService.GetByIdAsync(id);
            if (executorTask == null) return NotFound();

            ViewBag.Executors = await _executorService.GetAllAsync();
            ViewBag.Clients = await _executorTaskService.GetClientsByExecutorAsync(executorTask.ExecutorId);
            var clientId = executorTask.ClientTask?.ClientId ?? 0;
            ViewBag.Tasks = await _executorTaskService.GetTasksByExecutorAndClientAsync(executorTask.ExecutorId, clientId);
            ViewBag.CurrentClientId = clientId;
            return View(executorTask);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ExecutorTask executorTask)
        {
            if (ModelState.IsValid)
            {
                await _executorTaskService.UpdateAsync(executorTask);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Executors = await _executorService.GetAllAsync();
            ViewBag.Clients = await _executorTaskService.GetClientsByExecutorAsync(executorTask.ExecutorId);
            var clientId = (await _executorTaskService.GetByIdAsync(executorTask.ExecutorTaskId))?.ClientTask?.ClientId ?? 0;
            ViewBag.Tasks = await _executorTaskService.GetTasksByExecutorAndClientAsync(executorTask.ExecutorId, clientId);
            ViewBag.CurrentClientId = clientId;
            return View(executorTask);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var executorTask = await _executorTaskService.GetByIdAsync(id);
            if (executorTask == null) return NotFound();
            return View(executorTask);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _executorTaskService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetClientsByExecutor(int executorId)
        {
            var clients = await _executorTaskService.GetClientsByExecutorAsync(executorId);
            return Json(clients.Select(c => new { c.ClientId, c.Name }));
        }

        [HttpGet]
        public async Task<IActionResult> GetTasksByExecutorClient(int executorId, int clientId)
        {
            var tasks = await _executorTaskService.GetTasksByExecutorAndClientAsync(executorId, clientId);
            return Json(tasks.Select(t => new { t.ClientTaskId, t.TaskTitle }));
        }
    }
}
