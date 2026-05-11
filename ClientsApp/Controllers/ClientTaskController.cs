using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;
using ClientsApp.Models.Entities;
using ClientsApp.Models.ViewModels;
using ClientsApp.BLL.Interfaces;
using System.Collections.Generic;

namespace ClientsApp.Controllers
{
    // Доступ до задач клієнтів вимагає авторизації, щоб сторонні користувачі
    // не могли переглядати внутрішні дедлайни, статуси й призначених виконавців.
    [Authorize]
    public class ClientTaskController : Controller
    {
        private readonly IClientTaskService _taskService;
        private readonly IClientService _clientService;
        private readonly IExecutorService _executorService;

        public ClientTaskController(
            IClientTaskService taskService,
            IClientService clientService,
            IExecutorService executorService)
        {
            _taskService = taskService;
            _clientService = clientService;
            _executorService = executorService;
        }

        public async Task<IActionResult> Index(int? selectedClientId, int? selectedExecutorId, ClientTaskStatusEnum? selectedStatus, string sortOrder)
        {
            // Приймаємо тільки два варіанти сортування; будь-яке інше значення
            // примусово зводимо до "asc", щоб запит до списку задач був передбачуваний.
            var normalizedSortOrder = sortOrder == "desc" ? "desc" : "asc";
            var sortDescending = normalizedSortOrder == "desc";
            // Завантажуємо задачі з урахуванням вибраних фільтрів (клієнт, виконавець, статус)
            // та напряму сортування для таблиці задач на головній сторінці.
            var tasks = await _taskService.SearchAsync(selectedClientId, selectedExecutorId, selectedStatus, sortDescending);

            var clients = await _clientService.GetAllAsync();
            var executors = await _executorService.GetAllAsync();

            var model = new ClientTaskIndexViewModel
            {
                Tasks = tasks,
                // Формуємо список клієнтів для фільтра у верхній панелі,
                // щоб менеджер міг швидко показати задачі тільки конкретного клієнта.
                Clients = clients.Select(c => new SelectListItem
                {
                    Value = c.ClientId.ToString(),
                    Text = c.Name
                }).ToList(),
                // Формуємо список виконавців для фільтрації задач за відповідальним співробітником.
                Executors = executors.Select(e => new SelectListItem
                {
                    Value = e.ExecutorId.ToString(),
                    Text = e.FullName
                }).ToList(),
                // Перелік статусів потрібен для відбору задач:
                // наприклад, показати лише нові або ті, що вже в роботі.
                Statuses = Enum.GetValues(typeof(ClientTaskStatusEnum))
                    .Cast<ClientTaskStatusEnum>()
                    .Select(s => new SelectListItem
                    {
                        Value = ((int)s).ToString(),
                        Text = s.ToString()
                    }).ToList(),
                SelectedClientId = selectedClientId,
                SelectedExecutorId = selectedExecutorId,
                SelectedStatus = selectedStatus,
                SortOrder = normalizedSortOrder
            };

            return View(model);
        }

        // Створювати нові задачі клієнтів може тільки менеджер, бо саме він
        // визначає обсяг робіт, строки й виконавців.
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Create()
        {
            // Підготовлюємо дані для форми (клієнти, виконавці, статуси),
            // щоб на сторінці створення одразу були доступні всі потрібні вибори.
            await PopulateCreateViewBagsAsync();
            return View();
        }

