// ViewModel DebtStatisticsItemViewModel описує дані, які конкретна сторінка отримує або відправляє.
// Модель містить лише ті властивості, які реально використовуються у формі/представленні.
namespace ClientsApp.Models.ViewModels.Statistics
{
// DebtStatisticsItemViewModel: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public class DebtStatisticsItemViewModel
    {
        public string ClientName { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
        public decimal BalanceDue { get; set; }
    }
}
