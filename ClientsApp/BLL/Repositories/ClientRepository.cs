using ClientsApp.BLL.Interfaces;
using ClientsApp.Models;
using ClientsApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Repositories
{
    // Репозиторій надає низькорівневий доступ до таблиці Clients через EF Core.
    public class ClientRepository : IClientRepository
    {
        private readonly ApplicationDbContext _context;

        public ClientRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Створює нового клієнта й повертає ту ж сутність після збереження.
        public async Task<Client> AddClient(Client client)
        {
            _context.Clients.Add(client);
            // Після SaveChangesAsync зміни потрапляють у БД, а client.ClientId заповнюється з identity-колонки.
            await _context.SaveChangesAsync();
            return client;
        }

        // Зчитує повний набір клієнтів для сценаріїв, де потрібен список без фільтрів.
        public async Task<IEnumerable<Client>> GetAllClients()
        {
            return await _context.Clients.ToListAsync();
        }

        // Отримує одного клієнта за ключем; повертає null, якщо такого ID немає.
        public async Task<Client> GetClientById(int clientId)
        {
            return await _context.Clients.FindAsync(clientId);
        }
    }
}
