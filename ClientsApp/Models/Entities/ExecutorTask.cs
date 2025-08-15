using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientsApp.Models.Entities
{
    public class ExecutorTask
    {
        public int ExecutorTaskId { get; set; }  // Первичный ключ для связи исполнителя и задачи

        [Required(ErrorMessage = "Вкажіть виконавця")]
        public int? ExecutorId { get; set; }  // Ідентифікатор виконавця
        public Executor? Executor { get; set; }  // Навігаційне властивість виконавця

        [NotMapped]
        [Required(ErrorMessage = "Вкажіть клієнта")]
        public int? ClientId { get; set; }  // Вибраний клієнт (для форми)

        [Required(ErrorMessage = "Вкажіть завдання")]
        public int? ClientTaskId { get; set; }  // Ідентифікатор завдання
        public ClientTask? ClientTask { get; set; }  // Навігаційне властивість завдання

        public decimal ActualTime { get; set; }  // Время, затраченное исполнителем
        public decimal AdjustedTime { get; set; }  // Скорректированное время

        [NotMapped]
        public decimal TaskCost => AdjustedTime * (Executor?.HourlyRate ?? 0);  // Стоимость задачи
    }
}

