using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientsApp.Models.Entities
{
    public class ExecutorTask
    {
        public int ExecutorTaskId { get; set; }  // Primary key linking executor and task

        [Required(ErrorMessage = "Вкажіть виконавця")]
        [Display(Name = "Виконавець")]
        public int? ExecutorId { get; set; }  // Executor identifier
        public Executor? Executor { get; set; }  // Navigation property for executor

        [NotMapped]
        [Required(ErrorMessage = "Вкажіть клієнта")]
        [Display(Name = "Клієнт")]
        public int? ClientId { get; set; }  // Selected client (for form)

        [Required(ErrorMessage = "Вкажіть завдання")]
        [Display(Name = "Завдання")]
        public int? ClientTaskId { get; set; }  // Task identifier
        public ClientTask? ClientTask { get; set; }  // Navigation property for task

        [Display(Name = "Фактичний час")]
        public decimal ActualTime { get; set; }  // Time spent by executor
        [Display(Name = "Скоригований час")]
        public decimal AdjustedTime { get; set; }  // Adjusted time

        [NotMapped]
        public decimal TaskCost => AdjustedTime * (Executor?.HourlyRate ?? 0);  // Task cost
    }
}

