namespace duLichQuangNam.Models
{
    public class Stay
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Price { get; set; }
        public string Type { get; set; } = string.Empty;
        public string ServiceStay { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Mail { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool Deleted { get; set; }
        public List<Img> Images { get; set; } = new();
    }

}
