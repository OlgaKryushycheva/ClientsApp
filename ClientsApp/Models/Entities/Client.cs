using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClientsApp.Models.Entities
{
    public class Client
    {
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Ім'я обов'язкове")]
        [StringLength(50, ErrorMessage = "Ім'я не може бути довшим за 50 символів")]
        [Display(Name = "Ім'я")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Адреса обов'язкова")]
        [StringLength(200, ErrorMessage = "Адреса не може бути довшою за 200 символів")]
        [Display(Name = "Адреса")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Email обов'язковий")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Невірний формат Email")]
        [StringLength(100, ErrorMessage = "Email не може бути довшим за 100 символів")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Телефон обов'язковий")]
        [RegularExpression(@"^\+?\d{10,15}$", ErrorMessage = "Телефон має містити тільки цифри (10–15 символів)")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; }

        public ICollection<ClientTask>? ClientTasks { get; set; }
    }
}
