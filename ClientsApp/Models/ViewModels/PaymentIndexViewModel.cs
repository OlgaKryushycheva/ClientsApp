// ViewModel PaymentIndexViewModel описує дані, які конкретна сторінка отримує або відправляє.
// Модель містить лише ті властивості, які реально використовуються у формі/представленні.
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClientsApp.Models.ViewModels
{
// PaymentIndexViewModel: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public class PaymentIndexViewModel
    {
        public IEnumerable<PaymentSummaryViewModel> Payments { get; set; } = new List<PaymentSummaryViewModel>();
        public List<SelectListItem> Clients { get; set; } = new List<SelectListItem>();
        public int? SelectedClientId { get; set; }
        public bool? IsPaid { get; set; }
    }
}
