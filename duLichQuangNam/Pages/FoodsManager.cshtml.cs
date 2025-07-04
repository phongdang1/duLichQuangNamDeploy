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
                string? jwtToken = null;

                if (User.Identity?.IsAuthenticated == true)
                {
                   
                    var tokenClaim = User.FindFirst("token");
                    if (tokenClaim != null)
                    {
                        jwtToken = tokenClaim.Value;
                    }
                }

                
                if (string.IsNullOrEmpty(jwtToken))
                {
                    ErrorMessage = "Không tìm thấy token xác thực. Vui lòng đăng nhập lại.";
                    return RedirectToPage(new { ErrorMessage, SuccessMessage });
                }

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

                var response = await client.PostAsync($"https://dulichquangnamdeploy.onrender.com/api/foods/delete/{id}", null);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = $"Xóa thành công ID = {id}";
                }
                else
                {
                   
                    string errorContent = await response.Content.ReadAsStringAsync();
                    string detailedErrorMessage = response.ReasonPhrase ?? "Lỗi không xác định.";

                    try
                    {
                        using var jsonDoc = JsonDocument.Parse(errorContent);
                        if (jsonDoc.RootElement.TryGetProperty("message", out var messageElement))
                        {
                            detailedErrorMessage = messageElement.GetString() ?? detailedErrorMessage;
                        }
                        else
                        {
                            detailedErrorMessage = errorContent;
                        }
                    }
                    catch (JsonException)
                    {
                        detailedErrorMessage = errorContent;
                    }

                    ErrorMessage = $"Lỗi xóa: {response.StatusCode} - {detailedErrorMessage}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi kết nối API: {ex.Message}";
            }

            return RedirectToPage(new { ErrorMessage, SuccessMessage });
        }
    }

}
