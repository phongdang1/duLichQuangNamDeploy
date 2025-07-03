using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using duLichQuangNam.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Security.Claims;
using System.Text;

namespace duLichQuangNam.Pages
{
    public class DestinationModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public DestinationModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public List<Destination> DestinationList { get; set; } = new();
        public Destination? SelectedDestination { get; set; }
        // Thuộc tính bind từ form đánh giá
        [BindProperty]
        public int Star { get; set; }

        [BindProperty]
        public string? Comment { get; set; }
        public List<Rate> DestinationRates { get; set; } = new(); // ? Thêm list ?ánh giá

        [BindProperty(SupportsGet = true)]
        public int? id { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchName { get; set; }

        public async Task OnGetAsync()
        {
            var client = _clientFactory.CreateClient();

            // G?i danh sách ??a ?i?m
            var response = await client.GetAsync("https://dulichquangnamdeploy.onrender.com/api/destinations");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var allDestinations = JsonConvert.DeserializeObject<List<Destination>>(jsonString) ?? new();

                if (!string.IsNullOrWhiteSpace(SearchName))
                {
                    var normalizedSearch = RemoveVietnameseSigns(SearchName).ToLower();

                    DestinationList = allDestinations
                        .Where(d =>
                            RemoveVietnameseSigns(d.Name).ToLower().Contains(normalizedSearch)
                            || RemoveVietnameseSigns(d.Description ?? "").ToLower().Contains(normalizedSearch)
                            || RemoveVietnameseSigns(d.Type ?? "").ToLower().Contains(normalizedSearch)
                            || RemoveVietnameseSigns(d.Location ?? "").ToLower().Contains(normalizedSearch)
                        )
                        .ToList();
                }
                else
                {
                    DestinationList = allDestinations;
                }

                // N?u có id ? g?i thêm chi ti?t + ?ánh giá
                if (id.HasValue)
                {
                    SelectedDestination = DestinationList.FirstOrDefault(d => d.Id == id.Value);

                    // ? G?i API ?ánh giá
                    var rateUrl = $"https://dulichquangnamdeploy.onrender.com/api/rates?entityType=destination&entityId={id.Value}";
                    var rateResponse = await client.GetAsync(rateUrl);
                    if (rateResponse.IsSuccessStatusCode)
                    {
                        var rateJson = await rateResponse.Content.ReadAsStringAsync();
                        DestinationRates = JsonConvert.DeserializeObject<List<Rate>>(rateJson) ?? new();
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
