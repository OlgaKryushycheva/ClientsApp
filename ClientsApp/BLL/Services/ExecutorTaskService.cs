using ClientsApp.BLL.Interfaces;
using ClientsApp.Models;
using ClientsApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Services
{
    public class ExecutorTaskService : IExecutorTaskService
    {
        private readonly ApplicationDbContext _context;

        public ExecutorTaskService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ExecutorTask>> GetAllAsync(int? executorId = null, int? clientId = null, int? taskId = null)
        {
            var query = _context.ExecutorTasks
                .Include(et => et.Executor)
                .Include(et => et.ClientTask)
                    .ThenInclude(ct => ct.Client)
                .AsQueryable();

            if (executorId.HasValue)
            {
                query = query.Where(et => et.ExecutorId == executorId.Value);
            }

            if (clientId.HasValue)
            {
                query = query.Where(et => et.ClientTask!.ClientId == clientId.Value);
            }

            if (taskId.HasValue)
            {
                query = query.Where(et => et.ClientTaskId == taskId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<ExecutorTask> GetByIdAsync(int id)
        {
            return await _context.ExecutorTasks
                .Include(et => et.Executor)
                .Include(et => et.ClientTask)
                    .ThenInclude(ct => ct.Client)
                .FirstOrDefaultAsync(et => et.ExecutorTaskId == id);
        }

        public async Task AddAsync(ExecutorTask executorTask)
        {
            _context.ExecutorTasks.Add(executorTask);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ExecutorTask executorTask)
        {
            _context.ExecutorTasks.Update(executorTask);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.ExecutorTasks.FindAsync(id);
            if (entity != null)
            {
                _context.ExecutorTasks.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Client>> GetClientsByExecutorAsync(int executorId)
        {
            return await _context.ExecutorTasks
                .Where(et => et.ExecutorId == executorId)
                .Include(et => et.ClientTask)
                    .ThenInclude(ct => ct.Client)
                .Select(et => et.ClientTask!.Client!)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<ClientTask>> GetTasksByExecutorAndClientAsync(int executorId, int clientId)
        {
            return await _context.ExecutorTasks
                .Where(et => et.ExecutorId == executorId && et.ClientTask!.ClientId == clientId)
                .Select(et => et.ClientTask!)
                .Distinct()
                .ToListAsync();
        }
    }
}
