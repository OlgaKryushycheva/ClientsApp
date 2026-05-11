using System.Diagnostics;
using ClientsApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClientsApp.Controllers
{
    // Контролер стартового блоку: відкриває головну сторінку, сторінку приватності та екран помилки.
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Показує головну (стартову) сторінку застосунку, яку користувач бачить після відкриття сайту.
        // Обмежень доступу немає: атрибути Authorize відсутні, тому сторінка доступна всім відвідувачам.
        public IActionResult Index()
        {
            return View();
        }

        // Відкриває сторінку Privacy з політикою конфіденційності для всіх користувачів, включно з неавторизованими.
        public IActionResult Privacy()
        {
            return View();
        }

        // Екран помилки не кешуємо,
        // щоб після повторної спроби користувач не бачив застарілий стан аварійної сторінки.
        // Duration = 0 вимикає збереження на час, Location = None забороняє кеші браузера/проксі,
        // NoStore = true забороняє взагалі записувати відповідь у кеш.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // RequestId передаємо у модель помилки, щоб користувач або підтримка могли звірити
            // конкретний збій із записом у логах HomeController.
            // Якщо Activity.Current недоступний, беремо HttpContext.TraceIdentifier
            // як стабільний ідентифікатор поточного HTTP-запиту.
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
