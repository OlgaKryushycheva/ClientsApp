using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ClientsApp.Models.Entities;

namespace ClientsApp.Models.ViewModels
{
    public class ClientTaskEditViewModel
    {
        public int ClientTaskId { get; set; }

        [Required(ErrorMessage = "Назва завдання обов'язкова")]
        [StringLength(200, ErrorMessage = "Назва завдання не може бути довшою за 200 символів")]
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

        [Required(ErrorMessage = "Статус завдання обов'язковий")]
        [Display(Name = "Статус")]
        public ClientTaskStatusEnum TaskStatus { get; set; }

        public List<int> SelectedExecutors { get; set; } = new();

        public SelectList? Clients { get; set; }
        public MultiSelectList? Executors { get; set; }
        public SelectList? Statuses { get; set; }
    }
}
