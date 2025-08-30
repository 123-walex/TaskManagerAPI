using AutoMapper;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTO_s;
using TaskManagerAPI.Services;

namespace TaskManagerAPI.Controllers
{
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly TaskManagerDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthController> _logger;
        private readonly IGoogleService _googleService;
        private readonly IAuthService _authService;

        public AuthController(
                         IConfiguration configuration,
                         TaskManagerDbContext context,
                         IMapper mapper,
                         ILogger<AuthController> logger,
                         IGoogleService googleService,
                         IAuthService authService
                         )
        {
            _configuration = configuration;
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _authService = authService;
            _googleService = googleService;
            _authService = authService;
        }

        [HttpPost("Login_Manual")]
        public async Task<IActionResult> GoogleLogin([FromBody] LoginDTO_Google_ automatic)
        {
            ErrorOr<AuthResponse> result = await _googleService.LoginUser_Google(automatic);

            return result.Match(
                onValue: authResponse => Ok(authResponse),
                onError: errors => BadRequest(errors)
            );
        }
    }
}
