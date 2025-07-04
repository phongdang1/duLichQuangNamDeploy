using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using duLichQuangNam.Models;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.Text.Json;

namespace duLichQuangNam.Pages
{
    [Authorize(Roles = "adminFood,admin")]
    public class FoodsManagerModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public FoodsManagerModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<Foods> Foods { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? ErrorMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SuccessMessage { get; set; }

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient();

            try
            {
                var response = await client.GetAsync("https://dulichquangnamdeploy.onrender.com/api/foods");
                if (response.IsSuccessStatusCode)
                {
                    var list = await response.Content.ReadFromJsonAsync<List<Foods>>();
                    if (list != null)
                    {
                        Foods = list;
                    }
                }
                else
                {
                    ErrorMessage = $"Error take data: {response.ReasonPhrase}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error connect API: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var client = _httpClientFactory.CreateClient();

            try
            {
                // Lấy JWT token từ Claims của người dùng đã đăng nhập
                // LoginModel của bạn đã lưu JWT vào một claim tên là "token"
                var jwtToken = await HttpContext.GetTokenAsync("token");

                if (string.IsNullOrEmpty(jwtToken))
                {
                    // Nếu không tìm thấy JWT trong claims, có thể người dùng chưa đăng nhập đúng cách
                    // hoặc phiên cookie không chứa JWT.
                    ErrorMessage = "Không tìm thấy token xác thực. Vui lòng đăng nhập lại.";
                    return RedirectToPage(new { ErrorMessage, SuccessMessage });
                }

                // Thêm JWT token vào header Authorization của HttpClient
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

                // Gửi yêu cầu POST đến API xóa món ăn
                var response = await client.PostAsync($"https://dulichquangnamdeploy.onrender.com/api/foods/delete/{id}", null);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = $"Xóa thành công ID = {id}";
                }
                else
                {
                    // Đọc thông báo lỗi chi tiết từ body phản hồi của API
                    string errorContent = await response.Content.ReadAsStringAsync();
                    string detailedErrorMessage = response.ReasonPhrase ?? "Lỗi không xác định.";

                    try
                    {
                        // Cố gắng phân tích JSON để lấy thông báo lỗi tùy chỉnh
                        using var jsonDoc = JsonDocument.Parse(errorContent);
                        if (jsonDoc.RootElement.TryGetProperty("message", out var messageElement))
                        {
                            detailedErrorMessage = messageElement.GetString() ?? detailedErrorMessage;
                        }
                        else
                        {
                            // Nếu không có thuộc tính "message", sử dụng toàn bộ nội dung lỗi
                            detailedErrorMessage = errorContent;
                        }
                    }
                    catch (JsonException)
                    {
                        // Nếu nội dung không phải JSON, sử dụng toàn bộ nội dung lỗi
                        detailedErrorMessage = errorContent;
                    }

                    // Cập nhật ErrorMessage với thông báo chi tiết hơn
                    ErrorMessage = $"Lỗi xóa: {response.StatusCode} - {detailedErrorMessage}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi kết nối API: {ex.Message}";
            }

            // Chuyển hướng về trang hiện tại hoặc trang danh sách món ăn, mang theo thông báo
            return RedirectToPage(new { ErrorMessage, SuccessMessage });
        }
    }
}
