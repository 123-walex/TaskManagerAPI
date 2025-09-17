using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTO_s;
using TaskManagerAPI.Entities;
using TaskManagerAPI.Enums;


namespace TaskManagerAPI.Services
{
    public interface ITaskService
    {
        public Task<TaskResponse> CreateTask(CreateTask create);
        public Task<TaskResponse> GetTask(Guid MyTaskId);
        public Task<List<TaskResponse>> GetAllTasks();
        public Task<TaskResponse> UpdateTask(Guid MyTaskId);
        public Task<ErrorOr<bool>> DeleteTask(Guid MyTaskId);
        public Task<ErrorOr<List<TaskResponse>>> GetTasksByDueDate(DateOnly dueDate);
        public Task<ErrorOr<List<TaskResponse>>> GetOverdueTasks();
        public Task<ErrorOr<List<TaskResponse>>> GetTasksByStatus(TaskStatus status);
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

        public TaskService
            (
            IHttpContextAccessor httpContextAccessor,
            ILogger<TaskService> logger,
            TaskManagerDbContext context,
            PasswordHasher<User> passwordHasher,
            ItokenService tokenService,
            IConfiguration configuration
            )

        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _configuration = configuration;
        }
        private ClaimsPrincipal? CurrentUser => _httpContextAccessor.HttpContext?.User;

        public string? GetUserEmail()
        {
            return CurrentUser?.FindFirstValue(ClaimTypes.Email);
        }
        public string? GetUserId()
        {
            return CurrentUser?.FindFirstValue("UserId");
        }
        public async Task<TaskResponse> CreateTask(CreateTask create)
        {
            Guid UserId;
            var requestId = _httpContextAccessor.HttpContext?.TraceIdentifier;
            var userEmail = GetUserEmail();
            var userId = Guid.TryParse(GetUserId(), out UserId);

            if (userEmail == null)
                throw new UnauthorizedAccessException("User email not found in token");

            _logger.LogInformation("Create new task called for {userEmail} ", userEmail);

            var entity = new MyTask
            {
                Title = create.Title,
                Description = create.Description,
                DueDate = create.DueDate,
                DueTime = create.Duetime,
                CreatedAt = DateTime.UtcNow,
                State = ProgressStatus.Pending
            };

            await _context.AddAsync(entity);
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
                _logger.LogInformation(ex, "Erroe occurred in retriebeing the task .");
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
        }
    }
}
