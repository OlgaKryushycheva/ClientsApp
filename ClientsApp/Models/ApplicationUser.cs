// Файл ApplicationUser.cs містить допоміжний тип застосунку.
// Коментарі нижче пояснюють призначення ключових методів і правил валідації.
using ClientsApp.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace ClientsApp.Models
{
// ApplicationUser: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public class ApplicationUser : IdentityUser
    {
        public int? ExecutorId { get; set; }
        public Executor? Executor { get; set; }
    }
}
