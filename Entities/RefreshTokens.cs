using System;

namespace TaskManagerAPI.Entities
{
    public class RefreshTokens
    {
        public int Id { get; set; }
        public string RefreshToken { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public DateTime CreatedAt { get; set; }
        public string CreatedByIp { get; set; } = null!;
        public DateTime? RevokedAt { get; set; }
        public string? RevokedByIp { get; set; }
        public string? ReplacedByToken { get; set; }
        public bool IsActive => RevokedAt == null && !IsExpired;

        // Foreign key reference to user
        public Guid UserId { get; set; }
        public User user { get; set; } = null!;
    }
}
