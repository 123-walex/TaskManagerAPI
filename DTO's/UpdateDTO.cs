using TaskManagerAPI.Enums;

namespace TaskManagerAPI.DTO_s
{
    public class UpdateDTO
    {
       public string? Email { get; set; }
       public string? Password { get; set; }
       public string? AccessToken{ get; set; }
       public string? RefreshToken { get; set; }
       public RBAC Role { get; set; }
    }
}
