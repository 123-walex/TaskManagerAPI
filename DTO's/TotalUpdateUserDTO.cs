using TaskManagerAPI.Enums;

namespace TaskManagerAPI.DTO_s
{
    public class TotalUpdateUserDTO
    {
        public required string OldEmail { get; set; }
        public required string NewEmail { get; set; }
        public required string OldPassword { get; set; }
        public required string NewPassword { get; set; }
        public Guid? UserId { get; set; } 
    }
}
