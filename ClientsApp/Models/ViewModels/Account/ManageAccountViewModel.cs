// Модель містить лише ті властивості, які реально використовуються у формі/представленні.
namespace ClientsApp.Models.ViewModels.Account
{
    public class ManageAccountViewModel
    {
        public UpdateEmailViewModel UpdateEmail { get; set; } = new();

        public ChangePasswordViewModel ChangePassword { get; set; } = new();
    }
}
