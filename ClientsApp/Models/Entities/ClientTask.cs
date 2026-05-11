// Сутність ClientTask відповідає таблиці/даним предметної області.
// DataAnnotations нижче керують валідацією форми й мапінгом полів у БД.
﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientsApp.Models.Entities
{
// ClientTask: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public class ClientTask
    {
        [Key]
        public int ClientTaskId { get; set; }

        [Required(ErrorMessage = "Назва завдання обов'язкова")]
        [StringLength(200, ErrorMessage = "Назва завдання не може бути довшою за 200 символів")]
        [Column("TaskTitle")]
        [Display(Name = "Назва завдання")]
        public string TaskTitle { get; set; }

        [Required(ErrorMessage = "Опис завдання обов'язковий")]
        [Display(Name = "Опис")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Вкажіть дату початку")]
        [DataType(DataType.Date)]
        [Display(Name = "Дата початку")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Дата завершення")]
        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "Вкажіть клієнта")]
        [Display(Name = "Клієнт")]
        public int? ClientId { get; set; }

        [ForeignKey(nameof(ClientId))]
        public Client? Client { get; set; }

        [Display(Name = "Виконавець")]
        public int? ExecutorId { get; set; }

        [ForeignKey(nameof(ExecutorId))]
        public Executor? Executor { get; set; }

        public ICollection<ExecutorTask> ExecutorTasks { get; set; } = new List<ExecutorTask>();

        [Required(ErrorMessage = "Статус завдання обов'язковий")]
        [Column("TaskStatus")]
        [Display(Name = "Статус")]
        public ClientTaskStatusEnum TaskStatus { get; set; }
    }


}
