using AutoMapper;
using ErrorOr;
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
        private readonly IGoogleService _googleService;
        private readonly IAuthService _authService;

        public AuthController(
                         IConfiguration configuration,
                         TaskManagerDbContext context,
                         ILogger<AuthController> logger,
                         IGoogleService googleService,
                         IAuthService authService
                         )
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
            _authService = authService;
            _googleService = googleService;
        }

        [HttpPost("SIgnUp_Manual")]
        public async Task<IActionResult> ManualSignUp([FromBody] LoginDTO_Manual_ manual)
        {
            var result = await _authService.SignUpManual(manual);
            return Ok(result);
        }
        [HttpPost("Login_Manual")]
        public async Task<IActionResult> ManualLogin([FromBody] LoginDTO_Manual_ login)
        {
            var result = await _authService.LoginUser_Manual(login);

            return Ok(result);
        }
        [HttpPost("Login_Google")]
        public async Task<IActionResult> GoogleLogin([FromBody] LoginDTO_Google_ google)
        {
            var result = await _googleService.LoginUser_Google(google);

            return Ok(result);
        }
    }
}
