using ClientsApp.BLL.Interfaces;
using ClientsApp.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientsApp.Controllers
{
    [Authorize]
    public class ClientController : Controller
    {
        private readonly IClientService _clientService;

        public ClientController(IClientService clientService)
        {
            _clientService = clientService;
        }

        public async Task<IActionResult> Index(string searchString, string? sortBy, string? sortDirection)
        {
            IEnumerable<Client> clients;
            var hasSearch = !string.IsNullOrWhiteSpace(searchString) && searchString.Length >= 3;
            clients = hasSearch
                ? await _clientService.SearchByNameAsync(searchString)
                : await _clientService.GetAllAsync();

            var normalizedSortBy = string.IsNullOrWhiteSpace(sortBy) ? "id" : sortBy.ToLowerInvariant();
            if (normalizedSortBy != "name" && normalizedSortBy != "id")
            {
                normalizedSortBy = "id";
            }

            var normalizedSortDirection = string.IsNullOrWhiteSpace(sortDirection)
                ? "asc"
                : sortDirection.ToLowerInvariant();
            if (normalizedSortDirection != "asc" && normalizedSortDirection != "desc")
            {
                normalizedSortDirection = "asc";
            }

            clients = normalizedSortBy switch
            {
                "name" when normalizedSortDirection == "desc" => clients.OrderByDescending(c => c.Name),
                "name" => clients.OrderBy(c => c.Name),
                "id" when normalizedSortDirection == "desc" => clients.OrderByDescending(c => c.ClientId),
                _ => clients.OrderBy(c => c.ClientId)
            };

            ViewData["SearchString"] = searchString;
            ViewData["SortBy"] = normalizedSortBy;
            ViewData["SortDirection"] = normalizedSortDirection;
            return View(clients);
        }

        [Authorize(Roles = "Manager")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Create(Client client)
        {
            if (!ModelState.IsValid) return View(client);

            await _clientService.AddAsync(client);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(Client client)
        {
            if (!ModelState.IsValid) return View(client);

            await _clientService.UpdateAsync(client);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _clientService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
