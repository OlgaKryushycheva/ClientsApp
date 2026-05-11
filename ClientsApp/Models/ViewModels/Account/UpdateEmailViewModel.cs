using System.ComponentModel.DataAnnotations;

namespace ClientsApp.Models.ViewModels.Account
{
    public class UpdateEmailViewModel
    {
        [Required(ErrorMessage = "Email є обов'язковим")]
        [EmailAddress(ErrorMessage = "Некоректний email")]
        [Display(Name = "Новий email")]
        public string NewEmail { get; set; } = string.Empty;
    }
}
