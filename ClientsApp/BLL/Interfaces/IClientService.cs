using ClientsApp.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClientsApp.Models;

namespace ClientsApp.BLL.Interfaces
{
    public interface IClientService
    {
        Task<IEnumerable<Client>> GetAllAsync();
        Task<Client> GetByIdAsync(int id);
        Task AddAsync(Client client);
        Task UpdateAsync(Client client);
        Task DeleteAsync(int id);
        Task<IEnumerable<Client>> SearchByNameAsync(string namePart);
    }
}
