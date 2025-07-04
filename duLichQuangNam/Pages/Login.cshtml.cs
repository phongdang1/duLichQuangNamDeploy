//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using duLichQuangNam.Models;
//using System.Net.Http;
//using System.Net.Http.Json;
//using System.Threading.Tasks;
//using System.Text.Json;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using System.Security.Claims;

//namespace duLichQuangNam.Pages
//{

//    public class LoginModel : PageModel
//    {
//        private readonly IHttpClientFactory _httpClientFactory;

//        public LoginModel(IHttpClientFactory httpClientFactory)
//        {
//            _httpClientFactory = httpClientFactory;
//        }

//        [BindProperty]
//        public UsersLoginViewModel LoginData { get; set; } = new UsersLoginViewModel();

//        public void OnGet()
//        {
//        }

//        public async Task<IActionResult> OnPostAsync()
//        {
//            if (!ModelState.IsValid)
//                return Page();

//            try
//            {
//                var client = _httpClientFactory.CreateClient();
//                var response = await client.PostAsJsonAsync("https://dulichquangnamdeploy.onrender.com/api/Users/login", LoginData);

//                if (response.IsSuccessStatusCode)
//                {
//                    var responseBody = await response.Content.ReadAsStringAsync();
//                    var jsonDoc = JsonDocument.Parse(responseBody);

//                    var token = jsonDoc.RootElement.GetProperty("token").GetString();
//                    var id = jsonDoc.RootElement.GetProperty("id").GetInt32();
//                    var role = jsonDoc.RootElement.GetProperty("role").GetString();


//                    var claims = new List<Claim>
//                    {
//                        new Claim(ClaimTypes.NameIdentifier, id.ToString()),
//                        new Claim(ClaimTypes.Role, role),
//                        new Claim("token", token ?? "")
//                    };

//                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
//                    var authProperties = new AuthenticationProperties
//                    {
//                        IsPersistent = true,
//                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
//                    };


//                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
//                        new ClaimsPrincipal(claimsIdentity),
//                        authProperties);


//                    return RedirectToPage("/Index");
//                }
//                else
//                {
//                    ModelState.AddModelError(string.Empty, "Error Username or Password");
//                    return Page();
//                }
//            }
//            catch
//            {
//                ModelState.AddModelError(string.Empty, "Error system");
//                return Page();
//            }
//        }
//    }
//}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using duLichQuangNam.Models; // Đảm bảo namespace này chứa UsersLoginViewModel
using System.Net.Http;
using System.Net.Http.Json; // Cần thiết cho PostAsJsonAsync
using System.Threading.Tasks;
using System.Text.Json; // Cần thiết cho JsonDocument
using Microsoft.AspNetCore.Authentication; // Cần thiết cho HttpContext.SignInAsync, AuthenticationProperties, AuthenticationToken
using Microsoft.AspNetCore.Authentication.Cookies; // Cần thiết cho CookieAuthenticationDefaults.AuthenticationScheme
using System.Security.Claims; // Cần thiết cho Claims, ClaimsIdentity, ClaimsPrincipal

namespace duLichQuangNam.Pages
{
    // LoginModel là một Razor Page để xử lý việc đăng nhập của người dùng.
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        // Constructor để inject IHttpClientFactory.
        // IHttpClientFactory được sử dụng để tạo HttpClient, giúp gửi các yêu cầu HTTP đến API.
        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // BindProperty để liên kết dữ liệu từ form HTML với model này.
        // Đây là dữ liệu đăng nhập (username và password) mà người dùng nhập vào.
        [BindProperty]
        public UsersLoginViewModel LoginData { get; set; } = new UsersLoginViewModel();

        // Phương thức OnGet được gọi khi trang Login được yêu cầu lần đầu (HTTP GET).
        public void OnGet()
        {
            // Không có logic đặc biệt nào cần thực hiện khi tải trang.
        }

