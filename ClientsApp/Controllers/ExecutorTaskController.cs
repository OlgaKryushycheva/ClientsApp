using ClientsApp.BLL.Interfaces;
using ClientsApp.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;

namespace ClientsApp.Controllers
{
    public class ExecutorTaskController : Controller
    {
        private readonly IExecutorTaskService _executorTaskService;
        private readonly IExecutorService _executorService;
        private readonly IClientService _clientService;
        private readonly IClientTaskService _clientTaskService;

        public ExecutorTaskController(
            IExecutorTaskService executorTaskService,
            IExecutorService executorService,
            IClientService clientService,
            IClientTaskService clientTaskService)
        {
            _executorTaskService = executorTaskService;
            _executorService = executorService;
            _clientService = clientService;
            _clientTaskService = clientTaskService;
        }

        public async Task<IActionResult> Index(int? executorId, int? clientId, int? taskId)
        {
            ViewBag.Executors = new SelectList(await _executorService.GetAllAsync(), "ExecutorId", "FullName", executorId);
            ViewBag.Clients = new SelectList(await _clientService.GetAllAsync(), "ClientId", "Name", clientId);
            ViewBag.Tasks = new SelectList(await _clientTaskService.GetAllAsync(), "ClientTaskId", "TaskTitle", taskId);

            var items = await _executorTaskService.GetAllAsync(executorId, clientId, taskId);
            return View(items);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Executors = await _executorService.GetAllAsync();
            ViewBag.Clients = new SelectList(await _clientService.GetAllAsync(), "ClientId", "Name");
            ViewBag.Tasks = new SelectList(Enumerable.Empty<ClientTask>(), "ClientTaskId", "TaskTitle");
            return View(new ExecutorTask());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExecutorTask executorTask)
        {
            if (ModelState.IsValid)
            {
                await _executorTaskService.AddAsync(executorTask);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Executors = await _executorService.GetAllAsync();
            ViewBag.Clients = new SelectList(await _clientService.GetAllAsync(), "ClientId", "Name", executorTask.ClientId);
            var tasks = await _clientTaskService.GetAllAsync();
            ViewBag.Tasks = new SelectList(tasks.Where(t => t.ClientId == executorTask.ClientId), "ClientTaskId", "TaskTitle", executorTask.ClientTaskId);
            return View(executorTask);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var executorTask = await _executorTaskService.GetByIdAsync(id);
            if (executorTask == null) return NotFound();

            ViewBag.Executors = await _executorService.GetAllAsync();
            ViewBag.Clients = await _executorTaskService.GetClientsByExecutorAsync(executorTask.ExecutorId ?? 0);
            var clientId = executorTask.ClientTask?.ClientId ?? 0;
            executorTask.ClientId = clientId;
            ViewBag.Tasks = await _executorTaskService.GetTasksByExecutorAndClientAsync(executorTask.ExecutorId ?? 0, clientId);
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
            ViewBag.Clients = await _executorTaskService.GetClientsByExecutorAsync(executorTask.ExecutorId ?? 0);
            var clientId = executorTask.ClientId ?? (await _executorTaskService.GetByIdAsync(executorTask.ExecutorTaskId))?.ClientTask?.ClientId ?? 0;
            ViewBag.Tasks = await _executorTaskService.GetTasksByExecutorAndClientAsync(executorTask.ExecutorId ?? 0, clientId);
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
