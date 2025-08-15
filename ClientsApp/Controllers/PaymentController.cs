using ClientsApp.BLL.Interfaces;
using ClientsApp.Models.Entities;
using ClientsApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ClientsApp.Controllers
{
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

        public async Task<IActionResult> Index(int? clientId, bool? isPaid)
        {
            var tasks = await _taskService.GetAllAsync();
            var payments = await _paymentService.GetAllAsync();

            var summaries = tasks.Select(t =>
            {
                var cost = t.ExecutorTasks.Sum(et => et.AdjustedTime * (et.Executor?.HourlyRate ?? 0));
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

            if (clientId.HasValue)
                summaries = summaries.Where(s => s.ClientId == clientId.Value);

            if (isPaid.HasValue)
                summaries = isPaid.Value
                    ? summaries.Where(s => s.BalanceDue <= 0)
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
                SelectedClientId = clientId,
                IsPaid = isPaid
            };

            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateTasks();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Payment payment)
        {
            if (!ModelState.IsValid)
            {
                await PopulateTasks();
                return View(payment);
            }

            payment.PaymentDate = DateTime.Now;
            payment.BalanceDue = await CalculateBalanceDue(payment.ClientTaskId, payment.Amount, null);
            await _paymentService.AddAsync(payment);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var payment = await _paymentService.GetByIdAsync(id);
            if (payment == null) return NotFound();
            await PopulateTasks();
            return View(payment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Payment payment)
        {
            if (!ModelState.IsValid)
            {
                await PopulateTasks();
                return View(payment);
            }

            payment.BalanceDue = await CalculateBalanceDue(payment.ClientTaskId, payment.Amount, payment.PaymentId);
            await _paymentService.UpdateAsync(payment);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var payment = await _paymentService.GetByIdAsync(id);
            if (payment == null) return NotFound();
            return View(payment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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
            var paid = payments.Where(p => p.ClientTaskId == taskId && p.PaymentId != currentPaymentId).Sum(p => p.Amount);
            return cost - (paid + newAmount);
        }
    }
}
