using ClientsApp.BLL.Interfaces;
using ClientsApp.Models;
using ClientsApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Services
{
    public class ExecutorService : IExecutorService
    {
        private readonly ApplicationDbContext _context;

        public ExecutorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Executor>> GetAllAsync()
        {
            return await _context.Executors.ToListAsync();
        }

        public async Task<Executor> GetByIdAsync(int id)
        {
            return await _context.Executors.FindAsync(id);
        }

        public async Task AddAsync(Executor executor)
        {
            _context.Executors.Add(executor);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Executor executor)
        {
            _context.Executors.Update(executor);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var executor = await _context.Executors.FindAsync(id);
            if (executor != null)
            {
                _context.Executors.Remove(executor);
                await _context.SaveChangesAsync();
            }
        }
    }
}
