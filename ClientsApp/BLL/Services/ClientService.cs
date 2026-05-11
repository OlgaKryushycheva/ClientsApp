using ClientsApp.BLL.Interfaces;
using ClientsApp.Models;
using ClientsApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Services
{
    // Сервіс інкапсулює базові операції з клієнтами для контролерів:
    // читання списку, пошук, створення, оновлення та видалення.
    public class ClientService : IClientService
    {
        private readonly ApplicationDbContext _context;

        public ClientService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Повертає всіх клієнтів для таблиці на сторінці Index.
        public async Task<IEnumerable<Client>> GetAllAsync()
        {
            return await _context.Clients.ToListAsync();
        }

        // Шукає одного клієнта за первинним ключем ClientId.
        // Потрібно для форм редагування/видалення, де приходить конкретний ID.
        public async Task<Client> GetByIdAsync(int id)
        {
            return await _context.Clients.FindAsync(id);
        }

        // Додає нового клієнта до контексту та зберігає його в таблиці Clients.
        public async Task AddAsync(Client client)
        {
            _context.Clients.Add(client);
            // На цьому кроці EF Core виконує SQL INSERT і присвоює ClientId новому запису.
            await _context.SaveChangesAsync();
        }

        // Оновлює існуючий запис клієнта після редагування у формі.
        public async Task UpdateAsync(Client client)
        {
            _context.Clients.Update(client);
            // SaveChangesAsync застосовує змінені поля до таблиці через SQL UPDATE.
            await _context.SaveChangesAsync();
        }

        // Видаляє клієнта за ID, якщо такий запис реально існує в базі.
        public async Task DeleteAsync(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            // Якщо FindAsync повернув null, видаляти нічого — тихо завершуємо метод без помилки.
            if (client != null)
            {
                _context.Clients.Remove(client);
                // Після Remove виконується SQL DELETE, і клієнт зникає зі списків у UI.
                await _context.SaveChangesAsync();
            }
        }

        // Повертає клієнтів, чиє ім'я містить введений фрагмент namePart.
        public async Task<IEnumerable<Client>> SearchByNameAsync(string namePart)
        {
            return await _context.Clients
                // ToLower з обох боків робить пошук нечутливим до регістру (напр. "Іван" == "іван").
                .Where(c => c.Name.ToLower().Contains(namePart.ToLower()))
                .ToListAsync();
        }
    }
}
