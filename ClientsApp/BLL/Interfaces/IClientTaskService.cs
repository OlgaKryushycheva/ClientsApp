// Інтерфейс IClientTaskService задає контракт методів для DI-контейнера.
// Завдяки цьому контролер залежить від абстракції, а не від конкретної реалізації.
﻿using ClientsApp.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Interfaces
{
// IClientTaskService: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public interface IClientTaskService
    {
        Task<IEnumerable<ClientTask>> GetAllAsync();
        Task<ClientTask> GetByIdAsync(int id);
        Task AddAsync(ClientTask task);
        Task UpdateAsync(ClientTask task);
        Task DeleteAsync(int id);

        Task<IEnumerable<ClientTask>> SearchAsync(int? clientId, int? executorId, ClientTaskStatusEnum? status, bool sortByStartDateDescending = false);

    }
}
