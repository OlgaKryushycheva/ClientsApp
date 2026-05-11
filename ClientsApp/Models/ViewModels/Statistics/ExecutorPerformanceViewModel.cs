// ViewModel ExecutorPerformanceViewModel описує дані, які конкретна сторінка отримує або відправляє.
// Модель містить лише ті властивості, які реально використовуються у формі/представленні.
namespace ClientsApp.Models.ViewModels.Statistics
{
// ExecutorPerformanceViewModel: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public class ExecutorPerformanceViewModel
    {
        public string ExecutorName { get; set; } = string.Empty;
        public decimal TotalActualTime { get; set; }
        public decimal TotalAdjustedTime { get; set; }
        public decimal? Ratio { get; set; }
    }
}
