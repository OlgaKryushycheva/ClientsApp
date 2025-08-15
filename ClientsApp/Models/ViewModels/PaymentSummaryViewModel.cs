using System;

namespace ClientsApp.Models.ViewModels
{
    public class PaymentSummaryViewModel
    {
        public int ClientTaskId { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string TaskTitle { get; set; }
        public decimal ServiceCost { get; set; }
        public decimal AmountReceived { get; set; }
        public decimal BalanceDue { get; set; }
    }
}
