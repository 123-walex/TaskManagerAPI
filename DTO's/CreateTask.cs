namespace TaskManagerAPI.DTO_s
{
    public class CreateTask
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public TimeOnly Duetime { get; set; }
        public DateOnly DueDate { get; set; }
    }
}
