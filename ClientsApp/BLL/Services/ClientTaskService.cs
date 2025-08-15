﻿using ClientsApp.BLL.Interfaces;
using ClientsApp.Models;
using ClientsApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientsApp.BLL
{
    public class ClientTaskService : IClientTaskService
    {
        private readonly ApplicationDbContext _context;

        public ClientTaskService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ClientTask>> GetAllAsync()
        {
            return await _context.ClientTasks
                .Include(ct => ct.Client)
                .Include(ct => ct.ExecutorTasks)
                .ThenInclude(et => et.Executor)
                .ToListAsync();
        }

        public async Task<ClientTask> GetByIdAsync(int id)
        {
            return await _context.ClientTasks
                .Include(ct => ct.Client)
                .Include(ct => ct.ExecutorTasks)
                .ThenInclude(et => et.Executor)
                .FirstOrDefaultAsync(ct => ct.ClientTaskId == id);
        }

        public async Task AddAsync(ClientTask task)
        {
            _context.ClientTasks.Add(task);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ClientTask task)
        {
            _context.ClientTasks.Update(task);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var task = await _context.ClientTasks.FindAsync(id);
            if (task != null)
            {
                _context.ClientTasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ClientTask>> SearchAsync(int? clientId, int? executorId, ClientTaskStatusEnum? status)
        {
            var query = _context.ClientTasks
                .Include(ct => ct.Client)
                .Include(ct => ct.ExecutorTasks)
                .ThenInclude(et => et.Executor)
                .AsQueryable();

            if (clientId.HasValue)
                query = query.Where(ct => ct.ClientId == clientId.Value);

            if (executorId.HasValue)
                query = query.Where(ct => ct.ExecutorTasks.Any(et => et.ExecutorId == executorId.Value));

            if (status.HasValue)
                query = query.Where(ct => ct.TaskStatus == status.Value);

            return await query.ToListAsync();
        }
    }
}
