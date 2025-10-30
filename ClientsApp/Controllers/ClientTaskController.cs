using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;
using ClientsApp.Models.Entities;
using ClientsApp.Models.ViewModels;
using ClientsApp.BLL.Interfaces;

namespace ClientsApp.Controllers
{
    public class ClientTaskController : Controller
    {
        private readonly IClientTaskService _taskService;
        private readonly IClientService _clientService;
        private readonly IExecutorService _executorService;

        public ClientTaskController(
            IClientTaskService taskService,
            IClientService clientService,
            IExecutorService executorService)
        {
            _taskService = taskService;
            _clientService = clientService;
            _executorService = executorService;
        }

        public async Task<IActionResult> Index(int? selectedClientId, int? selectedExecutorId, ClientTaskStatusEnum? selectedStatus, string sortOrder)
        {
            var normalizedSortOrder = sortOrder == "desc" ? "desc" : "asc";
            var sortDescending = normalizedSortOrder == "desc";
            var tasks = await _taskService.SearchAsync(selectedClientId, selectedExecutorId, selectedStatus, sortDescending);

            var clients = await _clientService.GetAllAsync();
            var executors = await _executorService.GetAllAsync();

            var model = new ClientTaskIndexViewModel
            {
                Tasks = tasks,
                Clients = clients.Select(c => new SelectListItem
                {
                    Value = c.ClientId.ToString(),
                    Text = c.Name
                }).ToList(),
                Executors = executors.Select(e => new SelectListItem
                {
                    Value = e.ExecutorId.ToString(),
                    Text = e.FullName
                }).ToList(),
                Statuses = Enum.GetValues(typeof(ClientTaskStatusEnum))
                    .Cast<ClientTaskStatusEnum>()
                    .Select(s => new SelectListItem
                    {
                        Value = ((int)s).ToString(),
                        Text = s.ToString()
                    }).ToList(),
                SelectedClientId = selectedClientId,
                SelectedExecutorId = selectedExecutorId,
                SelectedStatus = selectedStatus,
                SortOrder = normalizedSortOrder
            };

            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Clients = new SelectList(await _clientService.GetAllAsync(), "ClientId", "Name");
            ViewBag.Executors = new MultiSelectList(await _executorService.GetAllAsync(), "ExecutorId", "FullName");
            ViewBag.Statuses = new SelectList(Enum.GetValues(typeof(ClientTaskStatusEnum)));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientTask task, int[] selectedExecutors)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Clients = new SelectList(await _clientService.GetAllAsync(), "ClientId", "Name", task.ClientId);
                ViewBag.Executors = new MultiSelectList(await _executorService.GetAllAsync(), "ExecutorId", "FullName", selectedExecutors);
                ViewBag.Statuses = new SelectList(Enum.GetValues(typeof(ClientTaskStatusEnum)), task.TaskStatus);
                return View(task);
            }

            task.ExecutorTasks = selectedExecutors.Select(eid => new ExecutorTask
            {
                ExecutorId = eid
            }).ToList();

            await _taskService.AddAsync(task);

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Edit(int id)
        {
            var task = await _taskService.GetByIdAsync(id);
            if (task == null) return NotFound();

            var model = new ClientTaskEditViewModel
            {
                ClientTaskId = task.ClientTaskId,
                TaskTitle = task.TaskTitle,
                Description = task.Description,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                ClientId = task.ClientId,
                TaskStatus = task.TaskStatus,
                SelectedExecutors = task.ExecutorTasks
                    .Where(et => et.ExecutorId.HasValue)
                    .Select(et => et.ExecutorId!.Value)
                    .ToList(),
                Clients = new SelectList(await _clientService.GetAllAsync(), "ClientId", "Name", task.ClientId),
                Executors = new MultiSelectList(await _executorService.GetAllAsync(), "ExecutorId", "FullName", task.ExecutorTasks.Select(et => et.ExecutorId)),
                Statuses = new SelectList(Enum.GetValues(typeof(ClientTaskStatusEnum)), task.TaskStatus)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClientTaskEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return Content("Некоректні дані моделі: " + string.Join(", ", errors));
            }

            var task = new ClientTask
            {
                ClientTaskId = model.ClientTaskId,
                TaskTitle = model.TaskTitle,
                Description = model.Description,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                ClientId = model.ClientId,
                TaskStatus = model.TaskStatus,
                ExecutorTasks = model.SelectedExecutors.Select(eid => new ExecutorTask { ExecutorId = eid }).ToList()
            };

            await _taskService.UpdateAsync(task);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var task = await _taskService.GetByIdAsync(id);
            if (task == null) return NotFound();
            return View(task);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _taskService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
