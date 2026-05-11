using ClientsApp.BLL.Interfaces;
using ClientsApp.Models.Entities;
using ClientsApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClientsApp.Controllers
{
    // Платежі містять чутливі фінансові дані по задачах клієнтів,
    // тому доступ до контролера є тільки в авторизованих користувачів.
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IClientService _clientService;
        private readonly IClientTaskService _taskService;

        public PaymentController(IPaymentService paymentService,
                                 IClientService clientService,
                                 IClientTaskService taskService)
        {
            _paymentService = paymentService;
            _clientService = clientService;
            _taskService = taskService;
        }

        public async Task<IActionResult> Index(int? selectedClientId, bool? isPaid)
        {
            var tasks = await _taskService.GetAllAsync();
            var payments = await _paymentService.GetAllAsync();

            // Для кожної задачі рахуємо фінансове зведення: повну вартість, уже отриману суму та залишок.
            var summaries = tasks.Select(t =>
            {
                // Вартість задачі базується на фактично витраченому часі виконавців:
                // AdjustedTime * HourlyRate для кожного запису в ExecutorTasks.
                // Якщо ставка виконавця відсутня, підставляємо 0, щоб не зламати обчислення.
                var cost = t.ExecutorTasks.Sum(et => et.AdjustedTime * (et.Executor?.HourlyRate ?? 0));

                // Беремо тільки ті платежі, що належать цій задачі (Where по ClientTaskId),
                // і підсумовуємо Amount через Sum, щоб отримати суму фактичних оплат клієнта.
                var received = payments.Where(p => p.ClientTaskId == t.ClientTaskId).Sum(p => p.Amount);
                return new PaymentSummaryViewModel
                {
                    ClientTaskId = t.ClientTaskId,
                    // Якщо ClientId порожній (nullable), ставимо 0 для стабільної роботи фільтра в UI.
                    ClientId = t.ClientId ?? 0,
                    // Якщо клієнт не підтягнувся з навігаційної властивості, показуємо порожній рядок замість null.
                    ClientName = t.Client?.Name ?? string.Empty,
                    TaskTitle = t.TaskTitle,
                    ServiceCost = cost,
                    AmountReceived = received,
                    // BalanceDue — це борг клієнта по задачі на поточний момент.
                    BalanceDue = cost - received
                };
            });

            // Фільтр по конкретному клієнту застосовуємо лише коли selectedClientId реально передано з форми.
            if (selectedClientId.HasValue)
                summaries = summaries.Where(s => s.ClientId == selectedClientId.Value);

            // Фільтр оплати працює по залишку боргу:
            // IsPaid=true показує задачі без боргу (BalanceDue <= 0),
            // IsPaid=false показує задачі, де ще є сума до сплати (BalanceDue > 0).
            if (isPaid.HasValue)
                summaries = isPaid.Value
                    ? summaries.Where(s => s.BalanceDue <= 0)
                    : summaries.Where(s => s.BalanceDue > 0);

            var clients = await _clientService.GetAllAsync();
            var model = new PaymentIndexViewModel
            {
                // PaymentIndexViewModel одночасно несе і таблицю оплат, і налаштування фільтрів для цієї сторінки.
                Payments = summaries.ToList(),
                // SelectListItem формує пари Value/Text для dropdown клієнтів у фільтрі.
                Clients = clients.Select(c => new SelectListItem
                {
                    Value = c.ClientId.ToString(),
                    Text = c.Name
                }).ToList(),
                // Повертаємо обрані фільтри назад у ViewModel,
                // щоб після перезавантаження сторінки користувач бачив актуальний стан форми.
                SelectedClientId = selectedClientId,
                IsPaid = isPaid
            };

            return View(model);
        }

        // Бухгалтер може реєструвати надходження коштів,
        // а менеджер має той самий доступ плюс подальше редагування/видалення в інших діях.
        [Authorize(Roles = "Manager,Accountant")]
        public async Task<IActionResult> Create()
        {
            // Готуємо список задач перед відкриттям форми,
            // щоб платіж одразу можна було прив'язати до потрібної клієнтської роботи.
            await PopulateTasks();
            return View();
        }

        [Authorize(Roles = "Manager,Accountant")]
        [HttpPost]
        // Перевіряє, що форма створення платежу відправлена з легітимної сторінки застосунку,
        // а не стороннім запитом.
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Payment payment)
        {
            // Якщо у формі є помилки, повертаємо її з тими ж даними та знову заповнюємо dropdown задач.
            if (!ModelState.IsValid)
            {
                await PopulateTasks();
                return View(payment);
            }

            // Дату платежу проставляємо на сервері автоматично,
            // щоб користувач не міг заднім числом або наперед змінити фінансову хронологію.
            payment.PaymentDate = DateTime.Now;
            // Після додавання нового платежу одразу зберігаємо залишок боргу по задачі.
            payment.BalanceDue = await CalculateBalanceDue(payment.ClientTaskId, payment.Amount, null);
            await _paymentService.AddAsync(payment);
            if (User.IsInRole("Accountant"))
            {
                // Бухгалтера лишаємо на формі Create, щоб він міг швидко внести наступну оплату.
                return RedirectToAction("Create");
            }

            // Менеджера повертаємо до зведеного списку оплат по задачах.
            return RedirectToAction(nameof(Index));
        }

        // Редагування історичних платежів дозволено тільки менеджеру,
        // бо зміна суми впливає на борг задачі та фінансові звіти.
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var payment = await _paymentService.GetByIdAsync(id);
            // Якщо платежу з таким ID немає, показуємо 404 замість порожньої форми редагування.
            if (payment == null) return NotFound();
            await PopulateTasks();
            return View(payment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(Payment payment)
        {
            if (!ModelState.IsValid)
            {
                await PopulateTasks();
                return View(payment);
            }

            // При перерахунку боргу в Edit передаємо currentPaymentId,
            // щоб у CalculateBalanceDue виключити поточний платіж і не подвоїти його суму.
            payment.BalanceDue = await CalculateBalanceDue(payment.ClientTaskId, payment.Amount, payment.PaymentId);
            await _paymentService.UpdateAsync(payment);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        // Видалення платежу доступне лише менеджеру,
        // оскільки це змінює історію оплат та кінцевий баланс задачі.
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var payment = await _paymentService.GetByIdAsync(id);
            // Якщо запис уже видалений або ID некоректний — повертаємо 404.
            if (payment == null) return NotFound();
            return View(payment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _paymentService.DeleteAsync(id);
            // Після видалення платежу повертаємо користувача до загального списку оплат.
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateTasks()
        {
            var tasks = await _taskService.GetAllAsync();
            // Через ViewBag.Tasks передаємо у View dropdown задач:
            // SelectListItem зберігає ID задачі у Value та зрозумілий підпис "Клієнт - Назва задачі" у Text.
            ViewBag.Tasks = tasks.Select(t => new SelectListItem
            {
                Value = t.ClientTaskId.ToString(),
                Text = $"{t.Client?.Name} - {t.TaskTitle}"
            }).ToList();
        }

        private async Task<decimal> CalculateBalanceDue(int taskId, decimal newAmount, int? currentPaymentId)
        {
            var task = await _taskService.GetByIdAsync(taskId);
            // Розрахунок повної вартості задачі завжди йде від фактичної праці виконавців (ExecutorTasks)
            // та їх погодинних ставок (HourlyRate), щоб борг відповідав реальній собівартості послуги.
            var cost = task.ExecutorTasks.Sum(et => et.AdjustedTime * (et.Executor?.HourlyRate ?? 0));
            var payments = await _paymentService.GetAllAsync();
            // Беремо всі платежі по задачі, окрім поточного (для Edit),
            // далі додаємо нову суму newAmount і віднімаємо від загальної вартості.
            var paid = payments.Where(p => p.ClientTaskId == taskId && p.PaymentId != currentPaymentId).Sum(p => p.Amount);
            return cost - (paid + newAmount);
        }
    }
}
