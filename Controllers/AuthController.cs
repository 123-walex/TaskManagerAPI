using AutoMapper;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTO_s;
using TaskManagerAPI.Services;
// remember to use the py visualizaion library for your tasks
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
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _authService.GetAllUsers();
            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("GetUserBy/{Id}")]
        public async Task<IActionResult> GetSingleUser(Guid Id)
        {
            var user = await _authService.GetUser(Id);
            return Ok(user);
        } 
        // use email to get user id 
        [Authorize(Roles = "Admin , User")]
        [HttpPut("TotalUpdate")]
        public async Task<IActionResult> TotalUpdate(TotalUpdateDTO update)
        {
            var requestId = HttpContext.TraceIdentifier;
           
            if(update == null)
            {
                _logger.LogError("An empty DTO was recieved , RequestId : {requestId} ", requestId);
                return BadRequest();
            }
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == update.OldEmail);
            if(user == null)
            {
                _logger.LogError("The User in the db is either null or deleted , RequestId : {requestId} ", requestId);
                return BadRequest();
            }
            update.UserId = user.UserId;

            var result = await _authService.TotalUpdate(update);
            return Ok(new
            {
                Message = "User successfully updated.",
                Email = result
            });
        }
        [Authorize(Roles = "User , Admin")]
        [HttpPatch("PartialUpdate")]
        public async Task<IActionResult> PartialUpdate(PartialUpdateDTO update)
        {
            var requestId = HttpContext.TraceIdentifier;
            if (update == null)
            {
                _logger.LogError("An empty DTO was recieved , RequestId : {requestId} ", requestId);
                return BadRequest();
            }
            var hashpassword = new PasswordHasher<object>();
            string hashedpassword = hashpassword.HashPassword(new object(), update.OldPassword);

            // either use email or hashedpassword and the key for querying ,
            // using password 
            if (hashedpassword != null)
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.Password == hashedpassword);
            }
            //using email
            else if(update.OldEmail != null)
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.Email == update.OldEmail);
            }
            // continut with the partial update logic and the service layer method 
        }
    }
}
