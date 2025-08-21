using Microsoft.EntityFrameworkCore;
using ClientsApp.Models.Entities;

namespace ClientsApp.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Executor> Executors { get; set; }
        public DbSet<ClientTask> ClientTasks { get; set; }
        public DbSet<ExecutorTask> ExecutorTasks { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClientTask>()
                .Property(t => t.TaskStatus)
                .HasConversion<int>();
            modelBuilder.Entity<ClientTask>()
                .HasOne(t => t.Executor)
                .WithMany(e => e.ClientTasks)
                .HasForeignKey(t => t.ExecutorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Executor>()
                .Property(e => e.HourlyRate)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ExecutorTask>()
                .Property(et => et.ActualTime)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ExecutorTask>()
                .Property(et => et.AdjustedTime)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Payment>()
                .Property(p => p.BalanceDue)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ExecutorTask>()
                .HasOne(et => et.Executor)
                .WithMany(e => e.ExecutorTasks)
                .HasForeignKey(et => et.ExecutorId);
            modelBuilder.Entity<ExecutorTask>()
                .HasOne(et => et.ClientTask)
                .WithMany(ct => ct.ExecutorTasks)
                .HasForeignKey(et => et.ClientTaskId);

            base.OnModelCreating(modelBuilder);
        }

    }
}
