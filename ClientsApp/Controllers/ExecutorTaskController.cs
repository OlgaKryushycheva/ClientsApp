using ClientsApp.BLL.Interfaces;
using ClientsApp.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClientsApp.Controllers
{
    // Доступ до призначень задач закритий для анонімних користувачів:
    // у таблиці видно зв’язки "виконавець ↔ клієнт ↔ конкретна задача" та фактичний час виконання.
    [Authorize]
    public class ExecutorTaskController : Controller
    {
        private readonly IExecutorTaskService _executorTaskService;
        private readonly IExecutorService _executorService;
        private readonly IClientService _clientService;
        private readonly IClientTaskService _clientTaskService;
        private readonly UserManager<Models.ApplicationUser> _userManager;

        public ExecutorTaskController(
            IExecutorTaskService executorTaskService,
            IExecutorService executorService,
            IClientService clientService,
            IClientTaskService clientTaskService,
            UserManager<Models.ApplicationUser> userManager)
        {
            _executorTaskService = executorTaskService;
            _executorService = executorService;
            _clientService = clientService;
            _clientTaskService = clientTaskService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? executorId, int? clientId, int? taskId)
        {
            // ViewBag + SelectList потрібні для фільтрів у верхній частині сторінки:
            // зберігаємо вибрані executorId/clientId/taskId, щоб після перезавантаження користувач бачив,
            // за яким саме виконавцем, клієнтом і задачею зараз відфільтровано список призначень.
            ViewBag.Executors = new SelectList(await _executorService.GetAllAsync(), "ExecutorId", "FullName", executorId);
            ViewBag.Clients = new SelectList(await _clientService.GetAllAsync(), "ClientId", "Name", clientId);
            ViewBag.Tasks = new SelectList(await _clientTaskService.GetAllAsync(), "ClientTaskId", "TaskTitle", taskId);

            // LINQ-фільтрація виконується в сервісі за переданими nullable-ідентифікаторами:
            // null означає "не застосовувати цей фільтр", значення означає "залишити тільки відповідні записи".
            var items = await _executorTaskService.GetAllAsync(executorId, clientId, taskId);
            return View(items);
        }

        // Створювати нове призначення може лише Manager, бо саме він розподіляє задачі між виконавцями
        // і відповідає за коректну пару ExecutorId + ClientTaskId.
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Create()
        {
            // Передаємо списки в ViewBag для каскадних випадаючих полів у формі призначення.
            ViewBag.Executors = await _executorService.GetAllAsync();
            ViewBag.Clients = new SelectList(await _clientService.GetAllAsync(), "ClientId", "Name");
            // На старті список задач порожній: він заповнюється після вибору виконавця/клієнта через JSON-метод.
            ViewBag.Tasks = new SelectList(Enumerable.Empty<ClientTask>(), "ClientTaskId", "TaskTitle");
            return View(new ExecutorTask());
        }

        [HttpPost]
        // Anti-forgery-токен захищає створення призначення від підробленого POST-запиту від імені менеджера.
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Create(ExecutorTask executorTask)
        {
            if (ModelState.IsValid)
            {
                // Фактичне призначення задачі виконавцю: зберігаємо зв’язок ExecutorId + ClientTaskId (+ заповнений ClientId у формі).
                await _executorTaskService.AddAsync(executorTask);
                // Повертаємо менеджера до реєстру призначень, щоб він одразу бачив новий розподіл.
                return RedirectToAction(nameof(Index));
            }

            // Якщо є помилки валідації, повторно наповнюємо всі випадаючі списки, інакше форма втратить контекст вибору.
            ViewBag.Executors = await _executorService.GetAllAsync();
            ViewBag.Clients = new SelectList(await _clientService.GetAllAsync(), "ClientId", "Name", executorTask.ClientId);
            var tasks = await _clientTaskService.GetAllAsync();
            // Залишаємо в списку тільки задачі обраного клієнта, щоб не допустити крос-клієнтського призначення.
            ViewBag.Tasks = new SelectList(tasks.Where(t => t.ClientId == executorTask.ClientId), "ClientTaskId", "TaskTitle", executorTask.ClientTaskId);
            return View(executorTask);
        }

        // Manager редагує будь-яке призначення, Executor — тільки свою задачу і тільки фактичний час.
        [Authorize(Roles = "Manager,Executor")]
        public async Task<IActionResult> Edit(int id)
        {
            var executorTask = await _executorTaskService.GetByIdAsync(id);
            // Якщо запису призначення з цим id не існує, повертаємо 404 замість порожньої форми.
            if (executorTask == null) return NotFound();

            // Гілка виконавця: він не перерозподіляє задачі, а лише звітує свій ActualTime.
            if (User.IsInRole("Executor"))
            {
                // Перевіряємо, що поточний виконавець намагається редагувати саме свою задачу.
                if (!await CanExecutorEditTaskAsync(executorTask))
                {
                    // Забороняємо доступ, якщо id призначення належить іншому виконавцю.
                    return Forbid();
                }

                // Для виконавця відкриваємо окрему форму з єдиним редагованим полем ActualTime.
                return View("EditActualTime", executorTask);
            }

            // Гілка менеджера: він може змінювати виконавця, клієнта і саму задачу в межах призначення.
            ViewBag.Executors = await _executorService.GetAllAsync();
            // Клієнтів показуємо тільки тих, з якими дозволено працювати вибраному ExecutorId.
            ViewBag.Clients = await _executorTaskService.GetClientsByExecutorAsync(executorTask.ExecutorId ?? 0);
            // Визначаємо поточного клієнта з навігаційної властивості задачі; fallback 0 покриває nullable випадок.
            var clientId = executorTask.ClientTask?.ClientId ?? 0;
            executorTask.ClientId = clientId;
            // Показуємо тільки задачі конкретної пари (ExecutorId, ClientId), щоб уникнути невалідних комбінацій призначення.
            ViewBag.Tasks = await _executorTaskService.GetTasksByExecutorAndClientAsync(executorTask.ExecutorId ?? 0, clientId);
            ViewBag.CurrentClientId = clientId;
            return View(executorTask);
        }

        [Authorize(Roles = "Manager,Executor")]
        [HttpPost]
        // Anti-forgery-токен обов’язковий, бо тут змінюються дані призначення/фактичного часу.
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ExecutorTask executorTask)
        {
            if (User.IsInRole("Executor"))
            {
                // Перечитуємо запис із БД, щоб виконавець не зміг підмінити ExecutorId/ClientId/ClientTaskId через форму.
                var existingTask = await _executorTaskService.GetByIdAsync(executorTask.ExecutorTaskId);
                if (existingTask == null)
                {
                    return NotFound();
                }

                // Захист від редагування чужих призначень навіть при ручній зміні URL або payload.
                if (!await CanExecutorEditTaskAsync(existingTask))
                {
                    return Forbid();
                }

                // Виконавець змінює тільки ActualTime: перерозподіл задач залишається виключно за менеджером.
                existingTask.ActualTime = executorTask.ActualTime;
                await _executorTaskService.UpdateAsync(existingTask);
                // Після оновлення часу повертаємо виконавця на домашню сторінку з його робочим списком.
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                // Менеджер має повний доступ до редагування полів призначення, тому оновлюємо весь об’єкт.
                await _executorTaskService.UpdateAsync(executorTask);
                return RedirectToAction(nameof(Index));
            }

            // При помилках валідації знову готуємо залежні списки для поточної пари виконавець/клієнт.
            ViewBag.Executors = await _executorService.GetAllAsync();
            ViewBag.Clients = await _executorTaskService.GetClientsByExecutorAsync(executorTask.ExecutorId ?? 0);
            // Якщо ClientId не прийшов із форми, дістаємо його з наявного запису; це покриває nullable сценарій.
            var clientId = executorTask.ClientId ?? (await _executorTaskService.GetByIdAsync(executorTask.ExecutorTaskId))?.ClientTask?.ClientId ?? 0;
            ViewBag.Tasks = await _executorTaskService.GetTasksByExecutorAndClientAsync(executorTask.ExecutorId ?? 0, clientId);
            ViewBag.CurrentClientId = clientId;
            return View(executorTask);
        }

        // Видалення призначення доступне тільки менеджеру, бо це впливає на розподіл навантаження між виконавцями.
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var executorTask = await _executorTaskService.GetByIdAsync(id);
            // Якщо менеджер відкрив неіснуючий id — повертаємо 404.
            if (executorTask == null) return NotFound();
            return View(executorTask);
        }

        [HttpPost, ActionName("Delete")]
        // Anti-forgery-токен не дає виконати видалення підробленою формою зі стороннього сайту.
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _executorTaskService.DeleteAsync(id);
            // Після видалення повертаємось до загального списку, щоб менеджер бачив актуальний розподіл.
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetClientsByExecutor(int executorId)
        {
            // JSON для залежного dropdown: після вибору ExecutorId фронтенд отримує тільки клієнтів цього виконавця
            // і не пропонує менеджеру несумісні варіанти призначення.
            var clients = await _executorTaskService.GetClientsByExecutorAsync(executorId);
            return Json(clients.Select(c => new { c.ClientId, c.Name }));
        }

        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetTasksByExecutorClient(int executorId, int clientId)
        {
            // Другий крок каскадного вибору: повертаємо задачі лише для пари (executorId, clientId),
            // щоб у списку задач залишалися тільки ті, які реально можна закріпити за цим виконавцем.
            var tasks = await _executorTaskService.GetTasksByExecutorAndClientAsync(executorId, clientId);
            return Json(tasks.Select(t => new { t.ClientTaskId, t.TaskTitle }));
        }

        private async Task<bool> CanExecutorEditTaskAsync(ExecutorTask executorTask)
        {
            // UserManager читає поточного користувача з контексту авторизації і дає доступ до його ExecutorId.
            var user = await _userManager.GetUserAsync(User);
            // Якщо користувач не знайдений або до акаунта не прив’язаний ExecutorId, редагування забороняємо.
            if (user == null || !user.ExecutorId.HasValue)
            {
                return false;
            }

            // Дозволяємо редагування тільки коли ExecutorId в призначенні збігається з ExecutorId поточного користувача.
            return executorTask.ExecutorId == user.ExecutorId.Value;
        }
    }
}
