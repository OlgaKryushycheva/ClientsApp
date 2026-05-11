// Сутність Payment відповідає таблиці/даним предметної області.
// DataAnnotations нижче керують валідацією форми й мапінгом полів у БД.
using System;
using System.ComponentModel.DataAnnotations;

namespace ClientsApp.Models.Entities
{
// Payment: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
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
