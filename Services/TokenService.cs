using System;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TaskManagerAPI.Entities;

namespace TaskManagerAPI.Services
{
    public interface ItokenService 
    {
        string CreateAccessToken(User user);
        RefreshTokens CreateRefreshToken(string ipAddress);
    }

    public class TokenService : ItokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string CreateAccessToken(User user)
        {
            if (user == null)
                throw new ArgumentException("The User is empty from the database", nameof(user));

            if (user.UserId == default)
                throw new ArgumentException("User ID is invalid", nameof(user.UserId));

            if (string.IsNullOrWhiteSpace(user.Email))
                throw new ArgumentException("Email is required", nameof(user.Email));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration.GetValue<String>("JwtSettings:Key")!))
                              ?? throw new InvalidOperationException("JWT key is not configured.");

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration.GetValue<String>("JwtSettings:Issuer")
                             ?? throw new InvalidOperationException("JWT issuer is not configured."),
                audience: _configuration.GetValue<String>("JwtSettings:Audience")
                             ?? throw new InvalidOperationException("JWT audience is not configured."),
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
        public RefreshTokens CreateRefreshToken(string ipAddress)
        {
            return new RefreshTokens
            {
                RefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
        }
    }
}
