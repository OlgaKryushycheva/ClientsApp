using ClientsApp.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Interfaces
{
    public interface IClientRepository
    {
        Task<Client> AddClient(Client client);
        Task<IEnumerable<Client>> GetAllClients();
        Task<Client> GetClientById(int clientId);
    }
}
