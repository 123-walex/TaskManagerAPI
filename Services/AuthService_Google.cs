using System.Net;
using Azure.Core;
using ErrorOr;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTO_s;
using TaskManagerAPI.Entities;
using TaskManagerAPI.Enums;

//validate id token
// check the claims 
//create new user
// store record of tokens and sessions
namespace TaskManagerAPI.Services
{
    public interface IGoogleService
    {
        Task<ErrorOr<AuthResponse>> LoginUser_Google(LoginDTO_Google_ automatic);
    }
    public class AuthService_Google : IGoogleService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthService_Google> _logger;
        private readonly TaskManagerDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ItokenService _tokenService;
        private readonly IConfiguration _configuration;

        public AuthService_Google(
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthService_Google> logger,
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
        public async Task<ErrorOr<AuthResponse>> LoginUser_Google(LoginDTO_Google_ automatic)
        {
            var requestId = _httpContextAccessor.HttpContext?.TraceIdentifier;
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

            string accesstoken = string.Empty;
            RefreshTokens refreshtoken = null;

            GoogleJsonWebSignature.Payload Payload = null;

            if (!string.IsNullOrEmpty(automatic.IDToken))
            { 
                try
                {
                       Payload = await GoogleJsonWebSignature.ValidateAsync(automatic.IDToken ,
                         new GoogleJsonWebSignature.ValidationSettings
                         {
                             Audience = new[] { _configuration["Google:ClientId"] } // from Google Cloud console
                         });

                    if (Payload == null)
                    {
                        _logger.LogError("Unable to validate Idtoken !!! , StatusCode : {StatusCode} , requestId : {requestId} ." , StatusCodes.Status404NotFound , requestId);
                        return Error.NotFound(
                            code: "User.NotFound",
                            description: "Unable to Validate Id token "
                            );
                    }
                    // Since id token has now been decrypted we have differnt fields
                    var email = Payload.Email;
                    var googleId = Payload.Subject;
                    var pictureurl = Payload.Picture;
                    var username = Payload.Name;

                    var loginentity =  await _context.User
                         .Include(u => u.RefreshTokens)
                         .Include(u => u.UserSessions)
                         .FirstOrDefaultAsync(u => u.GoogleID  == googleId);

                    if(loginentity.IsDeleted == UserStatus.True && loginentity != null)
                    {
                        _logger.LogError("The user has been deleted or doesn't exist , Status Code {StatusCode} : {requestId}", StatusCodes.Status404NotFound, requestId);
                        return Error.NotFound(
                             code: "User.NotFound",
                             description: $"User with email {email} was not found on the db."
                             );
                    }
                    else
                    {
                        _logger.LogInformation("The user {email} has been found : {requestId}", email, requestId);
                    }
                    if(loginentity == null)
                    {
                        var user = new User
                        {
                            Email = email,
                            PictureUrl = pictureurl,
                            GoogleID = googleId,
                            Password = "Not required",
                            CreatedAt = DateTime.UtcNow,
                            AuthType = UserType.automatic,
                            IsDeleted = UserStatus.False,
                        };
                        await _context.User.AddAsync(user);
                        await _context.SaveChangesAsync();
                        loginentity = user;

                        _logger.LogInformation("New Google user {Email} created. RequestId: {RequestId}", email, requestId);
                }
                    //create new tokens 
                    accesstoken = _tokenService.CreateAccessToken(loginentity);
                    refreshtoken = _tokenService.CreateRefreshToken(ipAddress);

                    var session = new UserSessions
                    {
                        UserId = loginentity.UserId,
                        LoggedInAt = DateTime.UtcNow,
                        AccessSessionToken = accesstoken,
                        user = loginentity
                    };
                    loginentity.RefreshTokens.Add(refreshtoken);
                    loginentity.UserSessions.Add(session);
                    await _context.SaveChangesAsync();
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Error Processing Login , StatusCode : {StatusCode} , requestId {requestId} .", StatusCodes.Status404NotFound, requestId);
                    return Error.Failure("GoogleAuth.Failed", "Something went wrong during Google authentication.");
            }
            }
            else
            {
                _logger.LogError( "Error Processing Login , StatusCode : {StatusCode} , requestId {requestId} .", StatusCodes.Status404NotFound, requestId);
                return Error.Failure("GoogleAuth.Failed", "Something went wrong during Google authentication.");
            }
                return new AuthResponse
                {
                    Email = Payload?.Email,
                    AccessToken = accesstoken,
                    RefreshToken = refreshtoken.RefreshToken,
                };
        }
    }
}
