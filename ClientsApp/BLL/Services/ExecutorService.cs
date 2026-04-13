using ClientsApp.BLL.Interfaces;
using ClientsApp.Models;
using ClientsApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

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
            await ClearExpiredUnavailablePeriodAsync();
            return await _context.Executors.ToListAsync();
        }

        public async Task<Executor> GetByIdAsync(int id)
        {
            await ClearExpiredUnavailablePeriodAsync();
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

        public async Task<IEnumerable<Executor>> SearchAsync(string fullName, decimal? hourlyRate)
        {
            await ClearExpiredUnavailablePeriodAsync();
            var query = _context.Executors.AsQueryable();

            if (!string.IsNullOrWhiteSpace(fullName))
            {
                var normalized = fullName.ToLower();
                query = query.Where(e => e.FullName.ToLower().Contains(normalized));
            }

            if (hourlyRate.HasValue)
            {
                query = query.Where(e => e.HourlyRate == hourlyRate.Value);
            }

            return await query.ToListAsync();
        }

        private async Task ClearExpiredUnavailablePeriodAsync()
        {
            var today = DateTime.Today;

            var expiredExecutors = await _context.Executors
                .Where(e => e.UnavailableTo.HasValue && e.UnavailableTo.Value.Date < today)
                .ToListAsync();

            if (expiredExecutors.Count == 0)
            {
                return;
            }

            foreach (var executor in expiredExecutors)
            {
                executor.UnavailableFrom = null;
                executor.UnavailableTo = null;
            }

            await _context.SaveChangesAsync();
        }
    }
}
