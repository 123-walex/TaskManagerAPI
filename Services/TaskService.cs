using ErrorOr;
using Microsoft.AspNetCore.Identity;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTO_s;
using TaskManagerAPI.Entities;

namespace TaskManagerAPI.Services
{
    public interface ITaskService
    {
        public Task<ErrorOr<TaskResponse>> CreateTask(CreateTask create);
        public Task<ErrorOr<TaskResponse>> GetTask(Guid TaskId);
        public Task<ErrorOr<List<TaskResponse>>> GetAllTasks();
        public Task<ErrorOr<TaskResponse>> UpdateTask(Guid TaskId);
        public Task<ErrorOr<bool>> DeleteTask(Guid TaskId);
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
        public async Task<ErrorOr<TaskResponse>> CreateTask(CreateTask create)
        {
            var requestId = _httpContextAccessor.HttpContext?.TraceIdentifier;

            if()
        }
    }
}
