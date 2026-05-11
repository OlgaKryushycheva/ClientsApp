// Інтерфейс IExecutorRepository задає контракт методів для DI-контейнера.
// Завдяки цьому контролер залежить від абстракції, а не від конкретної реалізації.
﻿using ClientsApp.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Interfaces
{
// IExecutorRepository: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public interface IExecutorRepository
    {
        Task<Executor> AddExecutor(Executor executor);
        Task<IEnumerable<Executor>> GetAllExecutors();
        Task<Executor> GetExecutorById(int executorId);
    }
}
