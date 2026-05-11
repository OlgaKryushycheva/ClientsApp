// Інтерфейс IExecutorService задає контракт методів для DI-контейнера.
// Завдяки цьому контролер залежить від абстракції, а не від конкретної реалізації.
﻿using ClientsApp.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Interfaces
{
// IExecutorService: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public interface IExecutorService
    {
        Task<IEnumerable<Executor>> GetAllAsync();
        Task<Executor?> GetByIdAsync(int id);
        Task AddAsync(Executor executor);
        Task UpdateAsync(Executor executor);
        Task DeleteAsync(int id);
        Task<IEnumerable<Executor>> SearchAsync(string? fullName, decimal? hourlyRate);
    }
}
