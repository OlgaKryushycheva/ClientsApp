// Сервіс ExecutorTaskService містить прикладні правила для відповідної сутності.
// Методи сервісу координують запити до БД та підготовку даних для контролерів.
using ClientsApp.BLL.Interfaces;
using ClientsApp.Models;
using ClientsApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Services
{
// ExecutorTaskService: основний тип у цьому файлі, який визначає структуру даних або контракт поведінки.
    public class ExecutorTaskService : IExecutorTaskService
    {
        private readonly ApplicationDbContext _context;

        public ExecutorTaskService(ApplicationDbContext context)
        {
            _context = context;
        }

// Метод GetAllAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int? executorId = null, int? clientId = null, int? taskId = null.
        public async Task<IEnumerable<ExecutorTask>> GetAllAsync(int? executorId = null, int? clientId = null, int? taskId = null)
        {
            var query = _context.ExecutorTasks
// Include підтягує зв’язані сутності в одному запиті, щоб у View не було додаткових звернень до БД.
                .Include(et => et.Executor)
// Include підтягує зв’язані сутності в одному запиті, щоб у View не було додаткових звернень до БД.
                .Include(et => et.ClientTask)
                    .ThenInclude(ct => ct.Client)
                .AsQueryable();

// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (executorId.HasValue)
            {
// Фільтр Where залишає лише записи, що відповідають умові, тому у View не потрапляють зайві дані.
                query = query.Where(et => et.ExecutorId == executorId.Value);
            }

// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (clientId.HasValue)
            {
// Фільтр Where залишає лише записи, що відповідають умові, тому у View не потрапляють зайві дані.
                query = query.Where(et => et.ClientTask!.ClientId == clientId.Value);
            }

// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (taskId.HasValue)
            {
// Фільтр Where залишає лише записи, що відповідають умові, тому у View не потрапляють зайві дані.
                query = query.Where(et => et.ClientTaskId == taskId.Value);
            }

            return await query.ToListAsync();
        }

// Метод GetByIdAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int id.
        public async Task<ExecutorTask> GetByIdAsync(int id)
        {
            return await _context.ExecutorTasks
// Include підтягує зв’язані сутності в одному запиті, щоб у View не було додаткових звернень до БД.
                .Include(et => et.Executor)
// Include підтягує зв’язані сутності в одному запиті, щоб у View не було додаткових звернень до БД.
                .Include(et => et.ClientTask)
                    .ThenInclude(ct => ct.Client)
                .FirstOrDefaultAsync(et => et.ExecutorTaskId == id);
        }

// Метод AddAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: ExecutorTask executorTask.
        public async Task AddAsync(ExecutorTask executorTask)
        {
            _context.ExecutorTasks.Add(executorTask);
// SaveChangesAsync відправляє накопичені зміни в БД як SQL-команди INSERT/UPDATE/DELETE.
            await _context.SaveChangesAsync();
        }

// Метод UpdateAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: ExecutorTask executorTask.
        public async Task UpdateAsync(ExecutorTask executorTask)
        {
            _context.ExecutorTasks.Update(executorTask);
// SaveChangesAsync відправляє накопичені зміни в БД як SQL-команди INSERT/UPDATE/DELETE.
            await _context.SaveChangesAsync();
        }

// Метод DeleteAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int id.
        public async Task DeleteAsync(int id)
        {
// FindAsync шукає рядок за первинним ключем; повертає null, якщо запис із таким ID відсутній.
            var entity = await _context.ExecutorTasks.FindAsync(id);
// Умова нижче відсікає невалідний або небезпечний шлях виконання перед зміною даних.
            if (entity != null)
            {
                _context.ExecutorTasks.Remove(entity);
// SaveChangesAsync відправляє накопичені зміни в БД як SQL-команди INSERT/UPDATE/DELETE.
                await _context.SaveChangesAsync();
            }
        }

// Метод GetClientsByExecutorAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int executorId.
        public async Task<IEnumerable<Client>> GetClientsByExecutorAsync(int executorId)
        {
            return await _context.ExecutorTasks
// Фільтр Where залишає лише записи, що відповідають умові, тому у View не потрапляють зайві дані.
                .Where(et => et.ExecutorId == executorId)
// Include підтягує зв’язані сутності в одному запиті, щоб у View не було додаткових звернень до БД.
                .Include(et => et.ClientTask)
                    .ThenInclude(ct => ct.Client)
                .Select(et => et.ClientTask!.Client!)
                .Distinct()
                .ToListAsync();
        }

// Метод GetTasksByExecutorAndClientAsync реалізує конкретний крок сценарію, що видно з його назви та тіла нижче.
// Параметри методу: int executorId, int clientId.
        public async Task<IEnumerable<ClientTask>> GetTasksByExecutorAndClientAsync(int executorId, int clientId)
        {
            return await _context.ExecutorTasks
// Фільтр Where залишає лише записи, що відповідають умові, тому у View не потрапляють зайві дані.
                .Where(et => et.ExecutorId == executorId && et.ClientTask!.ClientId == clientId)
                .Select(et => et.ClientTask!)
                .Distinct()
                .ToListAsync();
        }
    }
}
