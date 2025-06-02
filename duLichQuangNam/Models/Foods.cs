namespace duLichQuangNam.Models
{
    public class Foods
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool Deleted { get; set; }
        public List<Img> Images { get; set; } = new();
    }
}
