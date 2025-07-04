using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using duLichQuangNam.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace duLichQuangNam.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public UsersLoginViewModel LoginData { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var client = _httpClientFactory.CreateClient();

                var response = await client.PostAsJsonAsync("https://dulichquangnamdeploy.onrender.com/api/Users/login", LoginData);

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Sai tên đăng nhập hoặc mật khẩu.");
                    return Page();
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(responseBody);

                var token = jsonDoc.RootElement.GetProperty("token").GetString();
                var id = jsonDoc.RootElement.GetProperty("id").GetInt32();
                var role = jsonDoc.RootElement.GetProperty("role").GetString();

                // ✅ Lưu vào Session hoặc Cookie
                HttpContext.Session.SetString("JWT_TOKEN", token!);
                HttpContext.Session.SetString("USER_ID", id.ToString());
                HttpContext.Session.SetString("USER_ROLE", role!);


                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi hệ thống: {ex.Message}");
                return Page();
            }
        }
    }
}
