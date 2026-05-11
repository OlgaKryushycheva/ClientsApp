// ViewModel ClientTaskCostViewModel описує дані, які конкретна сторінка отримує або відправляє.
// Модель містить лише ті властивості, які реально використовуються у формі/представленні.
namespace ClientsApp.Models.ViewModels.Statistics
{
// ClientTaskCostViewModel: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public class ClientTaskCostViewModel
    {
        public string TaskTitle { get; set; } = string.Empty;
        public decimal TaskCost { get; set; }
        public decimal Payments { get; set; }
        public decimal BalanceDue { get; set; }
    }
}
