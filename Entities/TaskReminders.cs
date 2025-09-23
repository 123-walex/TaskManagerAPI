using TaskManagerAPI.Enums;

namespace TaskManagerAPI.Entities
{
    public class TaskReminders
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }  // Foreign key to the task
        public MyTask TaskName { get; set; }
        public TimeOnly DueTime { get; set; } // what time the reminder should trigger
        public DateOnly DueDate { get; set; } // what date it should trigger
        public DateTime? SentAt { get; set; } // When it was actually sent
        public ReminderStatus Status { get; set; } = ReminderStatus.Pending;
    }
}
