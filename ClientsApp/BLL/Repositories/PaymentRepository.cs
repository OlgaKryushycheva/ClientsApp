// Репозиторій PaymentRepository інкапсулює доступ до таблиць через EF Core.
// Тут зібрані CRUD-операції, щоб контролери/сервіси не працювали напряму з DbSet.
﻿using ClientsApp.BLL.Interfaces;
using ClientsApp.Models;
using ClientsApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Repositories
{
// PaymentRepository: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

// Метод AddPayment реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: Payment payment.
        public async Task<Payment> AddPayment(Payment payment)
        {
            _context.Payments.Add(payment);
// SaveChangesAsync відправляє накопичені зміни в БД як SQL-команди INSERT/UPDATE/DELETE.
            await _context.SaveChangesAsync();
            return payment;
        }

// Метод GetPaymentsByTaskId реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int taskId.
        public async Task<IEnumerable<Payment>> GetPaymentsByTaskId(int taskId)
        {
// Фільтр Where залишає лише записи, що відповідають умові, тому у View не потрапляють зайві дані.
            return await _context.Payments.Where(p => p.ClientTaskId == taskId).ToListAsync();
        }

// Метод GetPaymentById реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int paymentId.
        public async Task<Payment> GetPaymentById(int paymentId)
        {
// FindAsync шукає рядок за первинним ключем; повертає null, якщо запис із таким ID відсутній.
            return await _context.Payments.FindAsync(paymentId);
        }
    }
}
