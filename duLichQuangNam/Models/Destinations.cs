namespace duLichQuangNam.Models
{
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime OpenTime { get; set; }
        public DateTime CloseTime { get; set; }
        public decimal Price { get; set; }
        public string Mail { get; set; } = string.Empty;
        public bool Deleted { get; set; }
        public List<Img> Images { get; set; } = new();

    }

}
