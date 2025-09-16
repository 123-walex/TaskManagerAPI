using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Entities;

namespace TaskManagerAPI.Data
{
    public class TaskManagerDbContext : DbContext
    {
        public TaskManagerDbContext(DbContextOptions<TaskManagerDbContext> options)
            : base(options)
        {
        }
        public DbSet<Entities.Task> Task { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<UserSessions> Session { get; set; }
        public DbSet<RefreshTokens> RefreshTokens { get; set; }
    }   
}
