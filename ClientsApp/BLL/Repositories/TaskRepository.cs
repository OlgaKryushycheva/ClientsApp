// Репозиторій TaskRepository інкапсулює доступ до таблиць через EF Core.
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
// TaskRepository: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public class TaskRepository : ITaskRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskRepository(ApplicationDbContext context)
        {
            _context = context;
        }

// Метод AddTask реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: ClientTask task.
        public async Task<ClientTask> AddTask(ClientTask task)
        {
            _context.ClientTasks.Add(task);
// SaveChangesAsync відправляє накопичені зміни в БД як SQL-команди INSERT/UPDATE/DELETE.
            await _context.SaveChangesAsync();
            return task;
        }

// Метод GetAllTasks реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
        public async Task<IEnumerable<ClientTask>> GetAllTasks()
        {
            return await _context.ClientTasks.ToListAsync();
        }

// Метод GetTaskById реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int taskId.
        public async Task<ClientTask> GetTaskById(int taskId)
        {
// FindAsync шукає рядок за первинним ключем; повертає null, якщо запис із таким ID відсутній.
            return await _context.ClientTasks.FindAsync(taskId);
        }
    }
}
