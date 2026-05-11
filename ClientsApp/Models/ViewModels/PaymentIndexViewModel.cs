using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClientsApp.Models.ViewModels
{
    public class PaymentIndexViewModel
    {
        public IEnumerable<PaymentSummaryViewModel> Payments { get; set; } = new List<PaymentSummaryViewModel>();
        public List<SelectListItem> Clients { get; set; } = new List<SelectListItem>();
        public int? SelectedClientId { get; set; }
        public bool? IsPaid { get; set; }
    }
}