        [HttpPost]
        // Перевіряємо, що форма створення задачі надіслана з нашого сайту,
        // інакше запит відхиляється як потенційно підроблений.
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Create(ClientTask task, int[] selectedExecutors)
        {
            // Якщо обов’язкові поля задачі заповнені некоректно,
            // повертаємо ту саму форму зі збереженими значеннями для виправлення.
            if (!ModelState.IsValid)
            {
                await PopulateCreateViewBagsAsync(task, selectedExecutors);
                return View(task);
            }

            // Перевірки доступності виконавців прив’язуємо до дати старту задачі,
            // оскільки саме з цієї дати люди мають бути реально доступні в роботі.
            var startDate = task.StartDate.Date;
            var selectedExecutorsData = (await _executorService.GetAllAsync())
                .Where(e => selectedExecutors.Contains(e.ExecutorId))
                .ToList();

            // Відбираємо лише тих виконавців, які офіційно недоступні на дату початку задачі,
            // щоб не допустити призначення задачі на період відпустки/відсутності.
            var invalidUnavailableExecutors = selectedExecutorsData
                .Where(e => e.UnavailableFrom.HasValue
                    && e.UnavailableTo.HasValue
                    && startDate >= e.UnavailableFrom.Value.Date
                    && startDate <= e.UnavailableTo.Value.Date)
                .Select(e => e.FullName)
                .ToList();

            if (invalidUnavailableExecutors.Count > 0)
            {
                // Додаємо загальну помилку форми: менеджер бачить перелік недоступних людей
                // і може одразу змінити команду виконавців для цієї задачі клієнта.
                ModelState.AddModelError(string.Empty, $"Обрані виконавці недоступні на дату початку: {string.Join(", ", invalidUnavailableExecutors)}.");
            }

            // Окремо блокуємо призначення виконавців, які вже звільнені на дату старту,
            // щоб у задачі не залишалися неактуальні відповідальні.
            var dismissedExecutors = selectedExecutorsData
                .Where(e => e.DismissedFrom.HasValue && startDate >= e.DismissedFrom.Value.Date)
                .Select(e => e.FullName)
                .ToList();

            if (dismissedExecutors.Count > 0)
            {
                ModelState.AddModelError(string.Empty, $"Обрані виконавці звільнені на дату початку: {string.Join(", ", dismissedExecutors)}.");
            }

            // Якщо після перевірок з’явилися помилки, повторно відкриваємо форму,
            // щоб менеджер відкоригував дату або склад виконавців.
            if (!ModelState.IsValid)
            {
                await PopulateCreateViewBagsAsync(task, selectedExecutors);
                return View(task);
            }

            // Перетворюємо вибрані ID у зв’язки задача-виконавець,
            // щоб зберегти фактичні призначення відповідальних до нової задачі.
            task.ExecutorTasks = selectedExecutors.Select(eid => new ExecutorTask
            {
                ExecutorId = eid
            }).ToList();

            await _taskService.AddAsync(task);

            // Після успішного створення повертаємо до загального списку,
            // де нова задача одразу з’явиться у таблиці задач клієнтів.
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> InProgressByExecutorIds([FromQuery] int[] executorIds)
        {
            // Якщо ідентифікатори не передані, немає сенсу шукати задачі —
            // повертаємо порожній JSON для коректної роботи клієнтського запиту.
            if (executorIds == null || executorIds.Length == 0)
            {
                return Json(Array.Empty<object>());
            }

            // Беремо лише задачі зі статусом InProgress,
            // щоб показати фактичне поточне навантаження виконавців.
            var allInProgressTasks = await _taskService.SearchAsync(null, null, ClientTaskStatusEnum.InProgress);

            // Фільтруємо задачі за переданими виконавцями та формуємо компактну відповідь:
            // клієнт, назва задачі, статус і список дотичних виконавців.
            var result = allInProgressTasks
                .Where(t => t.ExecutorTasks.Any(et => et.ExecutorId.HasValue && executorIds.Contains(et.ExecutorId.Value)))
                .Select(t => new
                {
                    clientName = t.Client?.Name ?? "Без клієнта",
                    taskTitle = t.TaskTitle,
                    status = t.TaskStatus.ToString(),
                    executors = t.ExecutorTasks
                        .Where(et => et.ExecutorId.HasValue && executorIds.Contains(et.ExecutorId.Value))
                        .Select(et => et.Executor?.FullName ?? "Невідомий виконавець")
                        .Distinct()
                        .ToList()
                })
                .ToList();

            return Json(result);
        }


        // Редагувати задачі клієнтів може лише менеджер,
        // оскільки зміна статусу/строків впливає на план робіт команди.
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _taskService.GetByIdAsync(id);
            // Якщо задачу видалено або ID помилковий, повертаємо 404,
            // щоб не відкривати порожню форму редагування.
            if (task == null) return NotFound();

            var model = new ClientTaskEditViewModel
            {
                ClientTaskId = task.ClientTaskId,
                TaskTitle = task.TaskTitle,
                Description = task.Description,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                ClientId = task.ClientId,
                TaskStatus = task.TaskStatus,
                // Беремо тільки валідні посилання на виконавців, де є ID,
                // щоб у формі редагування були позначені реальні призначення задачі.
                SelectedExecutors = task.ExecutorTasks
                    .Where(et => et.ExecutorId.HasValue)
                    .Select(et => et.ExecutorId!.Value)
                    .ToList(),
                // SelectList заповнює випадаючий список клієнтів
                // і одразу встановлює поточного клієнта задачі як вибраний.
                Clients = new SelectList(await _clientService.GetAllAsync(), "ClientId", "Name", task.ClientId),
                // MultiSelectList показує всіх виконавців і підсвічує тих,
                // хто вже призначений на задачу, щоб менеджер міг змінити склад команди.
                Executors = new MultiSelectList(await _executorService.GetAllAsync(), "ExecutorId", "FullName", task.ExecutorTasks.Select(et => et.ExecutorId)),
                Statuses = new SelectList(Enum.GetValues(typeof(ClientTaskStatusEnum)), task.TaskStatus)
            };

            return View(model);
        }

