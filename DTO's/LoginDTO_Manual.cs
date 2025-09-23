using TaskManagerAPI.Enums;

namespace TaskManagerAPI.DTO_s
{
    public class LoginDTO_Manual_
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public RBAC Role { get; set; }
    }
}
