using System.Collections.Generic;

namespace ClientsApp.Models.ViewModels.Statistics
{
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
