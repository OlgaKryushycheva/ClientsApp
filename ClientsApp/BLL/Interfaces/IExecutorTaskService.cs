// Інтерфейс IExecutorTaskService задає контракт методів для DI-контейнера.
// Завдяки цьому контролер залежить від абстракції, а не від конкретної реалізації.
using ClientsApp.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Interfaces
{
// IExecutorTaskService: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public interface IExecutorTaskService
    {
        Task<IEnumerable<ExecutorTask>> GetAllAsync(int? executorId = null, int? clientId = null, int? taskId = null);
        Task<ExecutorTask> GetByIdAsync(int id);
        Task AddAsync(ExecutorTask executorTask);
        Task UpdateAsync(ExecutorTask executorTask);
        Task DeleteAsync(int id);

        Task<IEnumerable<Client>> GetClientsByExecutorAsync(int executorId);
        Task<IEnumerable<ClientTask>> GetTasksByExecutorAndClientAsync(int executorId, int clientId);
    }
}
