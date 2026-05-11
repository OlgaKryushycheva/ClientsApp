// Сервіс ExecutorService містить прикладні правила для відповідної сутності.
// Методи сервісу координують запити до БД та підготовку даних для контролерів.
﻿using ClientsApp.BLL.Interfaces;
using ClientsApp.Models;
using ClientsApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ClientsApp.BLL.Services
{
// ExecutorService: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public class ExecutorService : IExecutorService
    {
        private readonly ApplicationDbContext _context;

        public ExecutorService(ApplicationDbContext context)
        {
            _context = context;
        }

// Метод GetAllAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
        public async Task<IEnumerable<Executor>> GetAllAsync()
        {
            await ClearExpiredUnavailablePeriodAsync();
            return await _context.Executors.ToListAsync();
        }

// Метод GetByIdAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int id.
        public async Task<Executor?> GetByIdAsync(int id)
        {
            await ClearExpiredUnavailablePeriodAsync();
// FindAsync шукає рядок за первинним ключем; повертає null, якщо запис із таким ID відсутній.
            return await _context.Executors.FindAsync(id);
        }

// Метод AddAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: Executor executor.
        public async Task AddAsync(Executor executor)
        {
            _context.Executors.Add(executor);
// SaveChangesAsync відправляє накопичені зміни в БД як SQL-команди INSERT/UPDATE/DELETE.
            await _context.SaveChangesAsync();
        }

// Метод UpdateAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: Executor executor.
        public async Task UpdateAsync(Executor executor)
        {
            _context.Executors.Update(executor);
// SaveChangesAsync відправляє накопичені зміни в БД як SQL-команди INSERT/UPDATE/DELETE.
            await _context.SaveChangesAsync();
        }

// Метод DeleteAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int id.
        public async Task DeleteAsync(int id)
        {
// FindAsync шукає рядок за первинним ключем; повертає null, якщо запис із таким ID відсутній.
            var executor = await _context.Executors.FindAsync(id);
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (executor != null)
            {
                _context.Executors.Remove(executor);
// SaveChangesAsync відправляє накопичені зміни в БД як SQL-команди INSERT/UPDATE/DELETE.
                await _context.SaveChangesAsync();
            }
        }

// Метод SearchAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: string? fullName, decimal? hourlyRate.
        public async Task<IEnumerable<Executor>> SearchAsync(string? fullName, decimal? hourlyRate)
        {
            await ClearExpiredUnavailablePeriodAsync();
            var query = _context.Executors.AsQueryable();

// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (!string.IsNullOrWhiteSpace(fullName))
            {
                var normalized = fullName.ToLower();
// Фільтр Where залишає лише записи, що відповідають умові, тому у View не потрапляють зайві дані.
                query = query.Where(e => e.FullName.ToLower().Contains(normalized));
            }

// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (hourlyRate.HasValue)
            {
// Фільтр Where залишає лише записи, що відповідають умові, тому у View не потрапляють зайві дані.
                query = query.Where(e => e.HourlyRate == hourlyRate.Value);
            }

            return await query.ToListAsync();
        }

        private async Task ClearExpiredUnavailablePeriodAsync()
        {
            var today = DateTime.Today;

            var expiredExecutors = await _context.Executors
// Фільтр Where залишає лише записи, що відповідають умові, тому у View не потрапляють зайві дані.
                .Where(e => e.UnavailableTo.HasValue && e.UnavailableTo.Value.Date < today)
                .ToListAsync();

// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (expiredExecutors.Count == 0)
            {
                return;
            }

            foreach (var executor in expiredExecutors)
            {
                executor.UnavailableFrom = null;
                executor.UnavailableTo = null;
            }

// SaveChangesAsync відправляє накопичені зміни в БД як SQL-команди INSERT/UPDATE/DELETE.
            await _context.SaveChangesAsync();
        }
    }
}
