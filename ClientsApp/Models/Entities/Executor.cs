using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClientsApp.Models.Entities
{
    public class Executor
    {
        public int ExecutorId { get; set; }

        [Required(ErrorMessage = "ПІБ виконавця обов'язкове")]
        [StringLength(100, ErrorMessage = "ПІБ не може бути довше 100 символів")]
        [RegularExpression(@"^[А-Яа-яІіЇїЄєҐґ\s]+$", ErrorMessage = "ПІБ може містити тільки літери та пробіли")]
        [Display(Name = "ПІБ")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Ставка за годину обов'язкова")]
        [Range(0.1, 10000, ErrorMessage = "Ставка має бути більше 0")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Введіть правильну ставку, наприклад 150.50")]
        [Display(Name = "Ставка за годину")]
        public decimal HourlyRate { get; set; }

        public ICollection<ExecutorTask>? ExecutorTasks { get; set; }

        public ICollection<ClientTask> ClientTasks { get; set; } = new List<ClientTask>();
    }
}
