using System.Reflection.PortableExecutable;
using TaskManagerAPI.Enums;

namespace TaskManagerAPI.Entities
{
    public class MyTask
    {
        public Guid MyTaskId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }   // Foreign key to User
        public required string Title { get; set; } 
        public string? Description { get; set; }
        public ProgressStatus State { get; set; }
        public TimeOnly DueTime { get; set; }
        public DateOnly DueDate{ get; set; }
        public DateTime CreatedAt { get; set; } 
        public DateTime? CompletedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        // Navigation
        public User User { get; set; }
    }
}
