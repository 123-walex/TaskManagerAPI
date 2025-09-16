using System;

namespace TaskManagerAPI.Entities
{
    public class UserSessions
    {
        public int Id { get; set; } 
        public Guid UserId { get; set; }
        public User user { get; set; } = null!;
        public DateTime? LoggedInAt { get; set; }
        public DateTime? LoggedOutAt { get; set; }
        public string? AccessSessionToken { get; set; }
        public int NoOfSessions { get; set; }
    }
}
