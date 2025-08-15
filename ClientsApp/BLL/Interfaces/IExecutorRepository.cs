using ClientsApp.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Interfaces
{
    public interface IExecutorRepository
    {
        Task<Executor> AddExecutor(Executor executor);
        Task<IEnumerable<Executor>> GetAllExecutors();
        Task<Executor> GetExecutorById(int executorId);
    }
}
