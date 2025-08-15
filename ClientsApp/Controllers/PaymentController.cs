using ClientsApp.BLL.Interfaces;
using ClientsApp.Models;
using ClientsApp.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ClientsApp.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public async Task<IActionResult> Index()
        {
            var payments = await _paymentService.GetAllAsync();
            return View(payments);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Payment payment)
        {
            if (!ModelState.IsValid) return View(payment);
            await _paymentService.AddAsync(payment);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var payment = await _paymentService.GetByIdAsync(id);
            if (payment == null) return NotFound();
            return View(payment);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Payment payment)
        {
            if (!ModelState.IsValid) return View(payment);
            await _paymentService.UpdateAsync(payment);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var payment = await _paymentService.GetByIdAsync(id);
            if (payment == null) return NotFound();
            await _paymentService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
