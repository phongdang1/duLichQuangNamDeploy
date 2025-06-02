namespace duLichQuangNam.Models
{
    public class UsersRegistrationViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Mail { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public int Age { get; set; }

        public string Role { get; set; } = "user";
        public string Address { get; set; } = string.Empty;
    }
}
