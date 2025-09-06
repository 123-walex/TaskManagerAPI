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
        //public DbSet<Tasks> Task { get; set; }
        public DbSet<User> User { get; set; }

        internal async Task FindAsync(Guid? userId)
        {
            throw new NotImplementedException();
        }
    }   
}
