// Інтерфейс IClientService задає контракт методів для DI-контейнера.
// Завдяки цьому контролер залежить від абстракції, а не від конкретної реалізації.
﻿using ClientsApp.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClientsApp.Models;

namespace ClientsApp.BLL.Interfaces
{
// IClientService: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public interface IClientService
    {
        Task<IEnumerable<Client>> GetAllAsync();
        Task<Client> GetByIdAsync(int id);
        Task AddAsync(Client client);
        Task UpdateAsync(Client client);
        Task DeleteAsync(int id);
        Task<IEnumerable<Client>> SearchByNameAsync(string namePart);
    }
}
