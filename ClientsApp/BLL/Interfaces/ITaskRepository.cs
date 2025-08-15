using ClientsApp.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Interfaces
{
    public interface ITaskRepository
    {
        Task<ClientTask> AddTask(ClientTask task);
        Task<IEnumerable<ClientTask>> GetAllTasks();
        Task<ClientTask> GetTaskById(int taskId);
    }
}
