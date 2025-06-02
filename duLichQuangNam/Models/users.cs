namespace duLichQuangNam.Models
{
    public class Users
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Mail { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public bool Deleted { get; set; }
        public string Password { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

    }
}
