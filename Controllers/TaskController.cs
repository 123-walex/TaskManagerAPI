using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTO_s;
using TaskManagerAPI.Enums;
using TaskManagerAPI.Services;

namespace TaskManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly TaskManagerDbContext _context;
        private readonly ILogger<TaskController> _logger;
        private readonly ITaskService _taskservice;

        public TaskController(
                         IConfiguration configuration,
                         TaskManagerDbContext context,
                         ILogger<TaskController> logger,
                         ITaskService taskservice
                         )
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
            _taskservice = taskservice;
        }

        [Authorize( Roles = "Admin , User")]
        [HttpPost("CreateTask")]
        public async Task<IActionResult> CreateTask(CreateTask create , TaskPolicy policy)
        {
            var result = await _taskservice.CreateTask(create , policy);

           return Ok(result);
        }
        [Authorize(Roles = "Admin , User")]
        [HttpPost("Complete-Task/{TaskId}")]
        public async Task<IActionResult> CompleteTask(Guid TaskId)
        {
           var result = await _taskservice.CompleteTask(TaskId);
           return Ok(new
            {
                message = "Task completed successfully.",
                task = result
            });
        }
        [Authorize(Roles = "Admin , User")]
        [HttpGet("GetTaskById/{TaskId}")]
        public async Task<IActionResult> GetTaskById(Guid TaskId)
        {
            var result = await _taskservice.GetTask(TaskId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }
        [Authorize(Roles = "Admin , User")]
        [HttpGet("GetAllTasks")]
        public async Task<IActionResult> GetAllTasks()
        {
            var result = await _taskservice.GetAllTasks();
            
            if (result == null)
                return NotFound();

            return Ok(result);
        }
        [Authorize(Roles = "Admin , User")]
        [HttpPut("TotalUpdateTaskDTO")]
        public async Task<IActionResult> TotalUpdateTask(TotalUpdateTaskDTO newtask, Guid MyTaskId)
        {
            var result = await _taskservice.TotalUpdateTask(newtask, MyTaskId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }
        [Authorize(Roles = "Admin , User")]
        [HttpDelete("DeleteTask")]
        public async Task<IActionResult> DeleteTask(Guid MyTaskId)
        {
            var result = await _taskservice.DeleteTask(MyTaskId);
            return NoContent();
        }
        [Authorize(Roles = "Admin , User")]
        [HttpGet("GetTaskByDueDate{dueDate}")]
        public async Task<IActionResult> GetTasksByDueDate(DateOnly dueDate)
        {
            var result = await _taskservice.GetTasksByDueDate(dueDate);

            if (result == null)
                return NotFound();

            return Ok(result);
        }
        [Authorize(Roles = "Admin , User")]
        [HttpGet("GetOverdueTasks")]
        public async Task<IActionResult> GetOverdueTasks()
        {
            var result = await _taskservice.GetOverdueTasks();

            if (result == null)
                return NotFound();

            return Ok(result);
        }
        [Authorize(Roles = "Admin , User")]
        [HttpGet("GetTaskByStatus")]
        public async Task<IActionResult> GetTasksByStatus(ProgressStatus status)
        {
            var result = await _taskservice.GetTasksByStatus(status);

            if (result == null || !result.Any())
                return NotFound(new { message = $"No tasks found with status {status}" });

            var count = result.Count;

            return Ok( new {
                  count,
                  tasks = result
                });
        }
    }
}

