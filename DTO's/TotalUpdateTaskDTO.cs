namespace TaskManagerAPI.DTO_s
{
    public class TotalUpdateTaskDTO
    {
        public string NewTitle { get; set; }
        public string NewDescription { get; set; }
        public DateOnly NewDueDate { get; set; }
        public TimeOnly NewDueTime { get; set; }
        public string OldTitle { get; set; }
        public string OldDescription { get; set; }
        public string OldDueDate { get; set; }
        public string OldDueTime{ get; set; }
    }
}
