using ClientsApp.BLL.Interfaces;
using ClientsApp.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ClientsApp.Controllers
{
    public class ClientController : Controller
    {
        private readonly IClientService _clientService;

        public ClientController(IClientService clientService)
        {
            _clientService = clientService;
        }

        public async Task<IActionResult> Index()
        {
            var clients = await _clientService.GetAllAsync();
            return View(clients);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (!ModelState.IsValid) return View(client);

            await _clientService.AddAsync(client);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Client client)
        {
            if (!ModelState.IsValid) return View(client);

            await _clientService.UpdateAsync(client);
            return RedirectToAction(nameof(Index));
        }

        //public async Task<IActionResult> Delete(int id)
        //{
        //    var client = await _clientService.GetByIdAsync(id);
        //    if (client == null) return NotFound();

        //    await _clientService.DeleteAsync(id);
        //    return RedirectToAction(nameof(Index));
        //}


        // GET: показати сторінку підтвердження видалення
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null) return NotFound();

            return View(client); // повертає Delete.cshtml
        }

        // POST: власне видалення після підтвердження
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _clientService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }



    }
}
