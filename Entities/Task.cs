using System.Reflection.PortableExecutable;

namespace TaskManagerAPI.Entities
{
    public class Task
    {
        public Guid TaskId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }   // Foreign key to User
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskStatus State { get; set; }
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
