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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClientsApp.Controllers
{
    // Доступ до цього контролера потрібен лише авторизованим користувачам,
    // оскільки тут зведені фінансові показники: вартість робіт, оплати та борги клієнтів.
    [Authorize]
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
            // Завантажуємо повні набори даних для аналітики:
            // клієнтів, задач, оплат, відпрацьованих годин виконавців і довідник виконавців.
            var clients = (await _clientService.GetAllAsync()).OrderBy(c => c.Name).ToList();
            var tasks = (await _clientTaskService.GetAllAsync()).ToList();
            var payments = (await _paymentService.GetAllAsync()).ToList();
            var executorTasks = (await _executorTaskService.GetAllAsync()).ToList();
            var executors = (await _executorService.GetAllAsync()).ToList();

            // Готуємо ViewModel для сторінки статистики:
            // випадаючі списки для фільтрів та агреговані блоки для таблиць звітності.
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
                // Розраховуємо деталізовану статистику тільки для конкретної пари
                // "клієнт + завдання", яку обрав користувач у фільтрах.
                model.TaskStatistics = BuildTaskStatistics(selectedClientIdForTask.Value, selectedTaskId.Value, tasks, payments);
            }

            if (selectedClientIdForClient.HasValue)
            {
                // Формуємо зведення по всіх задачах вибраного клієнта:
                // сума робіт, оплати, борг і перелік задач без нарахованої вартості.
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
            // Select перетворює записи клієнтів у пункти списку для UI,
            // а прапорець Selected зберігає вибір користувача після перезавантаження сторінки.
            var items = clients
                .Select(c => new SelectListItem
                {
                    Value = c.ClientId.ToString(),
                    Text = c.Name,
                    Selected = selectedId.HasValue && c.ClientId == selectedId.Value
                })
                // OrderBy сортує клієнтів за назвою, щоб у фільтрі їх легше було знаходити вручну.
                .OrderBy(i => i.Text)
                .ToList();

            // Додаємо стартовий пункт "Оберіть клієнта",
            // якщо жоден клієнт ще не обраний у фільтрі.
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
                // Без вибраного клієнта не показуємо повний список задач,
                // щоб не змішувати завдання різних клієнтів в одному фільтрі.
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

            // Where відбирає тільки задачі вибраного клієнта,
            // далі OrderBy впорядковує їх за назвою, а Select готує елементи для випадаючого списку.
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
                // Якщо у клієнта ще немає задач, показуємо пояснювальний пункт замість порожнього списку.
                taskItems.Add(new SelectListItem
                {
                    Value = string.Empty,
                    Text = "Немає завдань для обраного клієнта",
                    Selected = true
                });
            }
            else
            {
                // Додаємо окремий пункт-підказку, щоб користувач явно обрав одну задачу для точкового звіту.
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
            // Беремо саме ту задачу, яка одночасно належить обраному клієнту
            // і має вибраний ідентифікатор завдання, щоб уникнути підміни даних між клієнтами.
            var task = tasks.FirstOrDefault(t => t.ClientTaskId == taskId && t.ClientId == clientId);
            if (task is null)
            {
                return null;
            }

            var taskCost = CalculateTaskCost(task);
            // Sum підсумовує всі платежі, прив'язані до цієї задачі,
            // щоб отримати фактично сплачену суму незалежно від кількості транзакцій.
            var totalPayments = payments.Where(p => p.ClientTaskId == task.ClientTaskId).Sum(p => p.Amount);

            // Формуємо модель для картки задачі і DOCX-звіту:
            // вартість, сплати та залишок боргу по конкретному завданню.
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

            // Вибираємо всі задачі клієнта для загального клієнтського звіту.
            var clientTasks = tasks.Where(t => t.ClientId == clientId).ToList();
            // Select перетворює кожну задачу в рядок майбутньої таблиці:
            // скільки коштує робота, скільки вже оплачено і який залишок до сплати.
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
            // У борг включаємо лише позитивний залишок,
            // щоб переплати не зменшували борг інших задач у загальному підсумку.
            var totalDebt = taskDetails.Sum(t => t.BalanceDue > 0 ? t.BalanceDue : 0);
            // Відбираємо задачі з нульовою вартістю, щоб підсвітити роботи,
            // де ще не внесено години/ставки і неможливо коректно виставити рахунок.
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
                // Для кожного виконавця збираємо його записи часу,
                // щоб окремо порахувати персональне навантаження і ефективність.
                var tasksForExecutor = executorTasks.Where(et => et.ExecutorId == executor.ExecutorId).ToList();
                if (!tasksForExecutor.Any())
                {
                    continue;
                }

                // Sum агрегує фактичні години та скориговані години по всіх задачах виконавця.
                var totalActual = tasksForExecutor.Sum(et => et.ActualTime);
                var totalAdjusted = tasksForExecutor.Sum(et => et.AdjustedTime);

                if (totalActual == 0 && totalAdjusted == 0)
                {
                    // Пропускаємо виконавців без жодного часу в статистиці,
                    // щоб таблиця містила лише релевантні дані.
                    continue;
                }

                decimal? ratio = null;
                if (totalActual > 0)
                {
                    // Коефіцієнт показує співвідношення скоригованого часу до фактичного:
                    // значення >1 означає збільшення трудомісткості після коригування.
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
                // Спочатку виводимо виконавців з найвищим коефіцієнтом,
                // а за однакового значення — сортуємо за ПІБ для стабільного порядку в таблиці.
                .OrderByDescending(p => p.Ratio ?? decimal.MinValue)
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
                    // У звіт боргів додаємо лише задачі з позитивним залишком:
                    // саме вони формують дебіторську заборгованість компанії.
                    debts.Add(new DebtStatisticsItemViewModel
                    {
                        ClientName = task.Client?.Name ?? "Невідомий клієнт",
                        TaskTitle = task.TaskTitle,
                        BalanceDue = balance
                    });
                }
            }

            debts = debts
                // Сортування за сумою боргу показує найкритичніші задачі першими,
                // а додаткове сортування за клієнтом/назвою стабілізує порядок у документі.
                .OrderByDescending(d => d.BalanceDue)
                .ThenBy(d => d.ClientName)
                .ThenBy(d => d.TaskTitle)
                .ToList();

            // Загальний борг — підсумок усіх рядків таблиці заборгованостей.
            totalDebt = debts.Sum(d => d.BalanceDue);
            return debts;
        }

        private static decimal CalculateTaskCost(ClientTask task)
        {
            // Вартість задачі = сума по всіх залучених виконавцях:
            // скориговані години * погодинна ставка виконавця.
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
                    // Якщо боргів немає, фіксуємо це окремим рядком у звіті,
                    // щоб документ однозначно підтверджував нульову заборгованість.
                    body.Append(CreateParagraph("Заборгованості відсутні."));
                }
                else
                {
                    // У звіті відображаємо деталізацію по кожній задачі
                    // та фінальний підсумок боргу по всіх клієнтах.
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
