using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using duLichQuangNam.Models;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Text;
using System.Security.Claims;

namespace duLichQuangNam.Pages
{
    public class FoodsModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public FoodsModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public List<Foods> FoodList { get; set; } = new();
        public Foods? SelectedFood { get; set; }
        public List<Rate> FoodRates { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? id { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchName { get; set; }

        // Thuộc tính bind từ form đánh giá
        [BindProperty]
        public int Star { get; set; }

        [BindProperty]
        public string? Comment { get; set; }

        public async Task OnGetAsync()
        {
            var client = _clientFactory.CreateClient();

            if (id.HasValue)
            {
                var response = await client.GetAsync($"https://dulichquangnamdeploy.onrender.com/api/foods/{id.Value}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    SelectedFood = JsonConvert.DeserializeObject<Foods>(json);
                }

                var rateResponse = await client.GetAsync($"https://dulichquangnamdeploy.onrender.com/api/rates?entityType=food&entityId={id.Value}");
                if (rateResponse.IsSuccessStatusCode)
                {
                    var rateJson = await rateResponse.Content.ReadAsStringAsync();
                    FoodRates = JsonConvert.DeserializeObject<List<Rate>>(rateJson) ?? new();
                }
            }
            else
            {
                var response = await client.GetAsync("https://dulichquangnamdeploy.onrender.com/api/foods");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var allFoods = JsonConvert.DeserializeObject<List<Foods>>(json) ?? new();

                    if (!string.IsNullOrWhiteSpace(SearchName))
                    {
                        var normalizedSearch = RemoveVietnameseSigns(SearchName).ToLower();

                        FoodList = allFoods
                            .Where(f =>
                                RemoveVietnameseSigns(f.Name).ToLower().Contains(normalizedSearch)
                                || RemoveVietnameseSigns(f.Description ?? "").ToLower().Contains(normalizedSearch)
                            )
                            .ToList();
                    }
                    else
                    {
                        FoodList = allFoods;
                    }
                }
            }
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
                EntityType = "food",
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

        private string RemoveVietnameseSigns(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            text = text.Normalize(System.Text.NormalizationForm.FormD);

            var sb = new System.Text.StringBuilder();

            foreach (var ch in text)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(ch);
                }
            }

            var noDiacritics = sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
            noDiacritics = Regex.Replace(noDiacritics, @"[^\w\s]", "");

            return noDiacritics;
        }
    }
}
