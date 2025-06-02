namespace duLichQuangNam.Models
{
    public class ScheduleItem
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public int DayOrder { get; set; }

        public Destination? Destination { get; set; }
        public Service? Service { get; set; }
    }
}
