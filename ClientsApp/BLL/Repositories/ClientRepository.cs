// Репозиторій ClientRepository інкапсулює доступ до таблиць через EF Core.
// Тут зібрані CRUD-операції, щоб контролери/сервіси не працювали напряму з DbSet.
﻿using ClientsApp.BLL.Interfaces;
using ClientsApp.Models;
using ClientsApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Repositories
{
// ClientRepository: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public class ClientRepository : IClientRepository
    {
        private readonly ApplicationDbContext _context;

        public ClientRepository(ApplicationDbContext context)
        {
            _context = context;
        }

// Метод AddClient реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: Client client.
        public async Task<Client> AddClient(Client client)
        {
            _context.Clients.Add(client);
// SaveChangesAsync відправляє накопичені зміни в БД як SQL-команди INSERT/UPDATE/DELETE.
            await _context.SaveChangesAsync();
            return client;
        }

// Метод GetAllClients реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
        public async Task<IEnumerable<Client>> GetAllClients()
        {
            return await _context.Clients.ToListAsync();
        }

// Метод GetClientById реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int clientId.
        public async Task<Client> GetClientById(int clientId)
        {
// FindAsync шукає рядок за первинним ключем; повертає null, якщо запис із таким ID відсутній.
            return await _context.Clients.FindAsync(clientId);
        }
    }
}
