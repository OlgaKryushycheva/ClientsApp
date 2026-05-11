// Сервіс PaymentService містить прикладні правила для відповідної сутності.
// Методи сервісу координують запити до БД та підготовку даних для контролерів.
﻿using ClientsApp.BLL.Interfaces;
using ClientsApp.Models;
using ClientsApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Services
{
// PaymentService: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;

        public PaymentService(ApplicationDbContext context)
        {
            _context = context;
        }

// Метод GetAllAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _context.Payments
// Include підтягує зв’язані сутності в одному запиті, щоб у View не було додаткових звернень до БД.
                .Include(p => p.ClientTask)
                    .ThenInclude(ct => ct.Client)
                .ToListAsync();
        }

// Метод GetByIdAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int id.
        public async Task<Payment> GetByIdAsync(int id)
        {
            return await _context.Payments
// Include підтягує зв’язані сутності в одному запиті, щоб у View не було додаткових звернень до БД.
                .Include(p => p.ClientTask)
                    .ThenInclude(ct => ct.Client)
                .FirstOrDefaultAsync(p => p.PaymentId == id);
        }

// Метод AddAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: Payment payment.
        public async Task AddAsync(Payment payment)
        {
            _context.Payments.Add(payment);
// SaveChangesAsync відправляє накопичені зміни в БД як SQL-команди INSERT/UPDATE/DELETE.
            await _context.SaveChangesAsync();
        }

// Метод UpdateAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: Payment payment.
        public async Task UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
// SaveChangesAsync відправляє накопичені зміни в БД як SQL-команди INSERT/UPDATE/DELETE.
            await _context.SaveChangesAsync();
        }

// Метод DeleteAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int id.
        public async Task DeleteAsync(int id)
        {
// FindAsync шукає рядок за первинним ключем; повертає null, якщо запис із таким ID відсутній.
            var payment = await _context.Payments.FindAsync(id);
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (payment != null)
            {
                _context.Payments.Remove(payment);
// SaveChangesAsync відправляє накопичені зміни в БД як SQL-команди INSERT/UPDATE/DELETE.
                await _context.SaveChangesAsync();
            }
        }
    }
}
