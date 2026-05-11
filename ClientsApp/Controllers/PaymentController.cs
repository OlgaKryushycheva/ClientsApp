// Контролер PaymentController обробляє HTTP-запити цього розділу UI.
// Дії нижче читають параметри запиту, викликають сервіси й повертають View/Redirect/JSON.
using ClientsApp.BLL.Interfaces;
using ClientsApp.Models.Entities;
using ClientsApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClientsApp.Controllers
{
// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
    [Authorize]
// PaymentController: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
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

// Метод Index реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int? selectedClientId, bool? isPaid.
        public async Task<IActionResult> Index(int? selectedClientId, bool? isPaid)
        {
            var tasks = await _taskService.GetAllAsync();
            var payments = await _paymentService.GetAllAsync();

            var summaries = tasks.Select(t =>
            {
                var cost = t.ExecutorTasks.Sum(et => et.AdjustedTime * (et.Executor?.HourlyRate ?? 0));
// Фільтр Where залишає лише записи, що відповідають умові, тому у View не потрапляють зайві дані.
                var received = payments.Where(p => p.ClientTaskId == t.ClientTaskId).Sum(p => p.Amount);
                return new PaymentSummaryViewModel
                {
                    ClientTaskId = t.ClientTaskId,
                    ClientId = t.ClientId ?? 0,
                    ClientName = t.Client?.Name ?? string.Empty,
                    TaskTitle = t.TaskTitle,
                    ServiceCost = cost,
                    AmountReceived = received,
                    BalanceDue = cost - received
                };
            });

// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (selectedClientId.HasValue)
// Фільтр Where залишає лише записи, що відповідають умові, тому у View не потрапляють зайві дані.
                summaries = summaries.Where(s => s.ClientId == selectedClientId.Value);

// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (isPaid.HasValue)
                summaries = isPaid.Value
// Фільтр Where залишає лише записи, що відповідають умові, тому у View не потрапляють зайві дані.
                    ? summaries.Where(s => s.BalanceDue <= 0)
// Фільтр Where залишає лише записи, що відповідають умові, тому у View не потрапляють зайві дані.
                    : summaries.Where(s => s.BalanceDue > 0);

            var clients = await _clientService.GetAllAsync();
            var model = new PaymentIndexViewModel
            {
                Payments = summaries.ToList(),
                Clients = clients.Select(c => new SelectListItem
                {
                    Value = c.ClientId.ToString(),
                    Text = c.Name
                }).ToList(),
                SelectedClientId = selectedClientId,
                IsPaid = isPaid
            };

            return View(model);
        }

// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager,Accountant")]
// Метод Create реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
        public async Task<IActionResult> Create()
        {
            await PopulateTasks();
            return View();
        }

// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager,Accountant")]
// HTTP POST приймає дані форми та запускає операцію створення/оновлення/видалення.
        [HttpPost]
// Anti-forgery токен блокує CSRF: сторонній сайт не зможе відправити форму від імені користувача.
        [ValidateAntiForgeryToken]
// Метод Create реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: Payment payment.
        public async Task<IActionResult> Create(Payment payment)
        {
// Якщо валідація моделі не пройдена, зупиняємо запис у БД і повертаємо форму з помилками користувачу.
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (!ModelState.IsValid)
            {
                await PopulateTasks();
                return View(payment);
            }

            payment.PaymentDate = DateTime.Now;
            payment.BalanceDue = await CalculateBalanceDue(payment.ClientTaskId, payment.Amount, null);
            await _paymentService.AddAsync(payment);
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (User.IsInRole("Accountant"))
            {
                return RedirectToAction("Create");
            }

            return RedirectToAction(nameof(Index));
        }

// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager")]
// Метод Edit реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int id.
        public async Task<IActionResult> Edit(int id)
        {
            var payment = await _paymentService.GetByIdAsync(id);
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (payment == null) return NotFound();
            await PopulateTasks();
            return View(payment);
        }

// HTTP POST приймає дані форми та запускає операцію створення/оновлення/видалення.
        [HttpPost]
// Anti-forgery токен блокує CSRF: сторонній сайт не зможе відправити форму від імені користувача.
        [ValidateAntiForgeryToken]
// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager")]
// Метод Edit реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: Payment payment.
        public async Task<IActionResult> Edit(Payment payment)
        {
// Якщо валідація моделі не пройдена, зупиняємо запис у БД і повертаємо форму з помилками користувачу.
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (!ModelState.IsValid)
            {
                await PopulateTasks();
                return View(payment);
            }

            payment.BalanceDue = await CalculateBalanceDue(payment.ClientTaskId, payment.Amount, payment.PaymentId);
            await _paymentService.UpdateAsync(payment);
            return RedirectToAction(nameof(Index));
        }

// HTTP GET використовується для читання даних і відкриття сторінки без зміни стану БД.
        [HttpGet]
// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager")]
// Метод Delete реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int id.
        public async Task<IActionResult> Delete(int id)
        {
            var payment = await _paymentService.GetByIdAsync(id);
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (payment == null) return NotFound();
            return View(payment);
        }

// HTTP POST приймає дані форми та запускає операцію створення/оновлення/видалення.
        [HttpPost, ActionName("Delete")]
// Anti-forgery токен блокує CSRF: сторонній сайт не зможе відправити форму від імені користувача.
        [ValidateAntiForgeryToken]
// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager")]
// Метод DeleteConfirmed реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int id.
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _paymentService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateTasks()
        {
            var tasks = await _taskService.GetAllAsync();
            ViewBag.Tasks = tasks.Select(t => new SelectListItem
            {
                Value = t.ClientTaskId.ToString(),
                Text = $"{t.Client?.Name} - {t.TaskTitle}"
            }).ToList();
        }

        private async Task<decimal> CalculateBalanceDue(int taskId, decimal newAmount, int? currentPaymentId)
        {
            var task = await _taskService.GetByIdAsync(taskId);
            var cost = task.ExecutorTasks.Sum(et => et.AdjustedTime * (et.Executor?.HourlyRate ?? 0));
            var payments = await _paymentService.GetAllAsync();
// Фільтр Where залишає лише записи, що відповідають умові, тому у View не потрапляють зайві дані.
            var paid = payments.Where(p => p.ClientTaskId == taskId && p.PaymentId != currentPaymentId).Sum(p => p.Amount);
            return cost - (paid + newAmount);
        }
    }
}
