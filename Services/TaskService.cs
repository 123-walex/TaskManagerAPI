using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTO_s;
using TaskManagerAPI.Entities;
using TaskManagerAPI.Enums;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace TaskManagerAPI.Services
{
    public interface ITaskService
    {
        public Task<TaskResponse> CreateTask(CreateTask create , TaskPolicy policy);
        public Task<TaskResponse> GetTask(Guid MyTaskId);
        public Task<List<TaskResponse>> GetAllTasks();
        public Task<TaskResponse> TotalUpdateTask(TotalUpdateTaskDTO newtask, Guid MyTaskId);
        public Task<bool> DeleteTask(Guid MyTaskId);
        public Task<List<TaskResponse>> GetTasksByDueDate(DateOnly dueDate);
        public Task<List<TaskResponse>> GetOverdueTasks();
        public Task<List<TaskResponse>> GetTasksByStatus(ProgressStatus status);
    }
    // remember to use the py visualizaion library for your tasks
    // add an ml model to predict task completion time based on past data
    public class TaskService : ITaskService
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TaskService> _logger;
        private readonly TaskManagerDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ItokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly IReminderService _reminderservice;
        public TaskService
            (
            IHttpContextAccessor httpContextAccessor,
            ILogger<TaskService> logger,
            TaskManagerDbContext context,
            PasswordHasher<User> passwordHasher,
            ItokenService tokenService,
            IConfiguration configuration, 
            IReminderService reminderService
            )

        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _configuration = configuration;
            _reminderservice = reminderService;
        }
        private ClaimsPrincipal? CurrentUser => _httpContextAccessor.HttpContext?.User;

        public string? GetUserEmail()
        {
            return CurrentUser?.FindFirstValue(ClaimTypes.Email);
        }
        public string? GetUserId()
        {
            return CurrentUser?.FindFirstValue(ClaimTypes.NameIdentifier);
        }
        public async Task<TaskResponse> CreateTask(CreateTask create , TaskPolicy policy)
        {
            Guid UserId;
            var requestId = _httpContextAccessor.HttpContext?.TraceIdentifier;
            var userEmail = GetUserEmail();

            if (!Guid.TryParse(GetUserId(), out Guid userId))
                throw new UnauthorizedAccessException("User UserId not found in token");

            if (userEmail == null)
                throw new UnauthorizedAccessException("User email not found in token");

            var user = await _context.User.FindAsync(userId);
            if (user == null)
                throw new UnauthorizedAccessException("User does not exist in the database");

            _logger.LogInformation("Create new task called for {userEmail} ", userEmail);

            var entity = new MyTask
            {
                Title = create.Title,
                Description = create.Description,
                DueDate = create.DueDate,
                DueTime = create.Duetime,
                CreatedAt = DateTime.UtcNow,
                State = ProgressStatus.Pending,
                UserId = userId,
                Policy = policy
            };

            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();

            var ReminderEntity = new TaskReminders
            {
                TaskId = entity.MyTaskId,
                TaskName = entity.Title,
                DueDate = entity.DueDate,
                DueTime = entity.DueTime,
                Policy = policy
            };
            await _context.TaskReminders.AddAsync(ReminderEntity);
            await _context.SaveChangesAsync();
            await _reminderservice.ScheduleTaskReminder(entity.MyTaskId, userEmail, create.DueDate , create.Duetime , policy);
           
            _logger.LogInformation("Task {Title} created successfully for {userEmail}", create.Title, userEmail);

            return new TaskResponse
            {
                Title = create.Title,
                DueDate = create.DueDate
            };
        }
        public async Task<TaskResponse> GetTask(Guid MyTaskId)
        {
            var requestId = _httpContextAccessor.HttpContext?.TraceIdentifier;

            try
            {
                var entity = await _context.MyTask.FirstOrDefaultAsync(x => x.MyTaskId == MyTaskId);

                if (entity == null)
                    throw new NullReferenceException("The user from the db was null , or deleted");

                _logger.LogInformation("The Task was found {MyTaskId}", MyTaskId);

                return new TaskResponse
                {
                    Title = entity.Title,
                    DueDate = entity.DueDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in retrieving the task .");
                return null;
            }
        }
        public async Task<List<TaskResponse>> GetAllTasks()
        {
            var requestId = _httpContextAccessor.HttpContext?.TraceIdentifier;
            var count = await _context.MyTask.CountAsync();
            _logger.LogInformation("Total count of tasks in DB: {Count}, RequestId: {RequestId}", count, requestId);

            var tasks = await _context.MyTask
                .Select(u => new TaskResponse
                {
                    Title = u.Title,
                    DueDate = u.DueDate
                })
                .ToListAsync();

            if (tasks.Count == 0)
            {
                _logger.LogWarning("No tasks found in the database. RequestId: {RequestId}", requestId);
            }
            return tasks;
        }
        public async Task<TaskResponse> TotalUpdateTask(TotalUpdateTaskDTO newtask, Guid TaskId)
        {
            var requestId = _httpContextAccessor.HttpContext?.TraceIdentifier;
            _logger.LogInformation("TotalUpdaeTask called , requestId : {requestId} ", requestId);

            var newentity = await _context.MyTask.FirstOrDefaultAsync(x => x.MyTaskId == TaskId);
            if (newentity == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found. RequestId: {RequestId}", TaskId, requestId);
                throw new KeyNotFoundException($"Task with ID {TaskId} was not found , requestId : {requestId}.");
            }

            newentity.Title = newtask.NewTitle;
            newentity.Description = newtask.NewDescription;
            newentity.DueDate = newtask.NewDueDate;
            newentity.DueTime = newtask.NewDueTime;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully updated user , requestId : {requestId} ", requestId);

            return new TaskResponse
            {
                Title = newtask.NewTitle,
                DueDate = newtask.NewDueDate
            };
        }
        public async Task<bool> DeleteTask(Guid MyTaskId)
        {
            var requestId = _httpContextAccessor.HttpContext?.TraceIdentifier;
            _logger.LogInformation($"Delete user endpoint called , {requestId} ");

            var task = await _context.MyTask.FindAsync(MyTaskId);
            if (task == null)
            {
                _logger.LogError($"Failed to find task , either the taskid is wrong or the user has been deleted , {requestId}");
                throw new KeyNotFoundException("Unable to find user in db");
            }
            task.IsDeleted = DeletionStatus.True;
            await _context.SaveChangesAsync();
            _logger.LogInformation($"User Successfully deleted ,  {requestId}");

            return true;
        }
        public async Task<List<TaskResponse>> GetTasksByDueDate(DateOnly DueDate)
        {
            var requestId = _httpContextAccessor.HttpContext?.TraceIdentifier;
            _logger.LogInformation($"GetTaskByDueDate Method called , requestId : {requestId}");

            var tasks = await _context.MyTask
                 .Where(u => u.DueDate == DueDate)
                 .Select(u => new TaskResponse
                 {
                     Title = u.Title,
                     DueDate = u.DueDate
                 })
                .ToListAsync();

            if (!tasks.Any())
            {
                _logger.LogWarning($"No tasks were retrieved , requestId : {requestId}");
                return [];
            }
            return tasks;
        }
        public async Task<List<TaskResponse>> GetOverdueTasks()
        {
            var requestId = _httpContextAccessor.HttpContext?.TraceIdentifier;
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            _logger.LogInformation("GetOverdueTasks called. requestId: {RequestId}, today: {Today}", requestId, today);

            var tasks = await _context.MyTask
                .Where(t => t.DueDate < today && t.IsDeleted == DeletionStatus.False)
                .Select(t => new TaskResponse
                {
                    Title = t.Title,
                    DueDate = t.DueDate
                })
                .ToListAsync();

            if (!tasks.Any())
            {
                _logger.LogWarning("No overdue tasks found. requestId: {RequestId}", requestId);
                return new List<TaskResponse>(); 
            }

            return tasks;
        }
        public async Task<List<TaskResponse>> GetTasksByStatus(ProgressStatus status)
        {
            var requestId = _httpContextAccessor.HttpContext?.TraceIdentifier;
            _logger.LogInformation(
                "GetTasksByStatus called. requestId: {RequestId}, status: {Status}",
                requestId, status);

            var tasks = await _context.MyTask
                .Where(t => t.State == status && t.IsDeleted == DeletionStatus.False)
                .Select(t => new TaskResponse
                {
                    Title = t.Title,
                    DueDate = t.DueDate
                })
                .ToListAsync();

            if (!tasks.Any())
            {
                _logger.LogWarning(
                    "No tasks found with status {Status}. requestId: {RequestId}",
                    status, requestId);
                return new List<TaskResponse>(); 
            }

            return tasks;
        }
    }
}

