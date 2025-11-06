namespace ClientsApp.Models.ViewModels.Statistics
{
    public class ExecutorPerformanceViewModel
    {
        public string ExecutorName { get; set; } = string.Empty;
        public decimal TotalActualTime { get; set; }
        public decimal TotalAdjustedTime { get; set; }
        public decimal? Ratio { get; set; }
    }
}
