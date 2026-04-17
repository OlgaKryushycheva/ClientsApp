using System.ComponentModel.DataAnnotations;

namespace ClientsApp.Models.ViewModels.Account
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Поточний пароль є обов'язковим")]
        [DataType(DataType.Password)]
        [Display(Name = "Поточний пароль")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Новий пароль є обов'язковим")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль має містити щонайменше 6 символів")]
        [Display(Name = "Новий пароль")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Підтвердження пароля є обов'язковим")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Паролі не співпадають")]
        [Display(Name = "Підтвердження нового пароля")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
