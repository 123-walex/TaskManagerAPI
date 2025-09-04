using AutoMapper;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTO_s;
using TaskManagerAPI.Services;

namespace TaskManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly TaskManagerDbContext _context;
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;

        public AuthController(
                         IConfiguration configuration,
                         TaskManagerDbContext context,
                         ILogger<AuthController> logger,
                         IAuthService authService
                         )
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
            _authService = authService;
        }
        [AllowAnonymous]
        [HttpPost("SIgnUp_Manual")]
        public async Task<IActionResult> ManualSignUp([FromBody] LoginDTO_Manual_ manual)
        {
            var result = await _authService.LoginUser_Manual(manual);
            return Ok(result);
        }
        [AllowAnonymous]
        [HttpPost("Login_Google")]
        public async Task<IActionResult> GoogleLogin([FromBody] LoginDTO_Google_ google)
        {
            var result = await _authService.LoginUser_Google(google);
            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("GetAllUsers")]
        public async Task<ErrorOr<List<UserDTO>>> GetAllUsers()
        {
            var result = await _authService.GetAllUsers();
            return Ok(result);
        }
    }
}
