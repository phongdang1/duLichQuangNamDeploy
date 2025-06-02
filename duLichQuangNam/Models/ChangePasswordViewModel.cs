using System.ComponentModel.DataAnnotations;

public class ChangePasswordViewModel
{
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu cũ không được để trống")]
    [DataType(DataType.Password)]
    public string OldPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Compare("NewPassword", ErrorMessage = "Xác nhận mật khẩu mới không khớp")]
    [DataType(DataType.Password)]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
