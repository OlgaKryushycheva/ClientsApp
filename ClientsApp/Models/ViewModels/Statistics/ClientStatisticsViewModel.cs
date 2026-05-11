// ViewModel ClientStatisticsViewModel описує дані, які конкретна сторінка отримує або відправляє.
// Модель містить лише ті властивості, які реально використовуються у формі/представленні.
using System.Collections.Generic;

namespace ClientsApp.Models.ViewModels.Statistics
{
// ClientStatisticsViewModel: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public class ClientStatisticsViewModel
    {
        public string ClientName { get; set; } = string.Empty;
        public IList<ClientTaskCostViewModel> Tasks { get; set; } = new List<ClientTaskCostViewModel>();
        public decimal TotalCost { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal TotalDebt { get; set; }
        public IList<string> TasksWithoutInvoice { get; set; } = new List<string>();
    }
}
