using ClientsApp.Models;
using ClientsApp.Models.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientsApp.Controllers
{
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

        [Authorize(Roles = "Manager")]
        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Roles = AllowedRoles;
            return View(new RegisterViewModel());
        }

        [Authorize(Roles = "Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ViewBag.Roles = AllowedRoles;

            if (!AllowedRoles.Contains(model.Role))
            {
                ModelState.AddModelError(nameof(model.Role), "Оберіть коректну роль.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            if (model.Role == "Executor")
            {
                var executor = await _context.Executors.FirstOrDefaultAsync(e => e.Email == model.Email);
                if (executor == null)
                {
                    ModelState.AddModelError(nameof(model.Email), "Для ролі Executor потрібен існуючий виконавець з таким email.");
                    return View(model);
                }

                user.ExecutorId = executor.ExecutorId;
            }

            var result = await _userManager.CreateAsync(user, model.Password);
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

        [Authorize(Roles = "Manager")]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
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

        [Authorize(Roles = "Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEmail([Bind(Prefix = "UpdateEmail")] UpdateEmailViewModel model)
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
                    UpdateEmail = model,
                    ChangePassword = new ChangePasswordViewModel()
                });
            }

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

            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Email успішно змінено.";

            return RedirectToAction(nameof(Profile));
        }

        [Authorize(Roles = "Manager")]
        [HttpPost]
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

            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Пароль успішно змінено.";

            return RedirectToAction(nameof(Profile));
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }

            ModelState.AddModelError(string.Empty, "Невірна спроба входу.");
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
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
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
