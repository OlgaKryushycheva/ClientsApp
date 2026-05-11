using ClientsApp.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment> AddPayment(Payment payment);
        Task<IEnumerable<Payment>> GetPaymentsByTaskId(int taskId);
        Task<Payment> GetPaymentById(int paymentId);
    }
}
