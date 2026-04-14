using System.ComponentModel.DataAnnotations;

namespace ClientsApp.Models.ViewModels.Account
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Email є обов'язковим")]
        [EmailAddress(ErrorMessage = "Некоректний email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль є обов'язковим")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль має містити щонайменше 6 символів")]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Підтвердження пароля є обов'язковим")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Паролі не співпадають")]
        [Display(Name = "Підтвердження пароля")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Оберіть роль")]
        [Display(Name = "Роль")]
        public string Role { get; set; } = string.Empty;
    }
}
