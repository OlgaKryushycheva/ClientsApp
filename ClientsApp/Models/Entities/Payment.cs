using System;
using System.ComponentModel.DataAnnotations;

namespace ClientsApp.Models.Entities
{
    public class Payment
    {
        public int PaymentId { get; set; }

        [Required]
        [Display(Name = "Завдання")]
        public int ClientTaskId { get; set; }
        public ClientTask? ClientTask { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Display(Name = "Сума")]
        public decimal Amount { get; set; }

        [Display(Name = "Дата платежу")]
        public DateTime PaymentDate { get; set; }
        [Display(Name = "Заборгованість")]
        public decimal BalanceDue { get; set; }
    }
}
