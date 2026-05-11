using ClientsApp.Models;
using ClientsApp.Models.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientsApp.Controllers
{
    // Цей контролер відповідає за облікові записи працівників:
    // вхід, вихід, створення акаунтів та зміну власних облікових даних менеджера.
    public class AccountController : Controller
    {
        // Через форму реєстрації менеджер може створити тільки ці ролі,
        // щоб випадково не видати привілеї Manager або іншу службову роль.
        private static readonly string[] AllowedRoles = ["Accountant", "Executor"];

        // UserManager працює з користувачем у сховищі Identity:
        // створює акаунт, змінює email/логін, змінює пароль, шукає за email.
        private readonly UserManager<ApplicationUser> _userManager;

        // SignInManager відповідає за вхід/вихід і cookie авторизації:
        // перевіряє пароль та створює/оновлює сесію в браузері.
        private readonly SignInManager<ApplicationUser> _signInManager;

        // Контекст потрібен для перевірки звʼязку з таблицею Executors
        // під час створення користувача з роллю Executor.
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // Нові облікові записи співробітників у системі створює тільки менеджер.
        [Authorize(Roles = "Manager")]
        [HttpGet]
        public IActionResult Register()
        {
            // Передаємо у форму лише дозволені ролі,
            // щоб список ролей у UI не розходився з серверною валідацією.
            ViewBag.Roles = AllowedRoles;
            return View(new RegisterViewModel());
        }

        // Нові облікові записи співробітників у системі створює тільки менеджер.
        [Authorize(Roles = "Manager")]
        [HttpPost]
        // Перевіряє, що форму реєстрації відправлено з нашого сайту,
        // а не підробленим запитом із зовнішньої сторінки.
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ViewBag.Roles = AllowedRoles;

            // Дублюємо перевірку ролі на сервері, бо значення role можна підмінити в HTTP-запиті,
            // навіть якщо в інтерфейсі користувач бачить тільки AllowedRoles.
            if (!AllowedRoles.Contains(model.Role))
            {
                ModelState.AddModelError(nameof(model.Role), "Оберіть коректну роль.");
            }

            // Якщо валідація моделі не пройшла (email, пароль, роль),
            // повертаємо форму з конкретними помилками із ModelState.
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Email використовується і як логін UserName,
            // тому входити в систему користувач буде саме через email.
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            if (model.Role == "Executor")
            {
                // Для ролі Executor шукаємо існуючого виконавця з тим самим email:
                // це гарантує, що акаунт привʼязується до реального запису співробітника.
                var executor = await _context.Executors.FirstOrDefaultAsync(e => e.Email == model.Email);
                if (executor == null)
                {
                    ModelState.AddModelError(nameof(model.Email), "Для ролі Executor потрібен існуючий виконавець з таким email.");
                    return View(model);
                }

                // Зберігаємо ExecutorId у користувачі,
                // щоб система могла повʼязувати вхід у акаунт з задачами та даними конкретного виконавця.
                user.ExecutorId = executor.ExecutorId;
            }

            // CreateAsync створює запис користувача в Identity та хешує пароль,
            // тому пароль не зберігається у відкритому вигляді.
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                // Помилки Identity (слабкий пароль, дубль email тощо)
                // додаємо в ModelState, щоб менеджер бачив причину відмови у формі.
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            // Призначаємо роль одразу після створення,
            // інакше новий користувач не отримає доступ до потрібних розділів системи.
            await _userManager.AddToRoleAsync(user, model.Role);

            // Після створення акаунта повертаємо менеджера на головну,
            // а не входимо під новим користувачем у поточній сесії.
            return RedirectToAction("Index", "Home");
        }

        // Сторінка редагування облікових даних доступна тільки менеджеру.
        [Authorize(Roles = "Manager")]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                // Якщо cookie є, але користувача вже видалили з бази,
                // перенаправляємо на повторний вхід.
                return RedirectToAction(nameof(Login));
            }

            var model = new ManageAccountViewModel
            {
                UpdateEmail = new UpdateEmailViewModel
                {
                    NewEmail = user.Email ?? string.Empty
                },
                ChangePassword = new ChangePasswordViewModel()
            };

            return View(model);
        }

        [Authorize(Roles = "Manager")]
        [HttpPost]
        // Блокує підроблену відправку форми зміни email з іншого сайту.
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEmail([Bind(Prefix = "UpdateEmail")] UpdateEmailViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Якщо новий email не проходить валідацію, показуємо цю ж сторінку профілю
            // та повертаємо введене значення, щоб менеджер міг виправити помилку.
            if (!ModelState.IsValid)
            {
                return View("Profile", new ManageAccountViewModel
                {
                    UpdateEmail = model,
                    ChangePassword = new ChangePasswordViewModel()
                });
            }

            // Не дозволяємо зайняти email іншого користувача,
            // бо email в цій системі одночасно є ідентифікатором входу.
            var emailInUse = await _userManager.FindByEmailAsync(model.NewEmail);
            if (emailInUse is not null && emailInUse.Id != user.Id)
            {
                ModelState.AddModelError("UpdateEmail.NewEmail", "Користувач з таким email вже існує.");
                return View("Profile", new ManageAccountViewModel
                {
                    UpdateEmail = model,
                    ChangePassword = new ChangePasswordViewModel()
                });
            }

            // Оновлюємо email у профілі Identity.
            var setEmailResult = await _userManager.SetEmailAsync(user, model.NewEmail);
            if (!setEmailResult.Succeeded)
            {
                foreach (var error in setEmailResult.Errors)
                {
                    ModelState.AddModelError("UpdateEmail.NewEmail", error.Description);
                }

                return View("Profile", new ManageAccountViewModel
                {
                    UpdateEmail = model,
                    ChangePassword = new ChangePasswordViewModel()
                });
            }

            // UserName також має збігатися з email,
            // щоб вхід працював передбачувано через одне й те саме значення.
            var setUserNameResult = await _userManager.SetUserNameAsync(user, model.NewEmail);
            if (!setUserNameResult.Succeeded)
            {
                foreach (var error in setUserNameResult.Errors)
                {
                    ModelState.AddModelError("UpdateEmail.NewEmail", error.Description);
                }

                return View("Profile", new ManageAccountViewModel
                {
                    UpdateEmail = model,
                    ChangePassword = new ChangePasswordViewModel()
                });
            }

            // Оновлюємо cookie поточної сесії, щоб у claims ідентичності
            // одразу відобразився новий email без примусового повторного входу.
            await _signInManager.RefreshSignInAsync(user);

            // TempData зберігає коротке повідомлення між редиректами,
            // тому текст успіху буде показаний вже після переходу на Profile.
            TempData["SuccessMessage"] = "Email успішно змінено.";

            // RedirectToAction розриває повторну відправку форми при оновленні сторінки
            // і відкриває чистий GET профілю з новими даними.
            return RedirectToAction(nameof(Profile));
        }

        [Authorize(Roles = "Manager")]
        [HttpPost]
        // Перевіряє anti-forgery токен для форми зміни пароля,
        // щоб пароль не можна було змінити через сторонній сайт від імені менеджера.
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword([Bind(Prefix = "ChangePassword")] ChangePasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return RedirectToAction(nameof(Login));
            }

            if (!ModelState.IsValid)
            {
                return View("Profile", new ManageAccountViewModel
                {
                    UpdateEmail = new UpdateEmailViewModel
                    {
                        NewEmail = user.Email ?? string.Empty
                    },
                    ChangePassword = model
                });
            }

            // ChangePasswordAsync вимагає поточний пароль,
            // тому стороння людина з відкритою сесією не зможе тихо змінити пароль без знання старого.
            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError("ChangePassword.CurrentPassword", error.Description);
                }

                return View("Profile", new ManageAccountViewModel
                {
                    UpdateEmail = new UpdateEmailViewModel
                    {
                        NewEmail = user.Email ?? string.Empty
                    },
                    ChangePassword = model
                });
            }

            // Після зміни пароля оновлюємо cookie входу,
            // щоб поточна сесія залишалась валідною без повторного логіну.
            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Пароль успішно змінено.";

            return RedirectToAction(nameof(Profile));
        }

        // Сторінка входу доступна без авторизації,
        // інакше неавторизований користувач не зможе потрапити у систему.
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Зберігаємо адресу, куди треба повернути користувача після входу,
            // якщо він спочатку відкрив захищену сторінку.
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        // Перевіряє, що форму входу надіслав саме наш сайт.
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // PasswordSignInAsync перевіряє email і пароль через Identity,
            // а у випадку успіху створює cookie авторизованого користувача.
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                // Якщо перед входом користувач запитував захищений URL,
                // RedirectToLocal поверне його на ту адресу замість головної.
                return RedirectToLocal(returnUrl);
            }

            ModelState.AddModelError(string.Empty, "Невірна спроба входу.");
            return View(model);
        }

        [Authorize]
        [HttpPost]
        // Підтверджує, що запит на вихід ініційовано з нашої сторінки.
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // SignOutAsync очищає cookie авторизації,
            // щоб цей браузер більше не вважався залогіненим.
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            // Дозволяємо редирект тільки на локальну адресу сайту,
            // щоб запобігти open redirect на зовнішні домени.
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Якщо returnUrl порожній або небезпечний,
            // відправляємо користувача на стандартну домашню сторінку.
            return RedirectToAction("Index", "Home");
        }
    }
}
