
using System.Diagnostics.Eventing.Reader;
using Azure.Core;
using ErrorOr;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTO_s;
using TaskManagerAPI.Entities;
using TaskManagerAPI.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TaskManagerAPI.Services
{
    public interface IAuthService
    {
        Task<ErrorOr<AuthResponse>> LoginUser_Manual(LoginDTO_Manual_ manual);
        Task<ErrorOr<AuthResponse>> LoginUser_Google(LoginDTO_Google_ automatic);
        Task<ErrorOr<List<UserDTO>>> GetAllUsers();
        Task<ErrorOr<UserDTO>> GetUser(Guid UserId);
        Task<ErrorOr<string>> TotalUpdate(TotalUpdateDTO update);
        Task<ErrorOr<string>> PartialUpdate(TotalUpdateDTO update);
        
    }
    public class AuthService : IAuthService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthService> _logger;
        private readonly TaskManagerDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ItokenService _tokenService;
        private readonly IConfiguration _configuration;

        public AuthService
            (
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthService> logger,
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
        public async Task<ErrorOr<AuthResponse>> LoginUser_Manual(LoginDTO_Manual_ manual)
        {
            var requestId = _httpContextAccessor.HttpContext?.TraceIdentifier;
            _logger.LogInformation("Create new user called : {RequestId}", requestId);

            string accessToken = String.Empty;
            RefreshTokens refreshedToken = null;

            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

            if (!String.IsNullOrEmpty(manual.Email) && !String.IsNullOrEmpty(manual.Password))
            {
                try
                {
                    //Creating new user 
                    var NewUser = new User
                    {
                        Email = manual.Email,
                        Password = "",
                        GoogleID = "Not Needed",
                        PictureUrl = "Not Needed"
                    };
                    NewUser.Password = _passwordHasher.HashPassword(NewUser, manual.Password);
                    NewUser.AuthType = UserType.manual;
                    NewUser.Role = RBAC.User;

                    _context.User.Add(NewUser);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("User successfully added to the DB : {RequestId}", requestId);

                    //Login 
                    var Loginentity = await _context.User.FirstOrDefaultAsync(l => l.Email == manual.Email);

                    if (Loginentity == null || Loginentity.IsDeleted == UserStatus.True)
                    {
                        _logger.LogError("The user has been deleted or doesn't exist , Status Code {StatusCode} : {requestId}", StatusCodes.Status404NotFound, requestId);
                        return Error.NotFound(
                             code: "User.NotFound",
                             description: $"User with email {NewUser.Email} was not found."
                             );
                    }
                    else
                    {
                        _logger.LogInformation("The user {Email} has been found : {requestId}", NewUser.Email, requestId);
                    }
                    var result = _passwordHasher.VerifyHashedPassword(Loginentity, Loginentity.Password, NewUser.Password);
                    if (result == PasswordVerificationResult.Success)
                    {
                        _logger.LogInformation("Password for {Email} Successfully verified : {requestId} ", NewUser.Email, requestId);
                    }
                    else
                    {
                        _logger.LogError("Wrong Password !! , StatusCode : {StatusCode} : {requestId} ", StatusCodes.Status401Unauthorized, requestId);
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
                _logger.LogError("An empty DTO was supplied , StatusCode : {StatusCode} , requestId : {requestId} .", StatusCodes.Status404NotFound, requestId);
            }
            return new AuthResponse
            {
                Email = manual.Email,
                AccessToken = accessToken,
                RefreshToken = refreshedToken.RefreshToken,
            };
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
                    Payload = await GoogleJsonWebSignature.ValidateAsync(automatic.IDToken,
                      new GoogleJsonWebSignature.ValidationSettings
                      {
                          Audience = new[] { _configuration["Google:ClientId"] } // from Google Cloud console
                      });

                    if (Payload == null)
                    {
                        _logger.LogError("Unable to validate Idtoken !!! , StatusCode : {StatusCode} , requestId : {requestId} .", StatusCodes.Status404NotFound, requestId);
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

                    var loginentity = await _context.User
                         .Include(u => u.RefreshTokens)
                         .Include(u => u.UserSessions)
                         .FirstOrDefaultAsync(u => u.GoogleID == googleId);

                    if (loginentity.IsDeleted == UserStatus.True && loginentity != null)
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
                    if (loginentity == null)
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
                            Role = RBAC.User
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
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error Processing Login , StatusCode : {StatusCode} , requestId {requestId} .", StatusCodes.Status404NotFound, requestId);
                    return Error.Failure("GoogleAuth.Failed", "Something went wrong during Google authentication.");
                }
            }
            else
            {
                _logger.LogError("Error Processing Login , StatusCode : {StatusCode} , requestId {requestId} .", StatusCodes.Status404NotFound, requestId);
                return Error.Failure("GoogleAuth.Failed", "Something went wrong during Google authentication.");
            }
            return new AuthResponse
            {
                Email = Payload?.Email,
                AccessToken = accesstoken,
                RefreshToken = refreshtoken.RefreshToken,
            };
        }
        public async Task<ErrorOr<List<UserDTO>>> GetAllUsers()
        {
            try
            {
                var users = await _context.User
                    .Where(u => u.IsDeleted != UserStatus.True)
                    .Select(u => new UserDTO
                        {
                          UserId = u.UserId,
                          Email = u.Email,
                          CreatedAt = u.CreatedAt
                    })
                    .ToListAsync();

                if (!users.Any())
                {
                    _logger.LogWarning("No users found in the database.");
                    return Error.NotFound(
                        code: "User.NotFound",
                        description: "No users were found in the system.");
                }
               return users;
            }
            catch (Exception ex)
            {
               _logger.LogError(ex, "An error occurred while retrieving users.");
               return Error.Failure(
                  code: "User.Retrieve.Failed",
                  description: "An unexpected error occurred while retrieving users.");
            }
        }
        public async Task<ErrorOr<UserDTO>> GetUser(Guid UserId)
        {
            var requestId = _httpContextAccessor.HttpContext?.TraceIdentifier;

            try
            {
                var user = await _context.User.FindAsync(UserId);

                if(user == null || user.IsDeleted == UserStatus.True)
                {
                    _logger.LogError("User is either detleted or null , StatusCode : {StatusCode} , requestId : {requestId} .", StatusCodes.Status404NotFound, requestId);
                    return Error.NotFound(
                         code: "User.NotFound",
                         description: $"User with ID {UserId} was not found or has been deleted.");
                }
                return new UserDTO
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt
                };
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID {UserId}", UserId);
                return Error.Failure(
                    code: "User.Retrieve.Failed",
                    description: "An unexpected error occurred while retrieving the user.");
            }
        }
        //core logic to change the mail and password 
        public async Task<ErrorOr<string>> TotalUpdate(TotalUpdateDTO update)
        {
            var requestId = _httpContextAccessor.HttpContext?.TraceIdentifier;
            try
            {
                var user = await _context.User.FindAsync(update.UserId);

                if (user == null)
                {
                    _logger.LogError("User is either detleted or null , StatusCode : {StatusCode} , requestId : {requestId} .", StatusCodes.Status404NotFound, requestId);
                    return Error.NotFound(
                         code: "User.NotFound",
                         description: $"User with ID {update.UserId} was not found or has been deleted.");
                }
                user.Email = update.NewEmail;
                user.Password = update.NewPassword;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully Updated user , RequesrId : {requestId} .", requestId);

                return user.Email;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while updating the user , requestId : {requestId} .", requestId);
                return Error.Failure(
                     code: "User.UpdateFailed",
                     description: "An unexpected error occurred while updating the user. Please try again later."
                     );
            }
        }
        public async Task<ErrorOr<string>> PartialUpdate(TotalUpdateDTO update)
        {
            var requestId = _httpContextAccessor.HttpContext?.TraceIdentifier;
            try
            {
                var user = await _context.User.FindAsync(update.UserId);

                if (user == null || user.IsDeleted == UserStatus.True)
                {
                    _logger.LogError("User is either detleted or null , StatusCode : {StatusCode} , requestId : {requestId} .", StatusCodes.Status404NotFound, requestId);
                    return Error.NotFound(
                         code: "User.NotFound",
                         description: $"User with ID {update.UserId} was not found or has been deleted.");
                }
                //update either one 
                if (update.OldEmail != null && update.NewEmail != null)
                {
                    user.Email = update.NewEmail;
                    user.UpdatedAt = DateTime.UtcNow;
                }
                else if (update.OldPassword != null && update.NewPassword != null)
                {
                    var hashpassword = new PasswordHasher<object>();
                    string hashedpassword = hashpassword.HashPassword(new object(), update.NewPassword);
                    user.Password = hashedpassword;
                    user.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully Updated user , RequesrId : {requestId} .", requestId);

                return user.Email;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occured while updating the user , requestId : {requestId} .", requestId);
                return Error.Failure(
                     code: "User.UpdateFailed",
                     description: "An unexpected error occurred while updating the user. Please try again later."
                     );
            }
        }
    }
}