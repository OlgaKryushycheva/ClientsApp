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
        public string TaskTitle { get; set; }

        [Required(ErrorMessage = "Опис завдання обов'язковий")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Вкажіть дату початку")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "Вкажіть клієнта")]
        public int? ClientId { get; set; }

        [Required(ErrorMessage = "Статус завдання обов'язковий")]
        public ClientTaskStatusEnum TaskStatus { get; set; }

        public List<int> SelectedExecutors { get; set; } = new();

        public SelectList Clients { get; set; }
        public MultiSelectList Executors { get; set; }
        public SelectList Statuses { get; set; }
    }
}
