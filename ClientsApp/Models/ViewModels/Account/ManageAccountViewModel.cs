// ViewModel ManageAccountViewModel описує дані, які конкретна сторінка отримує або відправляє.
// Модель містить лише ті властивості, які реально використовуються у формі/представленні.
namespace ClientsApp.Models.ViewModels.Account
{
// ManageAccountViewModel: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public class ManageAccountViewModel
    {
        public UpdateEmailViewModel UpdateEmail { get; set; } = new();

        public ChangePasswordViewModel ChangePassword { get; set; } = new();
    }
}
