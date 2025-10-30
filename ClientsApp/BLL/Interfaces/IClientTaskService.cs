using ClientsApp.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Interfaces
{
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
