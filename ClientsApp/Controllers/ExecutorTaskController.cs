// Контролер ExecutorTaskController обробляє HTTP-запити цього розділу UI.
// Дії нижче читають параметри запиту, викликають сервіси й повертають View/Redirect/JSON.
using ClientsApp.BLL.Interfaces;
using ClientsApp.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClientsApp.Controllers
{
// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
    [Authorize]
// ExecutorTaskController: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
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

// Метод Index реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int? executorId, int? clientId, int? taskId.
        public async Task<IActionResult> Index(int? executorId, int? clientId, int? taskId)
        {
            ViewBag.Executors = new SelectList(await _executorService.GetAllAsync(), "ExecutorId", "FullName", executorId);
            ViewBag.Clients = new SelectList(await _clientService.GetAllAsync(), "ClientId", "Name", clientId);
            ViewBag.Tasks = new SelectList(await _clientTaskService.GetAllAsync(), "ClientTaskId", "TaskTitle", taskId);

            var items = await _executorTaskService.GetAllAsync(executorId, clientId, taskId);
            return View(items);
        }

// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager")]
// Метод Create реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
        public async Task<IActionResult> Create()
        {
            ViewBag.Executors = await _executorService.GetAllAsync();
            ViewBag.Clients = new SelectList(await _clientService.GetAllAsync(), "ClientId", "Name");
            ViewBag.Tasks = new SelectList(Enumerable.Empty<ClientTask>(), "ClientTaskId", "TaskTitle");
            return View(new ExecutorTask());
        }

// HTTP POST приймає дані форми та запускає операцію створення/оновлення/видалення.
        [HttpPost]
// Anti-forgery токен блокує CSRF: сторонній сайт не зможе відправити форму від імені користувача.
        [ValidateAntiForgeryToken]
// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager")]
// Метод Create реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: ExecutorTask executorTask.
        public async Task<IActionResult> Create(ExecutorTask executorTask)
        {
// Якщо валідація моделі не пройдена, зупиняємо запис у БД і повертаємо форму з помилками користувачу.
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (ModelState.IsValid)
            {
                await _executorTaskService.AddAsync(executorTask);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Executors = await _executorService.GetAllAsync();
            ViewBag.Clients = new SelectList(await _clientService.GetAllAsync(), "ClientId", "Name", executorTask.ClientId);
            var tasks = await _clientTaskService.GetAllAsync();
// Фільтр Where залишає лише записи, що відповідають умові, тому у View не потрапляють зайві дані.
            ViewBag.Tasks = new SelectList(tasks.Where(t => t.ClientId == executorTask.ClientId), "ClientTaskId", "TaskTitle", executorTask.ClientTaskId);
            return View(executorTask);
        }

// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager,Executor")]
// Метод Edit реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int id.
        public async Task<IActionResult> Edit(int id)
        {
            var executorTask = await _executorTaskService.GetByIdAsync(id);
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (executorTask == null) return NotFound();

// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (User.IsInRole("Executor"))
            {
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
                if (!await CanExecutorEditTaskAsync(executorTask))
                {
                    return Forbid();
                }

                return View("EditActualTime", executorTask);
            }

            ViewBag.Executors = await _executorService.GetAllAsync();
            ViewBag.Clients = await _executorTaskService.GetClientsByExecutorAsync(executorTask.ExecutorId ?? 0);
            var clientId = executorTask.ClientTask?.ClientId ?? 0;
            executorTask.ClientId = clientId;
            ViewBag.Tasks = await _executorTaskService.GetTasksByExecutorAndClientAsync(executorTask.ExecutorId ?? 0, clientId);
            ViewBag.CurrentClientId = clientId;
            return View(executorTask);
        }

// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager,Executor")]
// HTTP POST приймає дані форми та запускає операцію створення/оновлення/видалення.
        [HttpPost]
// Anti-forgery токен блокує CSRF: сторонній сайт не зможе відправити форму від імені користувача.
        [ValidateAntiForgeryToken]
// Метод Edit реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: ExecutorTask executorTask.
        public async Task<IActionResult> Edit(ExecutorTask executorTask)
        {
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (User.IsInRole("Executor"))
            {
                var existingTask = await _executorTaskService.GetByIdAsync(executorTask.ExecutorTaskId);
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
                if (existingTask == null)
                {
                    return NotFound();
                }

// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
                if (!await CanExecutorEditTaskAsync(existingTask))
                {
                    return Forbid();
                }

                existingTask.ActualTime = executorTask.ActualTime;
                await _executorTaskService.UpdateAsync(existingTask);
                return RedirectToAction("Index", "Home");
            }

// Якщо валідація моделі не пройдена, зупиняємо запис у БД і повертаємо форму з помилками користувачу.
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (ModelState.IsValid)
            {
                await _executorTaskService.UpdateAsync(executorTask);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Executors = await _executorService.GetAllAsync();
            ViewBag.Clients = await _executorTaskService.GetClientsByExecutorAsync(executorTask.ExecutorId ?? 0);
            var clientId = executorTask.ClientId ?? (await _executorTaskService.GetByIdAsync(executorTask.ExecutorTaskId))?.ClientTask?.ClientId ?? 0;
            ViewBag.Tasks = await _executorTaskService.GetTasksByExecutorAndClientAsync(executorTask.ExecutorId ?? 0, clientId);
            ViewBag.CurrentClientId = clientId;
            return View(executorTask);
        }

// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager")]
// Метод Delete реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int id.
        public async Task<IActionResult> Delete(int id)
        {
            var executorTask = await _executorTaskService.GetByIdAsync(id);
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (executorTask == null) return NotFound();
            return View(executorTask);
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
            await _executorTaskService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

// HTTP GET використовується для читання даних і відкриття сторінки без зміни стану БД.
        [HttpGet]
// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager")]
// Метод GetClientsByExecutor реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int executorId.
        public async Task<IActionResult> GetClientsByExecutor(int executorId)
        {
            var clients = await _executorTaskService.GetClientsByExecutorAsync(executorId);
            return Json(clients.Select(c => new { c.ClientId, c.Name }));
        }

// HTTP GET використовується для читання даних і відкриття сторінки без зміни стану БД.
        [HttpGet]
// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager")]
// Метод GetTasksByExecutorClient реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int executorId, int clientId.
        public async Task<IActionResult> GetTasksByExecutorClient(int executorId, int clientId)
        {
            var tasks = await _executorTaskService.GetTasksByExecutorAndClientAsync(executorId, clientId);
            return Json(tasks.Select(t => new { t.ClientTaskId, t.TaskTitle }));
        }

        private async Task<bool> CanExecutorEditTaskAsync(ExecutorTask executorTask)
        {
            var user = await _userManager.GetUserAsync(User);
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (user == null || !user.ExecutorId.HasValue)
            {
                return false;
            }

            return executorTask.ExecutorId == user.ExecutorId.Value;
        }
    }
}
