using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using duLichQuangNam.Models;
using System.Net.Http;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Security.Claims;

namespace duLichQuangNam.Pages
{
    public class ServiceModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public ServiceModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public List<Service> ServiceList { get; set; } = new();

        public Service? ServiceDetail { get; set; }
        // Thuộc tính bind từ form đánh giá
        [BindProperty]
        public int Star { get; set; }

        [BindProperty]
        public string? Comment { get; set; }
        public List<Rate> Rates { get; set; } = new(); // Danh sách ?ánh giá

        [BindProperty(SupportsGet = true)]
        public int? id { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchName { get; set; }

        public static string RemoveVietnameseSigns(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text ?? "";

            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            string noDiacritics = sb.ToString().Normalize(NormalizationForm.FormC);
            noDiacritics = Regex.Replace(noDiacritics, @"\p{Mn}", "");
            return noDiacritics;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!id.HasValue || Star < 1 || Star > 5)
            {
                ModelState.AddModelError("", "Dữ liệu đánh giá không hợp lệ.");
                return await ReloadAndReturn(); // load lại dữ liệu
            }

            if (!User.Identity?.IsAuthenticated ?? true || !User.IsInRole("user"))
            {
                return Forbid(); // Chặn nếu không phải user
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Forbid(); // Không lấy được UserId
            }

            var rate = new Rate
            {
                UserId = userId,
                EntityType = "service",
                EntityId = id.Value,
                Star = Star,
                Comment = Comment
            };

            var client = _clientFactory.CreateClient();
            var jsonContent = new StringContent(JsonConvert.SerializeObject(rate), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://dulichquangnamdeploy.onrender.com/api/rates", jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Không thể gửi đánh giá. Vui lòng thử lại.");
            }

            return await ReloadAndReturn(); // Gửi xong load lại dữ liệu
        }

        private async Task<IActionResult> ReloadAndReturn()
        {
            await OnGetAsync();
            return Page();
        }

        public async Task OnGetAsync()
        {
            var client = _clientFactory.CreateClient();

            if (id.HasValue)
            {
                // L?y chi ti?t d?ch v?
                var serviceResponse = await client.GetAsync($"https://dulichquangnamdeploy.onrender.com/api/services/{id.Value}");

                if (serviceResponse.IsSuccessStatusCode)
                {
                    var json = await serviceResponse.Content.ReadAsStringAsync();
                    ServiceDetail = JsonConvert.DeserializeObject<Service>(json);
                }

                // L?y ?ánh giá cho d?ch v? hi?n t?i
                var rateResponse = await client.GetAsync($"https://dulichquangnamdeploy.onrender.com/api/rates?entityType=service&entityId={id.Value}");
                if (rateResponse.IsSuccessStatusCode)
                {
                    var rateJson = await rateResponse.Content.ReadAsStringAsync();
                    Rates = JsonConvert.DeserializeObject<List<Rate>>(rateJson) ?? new();
                }
            }
            else
            {
                var response = await client.GetAsync("https://dulichquangnamdeploy.onrender.com/api/services");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var allServices = JsonConvert.DeserializeObject<List<Service>>(json) ?? new();

                    if (!string.IsNullOrWhiteSpace(SearchName))
                    {
                        var searchNormalized = RemoveVietnameseSigns(SearchName).ToLower();

                        ServiceList = allServices
                            .Where(s => s.Name != null && RemoveVietnameseSigns(s.Name).ToLower().Contains(searchNormalized))
                            .ToList();
                    }
                    else
                    {
                        ServiceList = allServices;
                    }
                }
            }
        }
    }
}
