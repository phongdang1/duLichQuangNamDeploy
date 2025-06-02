namespace duLichQuangNam.Models
{
    public class Tour
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public bool Deleted { get; set; }
        public List<Img> Images { get; set; } = new();
    }

}
