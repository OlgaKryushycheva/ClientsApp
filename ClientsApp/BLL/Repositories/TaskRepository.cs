using ClientsApp.BLL.Interfaces;
using ClientsApp.Models;
using ClientsApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientsApp.BLL.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ClientTask> AddTask(ClientTask task)
        {
            _context.ClientTasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<IEnumerable<ClientTask>> GetAllTasks()
        {
            return await _context.ClientTasks.ToListAsync();
        }

        public async Task<ClientTask> GetTaskById(int taskId)
        {
            return await _context.ClientTasks.FindAsync(taskId);
        }
    }
}
