// Репозиторій ExecutorRepository інкапсулює доступ до таблиць через EF Core.
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
// ExecutorRepository: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public class ExecutorRepository : IExecutorRepository
    {
        private readonly ApplicationDbContext _context;

        public ExecutorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

// Метод AddExecutor реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: Executor executor.
        public async Task<Executor> AddExecutor(Executor executor)
        {
            _context.Executors.Add(executor);
// SaveChangesAsync відправляє накопичені зміни в БД як SQL-команди INSERT/UPDATE/DELETE.
            await _context.SaveChangesAsync();
            return executor;
        }

// Метод GetAllExecutors реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
        public async Task<IEnumerable<Executor>> GetAllExecutors()
        {
            return await _context.Executors.ToListAsync();
        }

// Метод GetExecutorById реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int executorId.
        public async Task<Executor> GetExecutorById(int executorId)
        {
// FindAsync шукає рядок за первинним ключем; повертає null, якщо запис із таким ID відсутній.
            return await _context.Executors.FindAsync(executorId);
        }
    }
}
