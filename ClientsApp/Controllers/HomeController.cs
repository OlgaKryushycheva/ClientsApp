// Контролер HomeController обробляє HTTP-запити цього розділу UI.
// Дії нижче читають параметри запиту, викликають сервіси й повертають View/Redirect/JSON.
using System.Diagnostics;
using ClientsApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClientsApp.Controllers
{
// HomeController: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

// Метод Index реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
        public IActionResult Index()
        {
            return View();
        }

// Метод Privacy реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
// Метод Error реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
