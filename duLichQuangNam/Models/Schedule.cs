namespace duLichQuangNam.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<ScheduleItem> ScheduleItems { get; set; } = new();
    }
}
