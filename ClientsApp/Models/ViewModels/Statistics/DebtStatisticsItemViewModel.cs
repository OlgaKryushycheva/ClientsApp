namespace ClientsApp.Models.ViewModels.Statistics
{
    public class DebtStatisticsItemViewModel
    {
        public string ClientName { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
        public decimal BalanceDue { get; set; }
    }
}
