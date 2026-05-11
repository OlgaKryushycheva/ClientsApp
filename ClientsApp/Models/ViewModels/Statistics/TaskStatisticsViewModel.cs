using System;

namespace ClientsApp.Models.ViewModels.Statistics
{
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
