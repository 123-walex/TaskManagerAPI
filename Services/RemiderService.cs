using Hangfire.Storage.Monitoring;
using TaskManagerAPI.Data;
using TaskManagerAPI.Enums;
using TaskManagerAPI.Services.Notifications;

namespace TaskManagerAPI.Services
{
    public interface IReminderService
    {
        Task ScheduleTaskReminderAtDueTime(Guid taskId, string userEmail, DateTime remindAt);
        Task ScheduleTaskReminder30minsToDeadline(Guid taskId , string userEmail );
    }
    public static class ReminderPolicyHelper
    {
        public static List<DateTime> ReminderTimes (DateOnly DueDate , TimeOnly DueTime)
        {
            var remindAt = DueDate.ToDateTime(DueTime);
            var reminders = new List<DateTime>();
            
            // Example: Add the due time as a reminder
            reminders.Add(remindAt);

            // You can add more reminder times here as needed



            return reminders;
        }
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
        public async Task ScheduleTaskReminderAtDueTime(Guid taskId, string userEmail, DateTime remindAt)
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
        public async Task ScheduleTaskReminder30minsToDeadline(Guid taskId, string userEmail)
        {
            var task = await _context.MyTask.FindAsync(taskId);

            if (task == null)
                throw new Exception("Task Not found on the Db");

            var remindAt = task.DueTime.AddMinutes(-30); // 30 mins before duetime 
            var now = TimeOnly.FromDateTime(DateTime.UtcNow); //current time 

            if (now >= remindAt)
                return; // too late for reminder .

            string subject = $"Reminder: {task.Title}";
            string body = $"Your task '{task.Description}' is due today in 30 minutes .";

            _jobService.DelayedJob(
                () => _emailService.SendEmailAsync(userEmail, subject, body),
                remindAt - now 
            );
        }
    }
}
