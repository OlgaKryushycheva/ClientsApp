// Файл ErrorViewModel.cs містить допоміжний тип застосунку.
// Коментарі нижче пояснюють призначення ключових методів і правил валідації.
namespace ClientsApp.Models
{
// ErrorViewModel: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
