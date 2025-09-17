namespace TaskManagerAPI.DTO_s
{
    public class UpdateTask
    {
        public string Title{ get; set; }
        public string Description { get; set; }
        public DateOnly DueDate { get; set; }
        public TimeOnly DueTime { get; set; }
    }
}
