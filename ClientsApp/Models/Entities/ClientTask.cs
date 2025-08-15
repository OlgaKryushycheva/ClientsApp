using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientsApp.Models.Entities
{
    public class ClientTask
    {
        [Key]
        public int ClientTaskId { get; set; }

        [Required(ErrorMessage = "Назва завдання обов'язкова")]
        [StringLength(200, ErrorMessage = "Назва завдання не може бути довшою за 200 символів")]
        [Column("TaskTitle")]
        public string TaskTitle { get; set; }

        [Required(ErrorMessage = "Опис завдання обов'язковий")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Вкажіть дату початку")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }  // <- робимо необов'язковим

        [Required(ErrorMessage = "Вкажіть клієнта")]
        public int? ClientId { get; set; }

        [ForeignKey(nameof(ClientId))]
        public Client? Client { get; set; }

        public int? ExecutorId { get; set; }

        [ForeignKey(nameof(ExecutorId))]
        public Executor? Executor { get; set; }  // <- головний виконавець

        public ICollection<ExecutorTask> ExecutorTasks { get; set; } = new List<ExecutorTask>();

        [Required(ErrorMessage = "Статус завдання обов'язковий")]
        [Column("TaskStatus")]
        public ClientTaskStatusEnum TaskStatus { get; set; }
    }


}
