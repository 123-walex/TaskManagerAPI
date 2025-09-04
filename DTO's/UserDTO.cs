namespace TaskManagerAPI.DTO_s
{
    public class UserDTO
    {
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UserId { get; set; }
    }
}
