using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using ClientsApp.Models.Entities;

namespace ClientsApp.Models.ViewModels
{
    public class ClientTaskIndexViewModel
    {
        public IEnumerable<ClientTask> Tasks { get; set; }
        public List<SelectListItem> Clients { get; set; }
        public List<SelectListItem> Executors { get; set; }
        public List<SelectListItem> Statuses { get; set; }

        public int? SelectedClientId { get; set; }
        public int? SelectedExecutorId { get; set; }
        public ClientTaskStatusEnum? SelectedStatus { get; set; }
        public string SortOrder { get; set; } = "asc";
    }
}
