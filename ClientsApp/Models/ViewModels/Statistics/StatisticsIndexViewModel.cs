using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClientsApp.Models.ViewModels.Statistics
{
    public class StatisticsIndexViewModel
    {
        public int? SelectedClientIdForTask { get; set; }
        public int? SelectedTaskId { get; set; }
        public int? SelectedClientIdForClient { get; set; }

        public IEnumerable<SelectListItem> ClientOptionsForTaskStats { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> TaskOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> ClientOptionsForClientStats { get; set; } = new List<SelectListItem>();

        public TaskStatisticsViewModel? TaskStatistics { get; set; }
        public ClientStatisticsViewModel? ClientStatistics { get; set; }
        public IList<ExecutorPerformanceViewModel> ExecutorPerformances { get; set; } = new List<ExecutorPerformanceViewModel>();
        public IList<DebtStatisticsItemViewModel> DebtStatistics { get; set; } = new List<DebtStatisticsItemViewModel>();
        public decimal TotalDebt { get; set; }
    }
}
