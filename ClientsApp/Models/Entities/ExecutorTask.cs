using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientsApp.Models.Entities
{
    public class ExecutorTask
    {
        public int ExecutorTaskId { get; set; }

        [Required(ErrorMessage = "Вкажіть виконавця")]
        [Display(Name = "Виконавець")]
        public int? ExecutorId { get; set; }
        public Executor? Executor { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "Вкажіть клієнта")]
        [Display(Name = "Клієнт")]
        public int? ClientId { get; set; }

        [Required(ErrorMessage = "Вкажіть завдання")]
        [Display(Name = "Завдання")]
        public int? ClientTaskId { get; set; }
        public ClientTask? ClientTask { get; set; }

        [Display(Name = "Фактичний час")]
        public decimal ActualTime { get; set; }
        [Display(Name = "Скоригований час")]
        public decimal AdjustedTime { get; set; }

        [NotMapped]
        public decimal TaskCost => AdjustedTime * (Executor?.HourlyRate ?? 0);
    }
}

