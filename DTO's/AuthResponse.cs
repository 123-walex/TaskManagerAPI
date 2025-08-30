namespace TaskManagerAPI.DTO_s
{
    public class AuthResponse
    {
        public required string Email { get; set; }
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
    }
}
