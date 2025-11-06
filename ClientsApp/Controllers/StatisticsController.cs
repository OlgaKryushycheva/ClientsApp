using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClientsApp.BLL.Interfaces;
using ClientsApp.Models.Entities;
using ClientsApp.Models.ViewModels.Statistics;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClientsApp.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly IClientService _clientService;
        private readonly IClientTaskService _clientTaskService;
        private readonly IPaymentService _paymentService;
        private readonly IExecutorTaskService _executorTaskService;
        private readonly IExecutorService _executorService;
        private readonly CultureInfo _culture = new("uk-UA");

        public StatisticsController(
            IClientService clientService,
            IClientTaskService clientTaskService,
            IPaymentService paymentService,
            IExecutorTaskService executorTaskService,
            IExecutorService executorService)
        {
            _clientService = clientService;
            _clientTaskService = clientTaskService;
            _paymentService = paymentService;
            _executorTaskService = executorTaskService;
            _executorService = executorService;
        }

        public async Task<IActionResult> Index(int? selectedClientIdForTask, int? selectedTaskId, int? selectedClientIdForClient)
        {
            var clients = (await _clientService.GetAllAsync()).OrderBy(c => c.Name).ToList();
            var tasks = (await _clientTaskService.GetAllAsync()).ToList();
            var payments = (await _paymentService.GetAllAsync()).ToList();
            var executorTasks = (await _executorTaskService.GetAllAsync()).ToList();
            var executors = (await _executorService.GetAllAsync()).ToList();

            var model = new StatisticsIndexViewModel
            {
                SelectedClientIdForTask = selectedClientIdForTask,
                SelectedTaskId = selectedTaskId,
                SelectedClientIdForClient = selectedClientIdForClient,
                ClientOptionsForTaskStats = BuildClientSelectList(clients, selectedClientIdForTask),
                ClientOptionsForClientStats = BuildClientSelectList(clients, selectedClientIdForClient),
                TaskOptions = BuildTaskSelectList(tasks, selectedClientIdForTask, selectedTaskId),
                ExecutorPerformances = BuildExecutorPerformance(executorTasks, executors),
                DebtStatistics = BuildDebtStatistics(tasks, payments, out var totalDebt),
                TotalDebt = totalDebt
            };

            if (selectedClientIdForTask.HasValue && selectedTaskId.HasValue)
            {
                model.TaskStatistics = BuildTaskStatistics(selectedClientIdForTask.Value, selectedTaskId.Value, tasks, payments);
            }

            if (selectedClientIdForClient.HasValue)
            {
                model.ClientStatistics = BuildClientStatistics(selectedClientIdForClient.Value, clients, tasks, payments);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateTaskReport(int clientId, int taskId)
        {
            var tasks = (await _clientTaskService.GetAllAsync()).ToList();
            var payments = (await _paymentService.GetAllAsync()).ToList();
            var statistics = BuildTaskStatistics(clientId, taskId, tasks, payments);

            if (statistics is null)
            {
                return NotFound();
            }

            var fileBytes = CreateTaskReport(statistics);
            var fileName = $"TaskReport_{DateTime.Now:yyyyMMddHHmmss}.docx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateDebtReport()
        {
            var tasks = (await _clientTaskService.GetAllAsync()).ToList();
            var payments = (await _paymentService.GetAllAsync()).ToList();
            var debts = BuildDebtStatistics(tasks, payments, out var totalDebt);

            var fileBytes = CreateDebtReport(debts, totalDebt);
            var fileName = $"DebtReport_{DateTime.Now:yyyyMMddHHmmss}.docx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
        }

        private IEnumerable<SelectListItem> BuildClientSelectList(IEnumerable<Client> clients, int? selectedId)
        {
            var items = clients
                .Select(c => new SelectListItem
                {
                    Value = c.ClientId.ToString(),
                    Text = c.Name,
                    Selected = selectedId.HasValue && c.ClientId == selectedId.Value
                })
                .OrderBy(i => i.Text)
                .ToList();

            items.Insert(0, new SelectListItem
            {
                Value = string.Empty,
                Text = "Оберіть клієнта",
                Selected = !selectedId.HasValue
            });

            return items;
        }

        private IEnumerable<SelectListItem> BuildTaskSelectList(IEnumerable<ClientTask> tasks, int? clientId, int? selectedTaskId)
        {
            if (!clientId.HasValue)
            {
                return new List<SelectListItem>
                {
                    new()
                    {
                        Value = string.Empty,
                        Text = "Спочатку оберіть клієнта",
                        Selected = true
                    }
                };
            }

            var taskItems = tasks
                .Where(t => t.ClientId == clientId.Value)
                .OrderBy(t => t.TaskTitle)
                .Select(t => new SelectListItem
                {
                    Value = t.ClientTaskId.ToString(),
                    Text = t.TaskTitle,
                    Selected = selectedTaskId.HasValue && t.ClientTaskId == selectedTaskId.Value
                })
                .ToList();

            if (!taskItems.Any())
            {
                taskItems.Add(new SelectListItem
                {
                    Value = string.Empty,
                    Text = "Немає завдань для обраного клієнта",
                    Selected = true
                });
            }
            else
            {
                taskItems.Insert(0, new SelectListItem
                {
                    Value = string.Empty,
                    Text = "Оберіть завдання",
                    Selected = !selectedTaskId.HasValue
                });
            }

            return taskItems;
        }

        private TaskStatisticsViewModel? BuildTaskStatistics(int clientId, int taskId, IEnumerable<ClientTask> tasks, IEnumerable<Payment> payments)
        {
            var task = tasks.FirstOrDefault(t => t.ClientTaskId == taskId && t.ClientId == clientId);
            if (task is null)
            {
                return null;
            }

            var taskCost = CalculateTaskCost(task);
            var totalPayments = payments.Where(p => p.ClientTaskId == task.ClientTaskId).Sum(p => p.Amount);

            return new TaskStatisticsViewModel
            {
                ClientName = task.Client?.Name ?? string.Empty,
                TaskTitle = task.TaskTitle,
                TaskDescription = task.Description,
                TaskCost = taskCost,
                TotalPayments = totalPayments,
                BalanceDue = taskCost - totalPayments
            };
        }

        private ClientStatisticsViewModel? BuildClientStatistics(int clientId, IEnumerable<Client> clients, IEnumerable<ClientTask> tasks, IEnumerable<Payment> payments)
        {
            var client = clients.FirstOrDefault(c => c.ClientId == clientId);
            if (client is null)
            {
                return null;
            }

            var clientTasks = tasks.Where(t => t.ClientId == clientId).ToList();
            var taskDetails = clientTasks.Select(task =>
            {
                var cost = CalculateTaskCost(task);
                var paymentsSum = payments.Where(p => p.ClientTaskId == task.ClientTaskId).Sum(p => p.Amount);
                return new ClientTaskCostViewModel
                {
                    TaskTitle = task.TaskTitle,
                    TaskCost = cost,
                    Payments = paymentsSum,
                    BalanceDue = cost - paymentsSum
                };
            }).ToList();

            var totalCost = taskDetails.Sum(t => t.TaskCost);
            var totalPayments = taskDetails.Sum(t => t.Payments);
            var totalDebt = taskDetails.Sum(t => t.BalanceDue > 0 ? t.BalanceDue : 0);
            var tasksWithoutInvoice = taskDetails
                .Where(t => t.TaskCost == 0)
                .Select(t => t.TaskTitle)
                .ToList();

            return new ClientStatisticsViewModel
            {
                ClientName = client.Name,
                Tasks = taskDetails,
                TotalCost = totalCost,
                TotalPayments = totalPayments,
                TotalDebt = totalDebt,
                TasksWithoutInvoice = tasksWithoutInvoice
            };
        }

        private IList<ExecutorPerformanceViewModel> BuildExecutorPerformance(IEnumerable<ExecutorTask> executorTasks, IEnumerable<Executor> executors)
        {
            var performance = new List<ExecutorPerformanceViewModel>();

            foreach (var executor in executors)
            {
                var tasksForExecutor = executorTasks.Where(et => et.ExecutorId == executor.ExecutorId).ToList();
                if (!tasksForExecutor.Any())
                {
                    continue;
                }

                var totalActual = tasksForExecutor.Sum(et => et.ActualTime);
                var totalAdjusted = tasksForExecutor.Sum(et => et.AdjustedTime);

                if (totalActual == 0 && totalAdjusted == 0)
                {
                    continue;
                }

                decimal? ratio = null;
                if (totalActual > 0)
                {
                    ratio = totalAdjusted / totalActual;
                }

                performance.Add(new ExecutorPerformanceViewModel
                {
                    ExecutorName = executor.FullName,
                    TotalActualTime = totalActual,
                    TotalAdjustedTime = totalAdjusted,
                    Ratio = ratio
                });
            }

            return performance
                .OrderBy(p => p.Ratio ?? decimal.MaxValue)
                .ThenBy(p => p.ExecutorName)
                .ToList();
        }

        private IList<DebtStatisticsItemViewModel> BuildDebtStatistics(IEnumerable<ClientTask> tasks, IEnumerable<Payment> payments, out decimal totalDebt)
        {
            var debts = new List<DebtStatisticsItemViewModel>();

            foreach (var task in tasks)
            {
                var cost = CalculateTaskCost(task);
                var paid = payments.Where(p => p.ClientTaskId == task.ClientTaskId).Sum(p => p.Amount);
                var balance = cost - paid;

                if (balance > 0)
                {
                    debts.Add(new DebtStatisticsItemViewModel
                    {
                        ClientName = task.Client?.Name ?? "Невідомий клієнт",
                        TaskTitle = task.TaskTitle,
                        BalanceDue = balance
                    });
                }
            }

            debts = debts
                .OrderByDescending(d => d.BalanceDue)
                .ThenBy(d => d.ClientName)
                .ThenBy(d => d.TaskTitle)
                .ToList();

            totalDebt = debts.Sum(d => d.BalanceDue);
            return debts;
        }

        private static decimal CalculateTaskCost(ClientTask task)
        {
            return task.ExecutorTasks.Sum(et => et.AdjustedTime * (et.Executor?.HourlyRate ?? 0));
        }

        private byte[] CreateTaskReport(TaskStatisticsViewModel statistics)
        {
            using var stream = new MemoryStream();
            using (var wordDocument = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
            {
                var mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                body.Append(CreateParagraph("Звіт про виконання завдання", true));
                body.Append(CreateParagraph($"Дата формування: {DateTime.Now:dd.MM.yyyy HH:mm}"));
                body.Append(new Paragraph(new Run(new Text(string.Empty))));
                body.Append(CreateParagraph($"Клієнт: {statistics.ClientName}"));
                body.Append(CreateParagraph($"Завдання: {statistics.TaskTitle}"));
                body.Append(CreateParagraph($"Опис: {statistics.TaskDescription}"));
                body.Append(CreateParagraph($"Вартість завдання: {statistics.TaskCost.ToString("F2", _culture)} грн"));
                body.Append(CreateParagraph($"Сума оплат: {statistics.TotalPayments.ToString("F2", _culture)} грн"));
                body.Append(CreateParagraph($"Сума заборгованості: {statistics.BalanceDue.ToString("F2", _culture)} грн"));

                mainPart.Document.Save();
            }

            return stream.ToArray();
        }

        private byte[] CreateDebtReport(IEnumerable<DebtStatisticsItemViewModel> debts, decimal totalDebt)
        {
            using var stream = new MemoryStream();
            using (var wordDocument = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
            {
                var mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                body.Append(CreateParagraph("Звіт про заборгованість", true));
                body.Append(CreateParagraph($"Дата формування: {DateTime.Now:dd.MM.yyyy HH:mm}"));
                body.Append(new Paragraph(new Run(new Text(string.Empty))));

                if (!debts.Any())
                {
                    body.Append(CreateParagraph("Заборгованості відсутні."));
                }
                else
                {
                    body.Append(CreateParagraph("Детальна інформація:"));
                    body.Append(CreateDebtTable(debts));
                    body.Append(CreateParagraph($"Загальна сума заборгованості: {totalDebt.ToString("F2", _culture)} грн"));
                }

                mainPart.Document.Save();
            }

            return stream.ToArray();
        }

        private static Paragraph CreateParagraph(string text, bool isTitle = false)
        {
            var run = new Run();

            if (isTitle)
            {
                run.RunProperties = new RunProperties
                {
                    Bold = new Bold(),
                    FontSize = new FontSize { Val = "28" }
                };
            }

            run.Append(new Text(text ?? string.Empty) { Space = SpaceProcessingModeValues.Preserve });
            var paragraph = new Paragraph(run);

            if (isTitle)
            {
                paragraph.ParagraphProperties = new ParagraphProperties
                {
                    Justification = new Justification { Val = JustificationValues.Center }
                };
            }

            return paragraph;
        }

        private Table CreateDebtTable(IEnumerable<DebtStatisticsItemViewModel> debts)
        {
            var table = new Table();
            var tableProperties = new TableProperties(
                new TableBorders(
                    new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                    new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                    new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                    new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                    new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                    new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 }
                ));

            table.AppendChild(tableProperties);

            var headerRow = new TableRow(
                CreateHeaderCell("Клієнт"),
                CreateHeaderCell("Завдання"),
                CreateHeaderCell("Сума заборгованості"));
            table.Append(headerRow);

            foreach (var debt in debts)
            {
                table.Append(new TableRow(
                    CreateCell(debt.ClientName),
                    CreateCell(debt.TaskTitle),
                    CreateCell($"{debt.BalanceDue.ToString("F2", _culture)} грн")));
            }

            return table;
        }

        private static TableCell CreateCell(string text)
        {
            var run = new Run(new Text(text ?? string.Empty) { Space = SpaceProcessingModeValues.Preserve });
            return new TableCell(new Paragraph(run));
        }

        private static TableCell CreateHeaderCell(string text)
        {
            var run = new Run
            {
                RunProperties = new RunProperties(new Bold())
            };
            run.Append(new Text(text ?? string.Empty) { Space = SpaceProcessingModeValues.Preserve });
            return new TableCell(new Paragraph(run));
        }
    }
}
