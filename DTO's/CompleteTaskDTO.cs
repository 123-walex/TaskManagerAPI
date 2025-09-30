namespace TaskManagerAPI.DTO_s
{
    public class CompleteTaskDTO
    {
        public Guid TaskId { get; set; }
        public required string TaskName { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}
