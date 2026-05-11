// ViewModel UpdateEmailViewModel описує дані, які конкретна сторінка отримує або відправляє.
// Модель містить лише ті властивості, які реально використовуються у формі/представленні.
using System.ComponentModel.DataAnnotations;

namespace ClientsApp.Models.ViewModels.Account
{
// UpdateEmailViewModel: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public class UpdateEmailViewModel
    {
        [Required(ErrorMessage = "Email є обов'язковим")]
        [EmailAddress(ErrorMessage = "Некоректний email")]
        [Display(Name = "Новий email")]
        public string NewEmail { get; set; } = string.Empty;
    }
}
