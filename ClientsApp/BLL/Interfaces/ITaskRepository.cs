// Інтерфейс ITaskRepository задає контракт методів для DI-контейнера.
// Завдяки цьому контролер залежить від абстракції, а не від конкретної реалізації.
﻿using ClientsApp.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Interfaces
{
// ITaskRepository: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public interface ITaskRepository
    {
        Task<ClientTask> AddTask(ClientTask task);
        Task<IEnumerable<ClientTask>> GetAllTasks();
        Task<ClientTask> GetTaskById(int taskId);
    }
}
