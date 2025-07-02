using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using duLichQuangNam.Models;
using System.Text.RegularExpressions;

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

        public async Task OnGetAsync()
        {
            var client = _clientFactory.CreateClient();

            if (id.HasValue)
            {
                // Lấy thông tin món ăn
                var response = await client.GetAsync($"https://dulichquangnamdeploy.onrender.com/api/foods/{id.Value}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    SelectedFood = JsonConvert.DeserializeObject<Foods>(json);
                }

                // Gọi API lấy đánh giá theo entityType=food và entityId
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
