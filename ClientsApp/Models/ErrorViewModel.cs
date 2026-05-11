// Файл ErrorViewModel.cs містить допоміжний тип застосунку.
// Коментарі нижче пояснюють призначення ключових методів і правил валідації.
namespace ClientsApp.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
