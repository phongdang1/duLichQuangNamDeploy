namespace duLichQuangNam.Models
{
    public class Img
    {
        public int ImageId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }     
        public string ImgUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }

}
