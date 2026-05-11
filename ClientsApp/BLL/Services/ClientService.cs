// Сервіс ClientService містить прикладні правила для відповідної сутності.
// Методи сервісу координують запити до БД та підготовку даних для контролерів.
﻿using ClientsApp.BLL.Interfaces;
using ClientsApp.Models;
using ClientsApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Services
{
// ClientService: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public class ClientService : IClientService
    {
        private readonly ApplicationDbContext _context;

        public ClientService(ApplicationDbContext context)
        {
            _context = context;
        }

// Метод GetAllAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
        public async Task<IEnumerable<Client>> GetAllAsync()
        {
            return await _context.Clients.ToListAsync();
        }

// Метод GetByIdAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int id.
        public async Task<Client> GetByIdAsync(int id)
        {
// FindAsync шукає рядок за первинним ключем; повертає null, якщо запис із таким ID відсутній.
            return await _context.Clients.FindAsync(id);
        }

// Метод AddAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: Client client.
        public async Task AddAsync(Client client)
        {
            _context.Clients.Add(client);
// SaveChangesAsync відправляє накопичені зміни в БД як SQL-команди INSERT/UPDATE/DELETE.
            await _context.SaveChangesAsync();
        }

// Метод UpdateAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: Client client.
        public async Task UpdateAsync(Client client)
        {
            _context.Clients.Update(client);
// SaveChangesAsync відправляє накопичені зміни в БД як SQL-команди INSERT/UPDATE/DELETE.
            await _context.SaveChangesAsync();
        }

// Метод DeleteAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int id.
        public async Task DeleteAsync(int id)
        {
// FindAsync шукає рядок за первинним ключем; повертає null, якщо запис із таким ID відсутній.
            var client = await _context.Clients.FindAsync(id);
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (client != null)
            {
                _context.Clients.Remove(client);
// SaveChangesAsync відправляє накопичені зміни в БД як SQL-команди INSERT/UPDATE/DELETE.
                await _context.SaveChangesAsync();
            }
        }

// Метод SearchByNameAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: string namePart.
        public async Task<IEnumerable<Client>> SearchByNameAsync(string namePart)
        {
            return await _context.Clients
// Фільтр Where залишає лише записи, що відповідають умові, тому у View не потрапляють зайві дані.
                .Where(c => c.Name.ToLower().Contains(namePart.ToLower()))
                .ToListAsync();
        }
    }
}
