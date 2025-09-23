using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Entities;
using TaskManagerAPI.Services;
namespace TaskManagerAPI.Data
{
    public class TaskManagerDbContext : DbContext
    {
        public TaskManagerDbContext(DbContextOptions<TaskManagerDbContext> options)
            : base(options)
        {
        }
        public DbSet<MyTask> MyTask { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<UserSessions> Session { get; set; }
        public DbSet<RefreshTokens> RefreshTokens { get; set; }
        public DbSet<TaskReminders> TaskReminders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Task And TaskReminders
            modelBuilder.Entity<MyTask>()
                .HasMany(t => t.Reminders)      // A task has many reminders
                .WithOne(r => r.TaskName)       // A reminder belongs to one task
                .HasForeignKey(r => r.TaskId)       // FK in TaskReminders
                .OnDelete(DeleteBehavior.Cascade);  // Cascade Delete , if a task is deleted then all its reminders are deleted
          
            //User And Task
            modelBuilder.Entity<User>()
                .HasMany(t => t.Tasks)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User And Session
            modelBuilder.Entity<User>()
                .HasMany(u => u.UserSessions)
                .WithOne(u => u.user)
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User And RefreshTokens
            modelBuilder.Entity<User>()
                .HasMany(u => u.RefreshTokens)
                .WithOne(u => u.user)
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
  }
}
