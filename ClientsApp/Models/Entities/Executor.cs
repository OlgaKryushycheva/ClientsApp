using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClientsApp.Models.Entities
{
    public class Executor : IValidatableObject
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

        [EmailAddress(ErrorMessage = "Введіть коректний email")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Недоступний з")]
        public DateTime? UnavailableFrom { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Недоступний до")]
        public DateTime? UnavailableTo { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var today = DateTime.Today;

            if (UnavailableFrom.HasValue && UnavailableFrom.Value.Date < today)
            {
                yield return new ValidationResult(
                    "Дата \"Недоступний з\" не може бути раніше поточної дати.",
                    new[] { nameof(UnavailableFrom) });
            }

            if (UnavailableFrom.HasValue && UnavailableTo.HasValue && UnavailableTo.Value.Date < UnavailableFrom.Value.Date)
            {
                yield return new ValidationResult(
                    "Дата \"Недоступний до\" не може бути раніше дати \"Недоступний з\".",
                    new[] { nameof(UnavailableTo) });
            }
        }

        public ICollection<ExecutorTask>? ExecutorTasks { get; set; }

        public ICollection<ClientTask> ClientTasks { get; set; } = new List<ClientTask>();
    }
}
