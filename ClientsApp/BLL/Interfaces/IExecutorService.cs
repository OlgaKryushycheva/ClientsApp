using ClientsApp.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Interfaces
{
    public interface IExecutorService
    {
        Task<IEnumerable<Executor>> GetAllAsync();
        Task<Executor> GetByIdAsync(int id);
        Task AddAsync(Executor executor);
        Task UpdateAsync(Executor executor);
        Task DeleteAsync(int id);
    }
}