        [HttpPost]
        // Захищаємо зміну даних задачі від CSRF,
        // оскільки редагування впливає на реальний робочий план клієнтських задач.
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(ClientTaskEditViewModel model)
        {
            // Якщо модель не проходить валідацію, повертаємо помилки,
            // щоб не зберегти задачу з неповними або некоректними даними.
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return Content("Некоректні дані моделі: " + string.Join(", ", errors));
            }

            var task = new ClientTask
            {
                ClientTaskId = model.ClientTaskId,
                TaskTitle = model.TaskTitle,
                Description = model.Description,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                ClientId = model.ClientId,
                TaskStatus = model.TaskStatus,
                // Формуємо оновлений перелік призначень виконавців:
                // після збереження саме цей склад відповідатиме за задачу.
                ExecutorTasks = model.SelectedExecutors.Select(eid => new ExecutorTask { ExecutorId = eid }).ToList()
            };

            await _taskService.UpdateAsync(task);
            // Після оновлення задачі повертаємося до списку,
            // щоб менеджер одразу бачив актуальні статуси й призначення.
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _taskService.GetByIdAsync(id);
            // Якщо задачу за цим ID не знайдено, показуємо 404
            // замість сторінки підтвердження видалення неіснуючого запису.
            if (task == null) return NotFound();
            return View(task);
        }

        [HttpPost, ActionName("Delete")]
        // Підтвердження видалення теж захищене anti-forgery токеном,
        // щоб сторонній сайт не міг видалити задачу від імені менеджера.
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _taskService.DeleteAsync(id);
            // Після видалення задачі повертаємо до переліку,
            // де запис уже не відображатиметься у списку задач клієнтів.
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateCreateViewBagsAsync(ClientTask? task = null, int[]? selectedExecutors = null)
        {
            var today = DateTime.Today;

            // ViewBag.Clients використовується у формі створення:
            // менеджер обирає, для якого саме клієнта заводиться задача.
            ViewBag.Clients = new SelectList(await _clientService.GetAllAsync(), "ClientId", "Name", task?.ClientId);
            // У список виконавців не додаємо звільнених станом на сьогодні
            // і сортуємо за ПІБ, щоб менеджеру було простіше знайти потрібну людину.
            ViewBag.Executors = (await _executorService.GetAllAsync())
                .Where(e => !e.DismissedFrom.HasValue || e.DismissedFrom.Value.Date >= today)
                .OrderBy(e => e.FullName)
                .ToList();
            // Зберігаємо попередній вибір виконавців після помилки валідації,
            // щоб менеджеру не довелося повторно відмічати їх у формі.
            ViewBag.SelectedExecutors = new HashSet<int>(selectedExecutors ?? Array.Empty<int>());
            // Передаємо перелік можливих статусів задачі (наприклад New/InProgress/Done)
            // для вибору поточного стану під час створення або повторного показу форми.
            ViewBag.Statuses = new SelectList(Enum.GetValues(typeof(ClientTaskStatusEnum)), task?.TaskStatus);
        }
    }
}
