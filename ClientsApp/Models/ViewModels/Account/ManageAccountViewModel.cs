namespace ClientsApp.Models.ViewModels.Account
{
    public class ManageAccountViewModel
    {
        public UpdateEmailViewModel UpdateEmail { get; set; } = new();

        public ChangePasswordViewModel ChangePassword { get; set; } = new();
    }
}
