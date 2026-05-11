using ClientsApp.BLL.Interfaces;
using ClientsApp.Models;
using ClientsApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Services
{
    public class ClientService : IClientService
    {
        private readonly ApplicationDbContext _context;

        public ClientService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Повертає весь список клієнтів для таблиці на головній сторінці довідника.
        public async Task<IEnumerable<Client>> GetAllAsync()
        {
            return await _context.Clients.ToListAsync();
        }

        // Шукає клієнта за первинним ключем; якщо запису немає, EF Core поверне null.
        public async Task<Client> GetByIdAsync(int id)
        {
            return await _context.Clients.FindAsync(id);
        }

        // Додає нового клієнта в DbSet Clients.
        public async Task AddAsync(Client client)
        {
            _context.Clients.Add(client);
            // Після SaveChangesAsync EF Core виконує INSERT у таблицю Clients.
            await _context.SaveChangesAsync();
        }

        // Оновлює існуючий запис після редагування форми клієнта.
        public async Task UpdateAsync(Client client)
        {
            _context.Clients.Update(client);
            // EF Core формує UPDATE лише для змінених полів відстежуваної сутності.
            await _context.SaveChangesAsync();
        }

        // Видаляє клієнта тільки якщо запис існує в базі.
        public async Task DeleteAsync(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
                // Підтверджуємо видалення SQL-командою DELETE.
                await _context.SaveChangesAsync();
            }
        }

        // Пошук для поля фільтра в UI: залишаємо клієнтів, у яких ім'я містить введений фрагмент.
        public async Task<IEnumerable<Client>> SearchByNameAsync(string namePart)
        {
            return await _context.Clients
                // ToLower з обох сторін робить порівняння нечутливим до регістру.
                .Where(c => c.Name.ToLower().Contains(namePart.ToLower()))
                .ToListAsync();
        }
    }
}
