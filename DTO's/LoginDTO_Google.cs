using TaskManagerAPI.Enums;

namespace TaskManagerAPI.DTO_s
{
    public class LoginDTO_Google_
    {
        public required string IDToken { get; set; }
        public RBAC Role { get; set; }
    }
}
