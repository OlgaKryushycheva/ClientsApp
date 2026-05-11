// Сутність Executor відповідає таблиці/даним предметної області.
// DataAnnotations нижче керують валідацією форми й мапінгом полів у БД.
﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClientsApp.Models.Entities
{
    using ClientsApp.Models;
// Executor: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public class Executor : IValidatableObject
    {
        public int ExecutorId { get; set; }

        [Required(ErrorMessage = "ПІБ виконавця обов'язкове")]
        [StringLength(100, ErrorMessage = "ПІБ не може бути довше 100 символів")]
        [RegularExpression(@"^[А-Яа-яІіЇїЄєҐґ\s]+$", ErrorMessage = "ПІБ може містити тільки літери та пробіли")]
        [Display(Name = "ПІБ")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Ставка за годину обов'язкова")]
        [Range(1, 10000, ErrorMessage = "Ставка має бути цілим числом більше 0")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Введіть ставку цілим числом без крапок і ком")]
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

        [DataType(DataType.Date)]
        [Display(Name = "Звільнений з дати")]
        public DateTime? DismissedFrom { get; set; }

// Метод Validate реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: ValidationContext validationContext.
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var today = DateTime.Today;

// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (UnavailableFrom.HasValue && UnavailableFrom.Value.Date < today)
            {
                yield return new ValidationResult(
                    "Дата \"Недоступний з\" не може бути раніше поточної дати.",
                    new[] { nameof(UnavailableFrom) });
            }

// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (UnavailableFrom.HasValue
                && UnavailableTo.HasValue
                && UnavailableTo.Value.Date < UnavailableFrom.Value.Date)
            {
                yield return new ValidationResult(
                    "Дата \"Недоступний до\" не може бути раніше дати \"Недоступний з\".",
                    new[] { nameof(UnavailableTo) });
            }

// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (DismissedFrom.HasValue && DismissedFrom.Value.Date < today)
            {
                yield return new ValidationResult(
                    "Дата \"Звільнений з дати\" не може бути раніше поточної дати.",
                    new[] { nameof(DismissedFrom) });
            }
        }
        public ICollection<ExecutorTask>? ExecutorTasks { get; set; }

        public ICollection<ClientTask> ClientTasks { get; set; } = new List<ClientTask>();
    }
}
