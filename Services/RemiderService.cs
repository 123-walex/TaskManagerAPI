using Hangfire;
using Hangfire.Storage.Monitoring;
using TaskManagerAPI.Data;
using TaskManagerAPI.Enums;
using TaskManagerAPI.Services.Notifications;

namespace TaskManagerAPI.Services
{
    public interface IReminderService
    {
        void ScheduleDailyDigest();
        Task ScheduleTaskReminder(Guid taskId, string userEmail, DateOnly DueDate , TimeOnly DueTime , TaskPolicy Policy);
    }
    public static class ReminderPolicySetter
    {
        public static List<DateTime> GetReminderTimes (DateOnly DueDate , TimeOnly DueTime , TaskPolicy policy)
        {
            var remindAt = DueDate.ToDateTime(DueTime);
            var reminders = new List<DateTime>();
            
            switch(policy)
            {
                case TaskPolicy.Low:
                    reminders.Add(remindAt); //On DueTime
                    break;

                case TaskPolicy.Normal:
                    reminders.Add(remindAt.AddDays(-1)); // One day to duetime
                    reminders.Add(remindAt.AddHours(-1)); //one hour to due time
                    reminders.Add(remindAt.AddMinutes(-30)); // 30 Minutes Before 
                    reminders.Add(remindAt); // on duetime
                    break;

                case TaskPolicy.High:
                    reminders.Add(remindAt.AddDays(-2));// 2 Days to duetime
                    reminders.Add(remindAt.AddDays(-1)); // 1Day to duetime 
                    reminders.Add(remindAt.AddHours(-1));// 1 Hour Before
                    reminders.Add(remindAt.AddMinutes(-30)); // 30 Minutes Before
                    reminders.Add(remindAt); // Duetime
                    break;
            }
            // only keep reminders that are still in the future
            return reminders.Where(r => r > DateTime.UtcNow).ToList();
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
        // Schedule The Job For Sending Daily Digests Using JobService
        public void ScheduleDailyDigest()
        {
            _jobService.RecurringJob(
                "Tasks Due Today .",                           // Job name (unique key)
                () => _jobService.SendDailyReminderList(),     // The calling Method
                Cron.Daily(8, 0)                               // Every day at 08 : 00 UTC
            );
        }
        public async Task ScheduleTaskReminder(Guid taskId, string userEmail, DateOnly DueDate , TimeOnly DueTime , TaskPolicy Policy)
        {
            // Schedule Reminders Based On Policy 
            var task = await _context.MyTask.FindAsync(taskId);
            if (task == null) 
                throw new Exception("Task not found");

            var reminders = ReminderPolicySetter.GetReminderTimes(DueDate, DueTime, Policy);

            foreach(var reminder in reminders)
            {
                string subject = $"Reminder: {task.Title}";
                string body = $"Your task '{task.Title}' is due on {task.DueDate} at {task.DueTime}.";

                _jobService.DelayedJob(
                () => _emailService.SendEmailAsync(userEmail, subject, body),
                reminder - DateTime.UtcNow
               );
            }
        }
    }
}
