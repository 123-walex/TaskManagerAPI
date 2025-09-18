using TaskManagerAPI.Data;
using TaskManagerAPI.Services.Notifications;

namespace TaskManagerAPI.Services
{
    public interface IReminderService
    {
        Task ScheduleTaskReminder(Guid taskId, string userEmail, DateTime remindAt);
    }
    public class ReminderService : IReminderService
    {
        private readonly IJobService _jobService;
        private readonly IEmailService _emailService;
        private readonly TaskManagerDbContext _context;

        public ReminderService(IJobService jobService, IEmailService emailService, TaskManagerDbContext context)
        {
            _jobService = jobService;
            _emailService = emailService;
            _context = context;
        }
        public async Task ScheduleTaskReminder(Guid taskId, string userEmail, DateTime remindAt)
        {
            var task = await _context.MyTask.FindAsync(taskId);
            if (task == null) 
                throw new Exception("Task not found");

            string subject = $"Reminder: {task.Title}";
            string body = $"Your task '{task.Title}' is due on {task.DueDate} at {task.DueTime}.";

            _jobService.DelayedJob(
                () => _emailService.SendEmailAsync(userEmail, subject, body),
                remindAt - DateTime.UtcNow
            );
        }
    }
}
