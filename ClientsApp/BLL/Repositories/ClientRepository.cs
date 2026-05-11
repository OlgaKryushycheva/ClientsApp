using ClientsApp.BLL.Interfaces;
using ClientsApp.Models;
using ClientsApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly ApplicationDbContext _context;

        public ClientRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Створює нового клієнта і повертає сутність після збереження.
        public async Task<Client> AddClient(Client client)
        {
            _context.Clients.Add(client);
            // Після INSERT база заповнює identity-ключ, тому client.ClientId стає доступним у пам'яті.
            await _context.SaveChangesAsync();
            return client;
        }

        // Повертає всі записи з таблиці Clients без додаткової фільтрації.
        public async Task<IEnumerable<Client>> GetAllClients()
        {
            return await _context.Clients.ToListAsync();
        }

        // Шукає одного клієнта за ID; якщо рядок відсутній, результат буде null.
        public async Task<Client> GetClientById(int clientId)
        {
            return await _context.Clients.FindAsync(clientId);
        }
    }
}
