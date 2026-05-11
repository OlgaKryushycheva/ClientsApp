using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;
using ClientsApp.Models.Entities;
using ClientsApp.Models.ViewModels;
using ClientsApp.BLL.Interfaces;
using System.Collections.Generic;

namespace ClientsApp.Controllers
{
    [Authorize]
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

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Create()
        {
            await PopulateCreateViewBagsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Create(ClientTask task, int[] selectedExecutors)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCreateViewBagsAsync(task, selectedExecutors);
                return View(task);
            }

            var startDate = task.StartDate.Date;
            var selectedExecutorsData = (await _executorService.GetAllAsync())
                .Where(e => selectedExecutors.Contains(e.ExecutorId))
                .ToList();

            var invalidUnavailableExecutors = selectedExecutorsData
                .Where(e => e.UnavailableFrom.HasValue
                    && e.UnavailableTo.HasValue
                    && startDate >= e.UnavailableFrom.Value.Date
                    && startDate <= e.UnavailableTo.Value.Date)
                .Select(e => e.FullName)
                .ToList();

            if (invalidUnavailableExecutors.Count > 0)
            {
                ModelState.AddModelError(string.Empty, $"Обрані виконавці недоступні на дату початку: {string.Join(", ", invalidUnavailableExecutors)}.");
            }

            var dismissedExecutors = selectedExecutorsData
                .Where(e => e.DismissedFrom.HasValue && startDate >= e.DismissedFrom.Value.Date)
                .Select(e => e.FullName)
                .ToList();

            if (dismissedExecutors.Count > 0)
            {
                ModelState.AddModelError(string.Empty, $"Обрані виконавці звільнені на дату початку: {string.Join(", ", dismissedExecutors)}.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateCreateViewBagsAsync(task, selectedExecutors);
                return View(task);
            }

            task.ExecutorTasks = selectedExecutors.Select(eid => new ExecutorTask
            {
                ExecutorId = eid
            }).ToList();

            await _taskService.AddAsync(task);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> InProgressByExecutorIds([FromQuery] int[] executorIds)
        {
            if (executorIds == null || executorIds.Length == 0)
            {
                return Json(Array.Empty<object>());
            }

            var allInProgressTasks = await _taskService.SearchAsync(null, null, ClientTaskStatusEnum.InProgress);

            var result = allInProgressTasks
                .Where(t => t.ExecutorTasks.Any(et => et.ExecutorId.HasValue && executorIds.Contains(et.ExecutorId.Value)))
                .Select(t => new
                {
                    clientName = t.Client?.Name ?? "Без клієнта",
                    taskTitle = t.TaskTitle,
                    status = t.TaskStatus.ToString(),
                    executors = t.ExecutorTasks
                        .Where(et => et.ExecutorId.HasValue && executorIds.Contains(et.ExecutorId.Value))
                        .Select(et => et.Executor?.FullName ?? "Невідомий виконавець")
                        .Distinct()
                        .ToList()
                })
                .ToList();

            return Json(result);
        }


        [Authorize(Roles = "Manager")]
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
        [Authorize(Roles = "Manager")]
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

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _taskService.GetByIdAsync(id);
            if (task == null) return NotFound();
            return View(task);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _taskService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateCreateViewBagsAsync(ClientTask? task = null, int[]? selectedExecutors = null)
        {
            var today = DateTime.Today;

            ViewBag.Clients = new SelectList(await _clientService.GetAllAsync(), "ClientId", "Name", task?.ClientId);
            ViewBag.Executors = (await _executorService.GetAllAsync())
                .Where(e => !e.DismissedFrom.HasValue || e.DismissedFrom.Value.Date >= today)
                .OrderBy(e => e.FullName)
                .ToList();
            ViewBag.SelectedExecutors = new HashSet<int>(selectedExecutors ?? Array.Empty<int>());
            ViewBag.Statuses = new SelectList(Enum.GetValues(typeof(ClientTaskStatusEnum)), task?.TaskStatus);
        }
    }
}
