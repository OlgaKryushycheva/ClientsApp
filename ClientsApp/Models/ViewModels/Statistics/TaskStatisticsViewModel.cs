// ViewModel TaskStatisticsViewModel описує дані, які конкретна сторінка отримує або відправляє.
// Модель містить лише ті властивості, які реально використовуються у формі/представленні.
using System;

namespace ClientsApp.Models.ViewModels.Statistics
{
// TaskStatisticsViewModel: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public class TaskStatisticsViewModel
    {
        public string ClientName { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
        public string TaskDescription { get; set; } = string.Empty;
        public decimal TaskCost { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal BalanceDue { get; set; }
    }
}
