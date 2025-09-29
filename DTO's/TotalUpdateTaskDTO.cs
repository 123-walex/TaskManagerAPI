namespace TaskManagerAPI.DTO_s
{
    public class TotalUpdateTaskDTO
    {
        public required string NewTitle { get; set; }
        public required string NewDescription { get; set; }
        public DateOnly NewDueDate { get; set; }
        public TimeOnly NewDueTime { get; set; }
    }
}
