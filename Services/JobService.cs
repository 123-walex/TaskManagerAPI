using Hangfire;
using Hangfire.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Linq.Expressions;
using System.Text;
using TaskManagerAPI.Data;
using TaskManagerAPI.Entities;
using TaskManagerAPI.Enums;
using TaskManagerAPI.Services;
using TaskManagerAPI.Services.Notifications;
namespace TaskManagerAPI.Services
{
    public interface IJobService
    {
        Task SendDailyReminders(string Email);
        Task SendNextReminder(string Email);
        void FireandForgetJob(Expression<Action> methodCall);
        void DelayedJob(Expression<Action> methodCall, TimeSpan delay);
        void ReccuringJob(string jobId, Expression<Action> methodCall, string cronExpression);
        void ContinuousJob(string parentJobId, Expression<Action> methodCall);
    }
}
public class JobService : IJobService
{
    private readonly TaskManagerDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;

    public JobService(TaskManagerDbContext context, IEmailService emailService, IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager)
    {
        _context = context;
        _emailService = emailService;
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
    }

    public async Task SendDailyReminders(string Email)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var DueTasks = await _context.MyTask
             .Where(t => t.DueDate == today && t.IsDeleted == DeletionStatus.False)
             .GroupBy(t => t.UserId)
             .ToListAsync();

        if (!DueTasks.Any())
            return;

        foreach (var dueTask in DueTasks)
        {
            var userId = dueTask.Key;
            var user = await _context.User.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.Email))
                continue;

            var tasksForUser = dueTask.ToList();

            // Build one email body for this user
            var bodyBuilder = new StringBuilder();
            bodyBuilder.AppendLine("<h3>Your tasks due today are :</h3>");
            bodyBuilder.AppendLine("<ul>");

            foreach (var task in tasksForUser)
            {
                bodyBuilder.AppendLine(
                    $"<li><b>{task.Title}</b> — {task.DueDate} at {task.DueTime}</li>"
                );
            } 

            bodyBuilder.AppendLine("</ul>");

            var subject = $"You have {tasksForUser.Count} task(s) due today";

            await _emailService.SendEmailAsync(user.Email, subject, bodyBuilder.ToString());
            return;
        }

    }
    public async Task SendNextReminder(string Email)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var task = await _context.MyTask
            .Where(x => x.DueDate == today 
             && x.IsDeleted == DeletionStatus.False
             && x.ReminderStatus == ReminderStatus.Pending)
            .GroupBy(x => x.UserId)
            .ToListAsync();

        if (!task.Any()) 
            return;

        foreach(var task in tasks)
        {
            var userId = task.Key;
            var user = await _context.User.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.Email))
                continue;
            //create reminder 
            // Build one email body for this users task
            var bodyBuilder = new StringBuilder();
            bodyBuilder.AppendLine($"<h3>Your Task {}  :</h3>");
            bodyBuilder.AppendLine("<ul>");

            foreach (var task in tasksForUser)
            {
                bodyBuilder.AppendLine(
                    $"<li><b>{task.Title}</b> — {task.DueDate} at {task.DueTime}</li>"
                );
            }

            bodyBuilder.AppendLine("</ul>");

            var subject = $"You have {tasksForUser.Count} task(s) due today";

            await _emailService.SendEmailAsync(user.Email, subject, bodyBuilder.ToString());
            return;
        }
    }
    public void FireandForgetJob(Expression<Action> methodCall)
    {
        _backgroundJobClient.Enqueue(methodCall);
    }

    public void DelayedJob(Expression<Action> methodCall, TimeSpan delay)
    {
        _backgroundJobClient.Schedule(methodCall, delay);
    }

    public void ReccuringJob(string jobId, Expression<Action> methodCall, string cronExpression)
    {
        _recurringJobManager.AddOrUpdate(jobId, methodCall, cronExpression);
    }

    public void ContinuousJob(string parentJobId, Expression<Action> methodCall)
    {
        BackgroundJob.ContinueJobWith(parentJobId, methodCall);
    }
}

