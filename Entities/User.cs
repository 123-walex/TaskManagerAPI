using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using TaskManagerAPI.Enums;

namespace TaskManagerAPI.Entities
{
    public class User
    {
        public Guid UserId { get; set; } = Guid.NewGuid();

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public required string Email{ get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public UserType AuthType { get; set; }
        public UserStatus IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? RestoredAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public RBAC Role { get; set; }

        [Required]
        [MaxLength(256)]
        public required string Password { get; set; }
        public required string PictureUrl { get; set; }
        public required string GoogleID { get; set; }
        public ICollection<UserSessions> UserSessions { get; set; } = new List<UserSessions>(); 
        public ICollection<RefreshTokens> RefreshTokens { get; set; } = new List<RefreshTokens>();
        public ICollection<MyTask> Tasks { get; set; } = new List<MyTask>();
        public ICollection<TaskReminders> Reminders { get; set; } = new List<TaskReminders>();
    }
}
