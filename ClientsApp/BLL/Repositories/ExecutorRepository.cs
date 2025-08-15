using ClientsApp.BLL.Interfaces;
using ClientsApp.Models;
using ClientsApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Repositories
{
    public class ExecutorRepository : IExecutorRepository
    {
        private readonly ApplicationDbContext _context;

        public ExecutorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Executor> AddExecutor(Executor executor)
        {
            _context.Executors.Add(executor);
            await _context.SaveChangesAsync();
            return executor;
        }

        public async Task<IEnumerable<Executor>> GetAllExecutors()
        {
            return await _context.Executors.ToListAsync();
        }

        public async Task<Executor> GetExecutorById(int executorId)
        {
            return await _context.Executors.FindAsync(executorId);
        }
    }
}
