// Сервіс ClientTaskService містить прикладні правила для відповідної сутності.
// Методи сервісу координують запити до БД та підготовку даних для контролерів.
﻿using ClientsApp.BLL.Interfaces;
using ClientsApp.Models;
using ClientsApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientsApp.BLL
{
// ClientTaskService: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public class ClientTaskService : IClientTaskService
    {
        private readonly ApplicationDbContext _context;

        public ClientTaskService(ApplicationDbContext context)
        {
            _context = context;
        }

// Метод GetAllAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
        public async Task<IEnumerable<ClientTask>> GetAllAsync()
        {
            return await _context.ClientTasks
// Include підтягує зв’язані сутності в одному запиті, щоб у View не було додаткових звернень до БД.
                .Include(ct => ct.Client)
// Include підтягує зв’язані сутності в одному запиті, щоб у View не було додаткових звернень до БД.
                .Include(ct => ct.ExecutorTasks)
                .ThenInclude(et => et.Executor)
                .ToListAsync();
        }

// Метод GetByIdAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int id.
        public async Task<ClientTask> GetByIdAsync(int id)
        {
            return await _context.ClientTasks
// Include підтягує зв’язані сутності в одному запиті, щоб у View не було додаткових звернень до БД.
                .Include(ct => ct.Client)
// Include підтягує зв’язані сутності в одному запиті, щоб у View не було додаткових звернень до БД.
                .Include(ct => ct.ExecutorTasks)
                .ThenInclude(et => et.Executor)
                .FirstOrDefaultAsync(ct => ct.ClientTaskId == id);
        }

// Метод AddAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: ClientTask task.
        public async Task AddAsync(ClientTask task)
        {
            _context.ClientTasks.Add(task);
// SaveChangesAsync відправляє накопичені зміни в БД як SQL-команди INSERT/UPDATE/DELETE.
            await _context.SaveChangesAsync();
        }

// Метод UpdateAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: ClientTask task.
        public async Task UpdateAsync(ClientTask task)
        {
            var existingTask = await _context.ClientTasks
// Include підтягує зв’язані сутності в одному запиті, щоб у View не було додаткових звернень до БД.
                .Include(ct => ct.ExecutorTasks)
                .FirstOrDefaultAsync(ct => ct.ClientTaskId == task.ClientTaskId);

// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (existingTask == null) return;

            _context.Entry(existingTask).CurrentValues.SetValues(task);

            existingTask.ExecutorTasks.Clear();
            foreach (var et in task.ExecutorTasks)
            {
                existingTask.ExecutorTasks.Add(new ExecutorTask { ExecutorId = et.ExecutorId });
            }

// SaveChangesAsync відправляє накопичені зміни в БД як SQL-команди INSERT/UPDATE/DELETE.
            await _context.SaveChangesAsync();
        }

// Метод DeleteAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int id.
        public async Task DeleteAsync(int id)
        {
// FindAsync шукає рядок за первинним ключем; повертає null, якщо запис із таким ID відсутній.
            var task = await _context.ClientTasks.FindAsync(id);
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (task != null)
            {
                _context.ClientTasks.Remove(task);
// SaveChangesAsync відправляє накопичені зміни в БД як SQL-команди INSERT/UPDATE/DELETE.
                await _context.SaveChangesAsync();
            }
        }

// Метод SearchAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int? clientId, int? executorId, ClientTaskStatusEnum? status, bool sortByStartDateDescending = false.
        public async Task<IEnumerable<ClientTask>> SearchAsync(int? clientId, int? executorId, ClientTaskStatusEnum? status, bool sortByStartDateDescending = false)
        {
            var query = _context.ClientTasks
// Include підтягує зв’язані сутності в одному запиті, щоб у View не було додаткових звернень до БД.
                .Include(ct => ct.Client)
// Include підтягує зв’язані сутності в одному запиті, щоб у View не було додаткових звернень до БД.
                .Include(ct => ct.ExecutorTasks)
                .ThenInclude(et => et.Executor)
                .AsQueryable();

// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (clientId.HasValue)
// Фільтр Where залишає лише записи, що відповідають умові, тому у View не потрапляють зайві дані.
                query = query.Where(ct => ct.ClientId == clientId.Value);

// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (executorId.HasValue)
// Фільтр Where залишає лише записи, що відповідають умові, тому у View не потрапляють зайві дані.
                query = query.Where(ct => ct.ExecutorTasks.Any(et => et.ExecutorId == executorId.Value));

// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (status.HasValue)
// Фільтр Where залишає лише записи, що відповідають умові, тому у View не потрапляють зайві дані.
                query = query.Where(ct => ct.TaskStatus == status.Value);

            query = sortByStartDateDescending
// Це сортування формує передбачуваний порядок рядків у таблиці на сторінці.
                ? query.OrderByDescending(ct => ct.StartDate)
// Це сортування формує передбачуваний порядок рядків у таблиці на сторінці.
                : query.OrderBy(ct => ct.StartDate);

            return await query.ToListAsync();
        }
    }
}
