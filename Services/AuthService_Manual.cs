
using System.Diagnostics.Eventing.Reader;
using Azure.Core;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTO_s;
using TaskManagerAPI.Entities;
using TaskManagerAPI.Enums;

namespace TaskManagerAPI.Services
{
    public interface IAuthService
    {
        Task<LoginDTO_Manual_> SignUpManual(LoginDTO_Manual_ manual);
        Task<ErrorOr<AuthResponse>> LoginUser_Manual(LoginDTO_Manual_ man);
    }
    public class AuthService_Manual : IAuthService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthService_Manual> _logger;
        private readonly TaskManagerDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ItokenService _tokenService;

        public AuthService_Manual
            (
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthService_Manual> logger,
            TaskManagerDbContext context,
            PasswordHasher<User> passwordHasher,
            ItokenService tokenService
            )

        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }
        public async Task<LoginDTO_Manual_> SignUpManual(LoginDTO_Manual_ manual)
        {
             var requestid = _httpContextAccessor.HttpContext?.TraceIdentifier;
             _logger.LogInformation("Create new user called : {RequestId}", requestid);

            if (!String.IsNullOrEmpty(manual.Email) && !String.IsNullOrEmpty(manual.Password))
            {
                try
                {
                    var NewUser = new User
                    {
                        Email = manual.Email,
                        Password = "", 
                        GoogleID = "Not Needed",
                        PictureUrl = "Not Needed"
                    };
                    NewUser.Password = _passwordHasher.HashPassword(NewUser, manual.Password);
                    NewUser.AuthType = UserType.manual;

                    _context.User.Add(NewUser);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("User successfully added to the DB : {RequestId}", requestid);

                    var type = new LoginDTO_Manual_
                    {
                        Email = manual.Email,
                        Password = string.Empty
                    };

                   return type;
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex , "Error creating user : {RequestId}" , requestid);
                }
            }
            else
            {
                _logger.LogError("Empty DTO provided : {RequestId}", requestid);
            }
            var type2 = new LoginDTO_Manual_
            {
                Email = manual.Email,
                Password = string.Empty
            };
            return type2;
        }
        public async Task<ErrorOr<AuthResponse>> LoginUser_Manual(LoginDTO_Manual_ man)
        {
            var requestId = _httpContextAccessor.HttpContext?.TraceIdentifier;
            _logger.LogInformation("Login Method Called : {RequestId}", requestId);

            string accessToken = String.Empty;
            RefreshTokens refreshedToken = null;

            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

            if (!string.IsNullOrEmpty(man.Email) && !string.IsNullOrEmpty(man.Password))
            {
                try
                {
                    var Loginentity = await _context.User.FirstOrDefaultAsync(l => l.Email == man.Email);

                    if (Loginentity == null || Loginentity.IsDeleted == UserStatus.True)
                    {
                        _logger.LogError("The user has been deleted or doesn't exist , Status Code {StatusCode} : {requestId}", StatusCodes.Status404NotFound , requestId);
                        return Error.NotFound(
                             code: "User.NotFound",
                             description: $"User with email {man.Email} was not found."
                             );
                    }
                    else 
                    {
                        _logger.LogInformation("The user {Email} has been found : {requestId}", man.Email, requestId);
                    }

                    var result = _passwordHasher.VerifyHashedPassword(Loginentity, Loginentity.Password, man.Password);
                    if(result == PasswordVerificationResult.Success)
                    {
                        _logger.LogInformation("Password for {Email} Successfully verified : {requestId} ", man.Email, requestId);
                    }
                    else
                    {
                        _logger.LogError("Wrong Password !! , StatusCode : {StatusCode} : {requestId} ", StatusCodes.Status401Unauthorized , requestId);
                        return Error.Unauthorized(
                            code: "User.InvalidPassword",
                            description: "Invalid password supplied.");
                    }

                    accessToken = _tokenService.CreateAccessToken(Loginentity);
                    refreshedToken = _tokenService.CreateRefreshToken(ipAddress);

                    var session = new UserSessions
                    {
                        UserId = Loginentity.UserId,
                        LoggedInAt = DateTime.UtcNow,
                        AccessSessionToken = accessToken,
                        user = Loginentity 
                    };
                  
                    Loginentity.RefreshTokens.Add(refreshedToken);
                    Loginentity.UserSessions.Add(session);

                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error Processing Login , StatusCode : {StatusCode} , requestId {requestId} .", StatusCodes.Status404NotFound, requestId);
                }
            }
            else
            {
                _logger.LogError("An empty DTO was supplied , StatusCode : {StatusCode} , requestId : {requestId} .", StatusCodes.Status404NotFound , requestId);
            }
            return new AuthResponse
            {
                Email = man.Email,
                AccessToken = accessToken,
                RefreshToken = refreshedToken.RefreshToken, 
            };

        }
    }
}
