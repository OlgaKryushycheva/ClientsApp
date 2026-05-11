// Інтерфейс IPaymentService задає контракт методів для DI-контейнера.
// Завдяки цьому контролер залежить від абстракції, а не від конкретної реалізації.
﻿using ClientsApp.Models;
using ClientsApp.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Interfaces
{
// IPaymentService: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public interface IPaymentService
    {
        Task<IEnumerable<Payment>> GetAllAsync();
        Task<Payment> GetByIdAsync(int id);
        Task AddAsync(Payment payment);
        Task UpdateAsync(Payment payment);
        Task DeleteAsync(int id);
    }
}
