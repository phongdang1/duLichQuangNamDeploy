namespace duLichQuangNam.Models
{
    public class Rate
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string UserNameCmt { get; set; } = string.Empty;

        public int Star { get; set; }
        public bool Deleted { get; set; }
    }

}
