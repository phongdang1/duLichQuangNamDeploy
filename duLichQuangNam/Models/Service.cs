namespace duLichQuangNam.Models
{
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime OpenTime { get; set; }
        public DateTime CloseTime { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool Deleted { get; set; } 
        public string MainService { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        public List<Img> Images { get; set; } = new();
    }

}
