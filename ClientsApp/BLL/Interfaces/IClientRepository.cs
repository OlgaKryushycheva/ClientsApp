// Інтерфейс IClientRepository задає контракт методів для DI-контейнера.
// Завдяки цьому контролер залежить від абстракції, а не від конкретної реалізації.
﻿using ClientsApp.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Interfaces
{
// IClientRepository: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public interface IClientRepository
    {
        Task<Client> AddClient(Client client);
        Task<IEnumerable<Client>> GetAllClients();
        Task<Client> GetClientById(int clientId);
    }
}
