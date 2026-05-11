// Інтерфейс IPaymentRepository задає контракт методів для DI-контейнера.
// Завдяки цьому контролер залежить від абстракції, а не від конкретної реалізації.
﻿using ClientsApp.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Interfaces
{
// IPaymentRepository: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public interface IPaymentRepository
    {
        Task<Payment> AddPayment(Payment payment);
        Task<IEnumerable<Payment>> GetPaymentsByTaskId(int taskId);
        Task<Payment> GetPaymentById(int paymentId);
    }
}
