using System.ComponentModel.DataAnnotations.Schema;

namespace ClientsApp.Models.Entities
{
    public class ExecutorTask
    {
        public int ExecutorTaskId { get; set; }  // Первичный ключ для связи исполнителя и задачи

        public int ExecutorId { get; set; }  // Идентификатор исполнителя
        public Executor Executor { get; set; }  // Навигационное свойство для исполнителя

        public int ClientTaskId { get; set; }  // Идентификатор задачи
        public ClientTask ClientTask { get; set; }  // Навигационное свойство для задачи

        public decimal ActualTime { get; set; }  // Время, затраченное исполнителем
        public decimal AdjustedTime { get; set; }  // Скорректированное время

        [NotMapped]
        public decimal TaskCost => AdjustedTime * (Executor?.HourlyRate ?? 0);  // Стоимость задачи
    }
}

