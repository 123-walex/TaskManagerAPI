namespace TaskManagerAPI.DTO_s
{
    public class TaskResponse
    {
        public required string Title { get; set; }
        public DateOnly DueDate { get; set; }
        public Guid MyTaskId{ get; set; }
    }
}
