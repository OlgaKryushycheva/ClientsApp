// Контролер AccountController обробляє HTTP-запити цього розділу UI.
// Дії нижче читають параметри запиту, викликають сервіси й повертають View/Redirect/JSON.
using ClientsApp.Models;
using ClientsApp.Models.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientsApp.Controllers
{
// AccountController: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public class AccountController : Controller
    {
        private static readonly string[] AllowedRoles = ["Accountant", "Executor"];

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
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

// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager")]
// HTTP GET використовується для читання даних і відкриття сторінки без зміни стану БД.
        [HttpGet]
// Метод Register реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
        public IActionResult Register()
        {
            ViewBag.Roles = AllowedRoles;
            return View(new RegisterViewModel());
        }

// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager")]
// HTTP POST приймає дані форми та запускає операцію створення/оновлення/видалення.
        [HttpPost]
// Anti-forgery токен блокує CSRF: сторонній сайт не зможе відправити форму від імені користувача.
        [ValidateAntiForgeryToken]
// Метод Register реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: RegisterViewModel model.
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ViewBag.Roles = AllowedRoles;

// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (!AllowedRoles.Contains(model.Role))
            {
                ModelState.AddModelError(nameof(model.Role), "Оберіть коректну роль.");
            }

// Якщо валідація моделі не пройдена, зупиняємо запис у БД і повертаємо форму з помилками користувачу.
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (model.Role == "Executor")
            {
                var executor = await _context.Executors.FirstOrDefaultAsync(e => e.Email == model.Email);
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
                if (executor == null)
                {
                    ModelState.AddModelError(nameof(model.Email), "Для ролі Executor потрібен існуючий виконавець з таким email.");
                    return View(model);
                }

                user.ExecutorId = executor.ExecutorId;
            }

            var result = await _userManager.CreateAsync(user, model.Password);
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            await _userManager.AddToRoleAsync(user, model.Role);

            return RedirectToAction("Index", "Home");
        }

// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager")]
// HTTP GET використовується для читання даних і відкриття сторінки без зміни стану БД.
        [HttpGet]
// Метод Profile реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (user is null)
            {
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

// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager")]
// HTTP POST приймає дані форми та запускає операцію створення/оновлення/видалення.
        [HttpPost]
// Anti-forgery токен блокує CSRF: сторонній сайт не зможе відправити форму від імені користувача.
        [ValidateAntiForgeryToken]
// Метод UpdateEmail реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: [Bind(Prefix = "UpdateEmail".
        public async Task<IActionResult> UpdateEmail([Bind(Prefix = "UpdateEmail")] UpdateEmailViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (user is null)
            {
                return RedirectToAction(nameof(Login));
            }

// Якщо валідація моделі не пройдена, зупиняємо запис у БД і повертаємо форму з помилками користувачу.
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (!ModelState.IsValid)
            {
                return View("Profile", new ManageAccountViewModel
                {
                    UpdateEmail = model,
                    ChangePassword = new ChangePasswordViewModel()
                });
            }

            var emailInUse = await _userManager.FindByEmailAsync(model.NewEmail);
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (emailInUse is not null && emailInUse.Id != user.Id)
            {
                ModelState.AddModelError("UpdateEmail.NewEmail", "Користувач з таким email вже існує.");
                return View("Profile", new ManageAccountViewModel
                {
                    UpdateEmail = model,
                    ChangePassword = new ChangePasswordViewModel()
                });
            }

            var setEmailResult = await _userManager.SetEmailAsync(user, model.NewEmail);
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
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

            var setUserNameResult = await _userManager.SetUserNameAsync(user, model.NewEmail);
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
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

            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Email успішно змінено.";

            return RedirectToAction(nameof(Profile));
        }

// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize(Roles = "Manager")]
// HTTP POST приймає дані форми та запускає операцію створення/оновлення/видалення.
        [HttpPost]
// Anti-forgery токен блокує CSRF: сторонній сайт не зможе відправити форму від імені користувача.
        [ValidateAntiForgeryToken]
// Метод ChangePassword реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: [Bind(Prefix = "ChangePassword".
        public async Task<IActionResult> ChangePassword([Bind(Prefix = "ChangePassword")] ChangePasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (user is null)
            {
                return RedirectToAction(nameof(Login));
            }

// Якщо валідація моделі не пройдена, зупиняємо запис у БД і повертаємо форму з помилками користувачу.
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
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

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
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

            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Пароль успішно змінено.";

            return RedirectToAction(nameof(Profile));
        }

        [AllowAnonymous]
// HTTP GET використовується для читання даних і відкриття сторінки без зміни стану БД.
        [HttpGet]
// Метод Login реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: string? returnUrl = null.
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [AllowAnonymous]
// HTTP POST приймає дані форми та запускає операцію створення/оновлення/видалення.
        [HttpPost]
// Anti-forgery токен блокує CSRF: сторонній сайт не зможе відправити форму від імені користувача.
        [ValidateAntiForgeryToken]
// Метод Login реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: LoginViewModel model, string? returnUrl = null.
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
// Якщо валідація моделі не пройдена, зупиняємо запис у БД і повертаємо форму з помилками користувачу.
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }

            ModelState.AddModelError(string.Empty, "Невірна спроба входу.");
            return View(model);
        }

// Атрибут обмежує доступ: дія виконається лише для користувача з потрібною роллю/автентифікацією.
        [Authorize]
// HTTP POST приймає дані форми та запускає операцію створення/оновлення/видалення.
        [HttpPost]
// Anti-forgery токен блокує CSRF: сторонній сайт не зможе відправити форму від імені користувача.
        [ValidateAntiForgeryToken]
// Метод Logout реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
// HTTP GET використовується для читання даних і відкриття сторінки без зміни стану БД.
        [HttpGet]
// Метод AccessDenied реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
