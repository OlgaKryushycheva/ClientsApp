using System;
using System.ComponentModel.DataAnnotations;

namespace ClientsApp.Models.Entities
{
    public class Payment
    {
        public int PaymentId { get; set; }

        [Required]
        public int ClientTaskId { get; set; }
        public ClientTask ClientTask { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }
        public decimal BalanceDue { get; set; }
    }
}