        // Phương thức OnPostAsync được gọi khi form đăng nhập được gửi đi (HTTP POST).
        public async Task<IActionResult> OnPostAsync()
        {
            // Kiểm tra tính hợp lệ của dữ liệu model (dựa trên Data Annotations trong UsersLoginViewModel).
            if (!ModelState.IsValid)
            {
                // Nếu dữ liệu không hợp lệ, hiển thị lại trang với các lỗi validation.
                return Page();
            }

            try
            {
                // Tạo một HttpClient để gọi API đăng nhập.
                var client = _httpClientFactory.CreateClient();
                // Gửi yêu cầu POST chứa dữ liệu đăng nhập đến API Users/login.
                // URL API được hardcode ở đây, bạn có thể cân nhắc đưa vào cấu hình (appsettings.json)
                // để dễ dàng thay đổi giữa các môi trường (development, production).
                var response = await client.PostAsJsonAsync("https://dulichquangnamdeploy.onrender.com/api/Users/login", LoginData);

                // Kiểm tra xem yêu cầu API có thành công không (mã trạng thái 2xx).
                if (response.IsSuccessStatusCode)
                {
                    // Đọc nội dung phản hồi từ API dưới dạng chuỗi.
                    var responseBody = await response.Content.ReadAsStringAsync();
                    // Phân tích chuỗi JSON thành một JsonDocument để dễ dàng truy cập các thuộc tính.
                    var jsonDoc = JsonDocument.Parse(responseBody);

                    // Trích xuất token, id và role từ phản hồi JSON của API.
                    var token = jsonDoc.RootElement.GetProperty("token").GetString();
                    var id = jsonDoc.RootElement.GetProperty("id").GetInt32();
                    var role = jsonDoc.RootElement.GetProperty("role").GetString();

                    // Tạo danh sách Claims (thông tin về người dùng) sẽ được lưu trong cookie xác thực.
                    var claims = new List<Claim>
                    {
                        // Claim cho định danh người dùng (ID).
                        new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                        // Claim cho vai trò của người dùng.
                        new Claim(ClaimTypes.Role, role)
                        // Không thêm JWT token ở đây như một Claim thông thường nữa
                    };

                    // Tạo một ClaimsIdentity từ các claims và scheme xác thực Cookie.
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    // Cấu hình các thuộc tính của authentication cookie.
                    var authProperties = new AuthenticationProperties
                    {
                        // IsPersistent = true để cookie tồn tại sau khi đóng trình duyệt (ghi nhớ đăng nhập).
                        IsPersistent = true,
                        // Thời gian hết hạn của cookie (2 giờ kể từ thời điểm hiện tại).
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
                    };

                    // --- THAY ĐỔI QUAN TRỌNG: LƯU TRỮ JWT TOKEN DƯỚI DẠNG AUTHENTICATION TOKEN ---
                    // Lưu trữ JWT token để có thể truy xuất sau này bằng HttpContext.GetTokenAsync("access_token").
                    // Tên "access_token" là một quy ước phổ biến, bạn có thể dùng tên khác nhưng phải nhất quán.
                    authProperties.StoreTokens(new[]
                    {
                        new AuthenticationToken { Name = "access_token", Value = token }
                    });
                    // --- KẾT THÚC THAY ĐỔI QUAN TRỌNG ---


                    // Thực hiện đăng nhập người dùng vào hệ thống bằng Cookie Authentication.
                    // Điều này sẽ tạo một authentication cookie và gửi nó về trình duyệt.
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // Chuyển hướng người dùng đến trang Index sau khi đăng nhập thành công.
                    return RedirectToPage("/Index");
                }
                else
                {
                    // Nếu API trả về lỗi (ví dụ: 401 Unauthorized), thêm lỗi vào ModelState
                    // để hiển thị thông báo lỗi trên giao diện người dùng.
                    ModelState.AddModelError(string.Empty, "Lỗi tên đăng nhập hoặc mật khẩu.");
                    return Page();
                }
            }
            catch (Exception ex) // Bắt các ngoại lệ chung (ví dụ: lỗi mạng, lỗi phân tích JSON).
            {
                // Thêm lỗi hệ thống vào ModelState để hiển thị trên giao diện người dùng.
                ModelState.AddModelError(string.Empty, $"Lỗi hệ thống: {ex.Message}");
                return Page();
            }
        }
    }
}