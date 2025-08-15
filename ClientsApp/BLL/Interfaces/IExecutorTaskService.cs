using ClientsApp.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Interfaces
{
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
