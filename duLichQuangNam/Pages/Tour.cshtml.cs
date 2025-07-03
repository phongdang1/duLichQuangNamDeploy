using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using duLichQuangNam.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace duLichQuangNam.Pages
{
    public class TourModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public TourModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public List<Tour> TourList { get; set; } = new();
        public Tour? SelectedTour { get; set; }

        // Danh sách đánh giá của tour (gọi từ /api/rates)
        public List<Rate> TourRates { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? id { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchName { get; set; }

        public async Task OnGetAsync()
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync("https://dulichquangnamdeploy.onrender.com/api/tours");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var allTours = JsonConvert.DeserializeObject<List<Tour>>(jsonString) ?? new();

                if (!string.IsNullOrWhiteSpace(SearchName))
                {
                    var normalizedSearch = RemoveVietnameseSigns(SearchName).ToLower();

                    TourList = allTours
                        .Where(t =>
                            RemoveVietnameseSigns(t.Name).ToLower().Contains(normalizedSearch)
                            || RemoveVietnameseSigns(t.Description ?? "").ToLower().Contains(normalizedSearch)
                        )
                        .ToList();
                }
                else
                {
                    TourList = allTours;
                }

                if (id.HasValue)
                {
                    SelectedTour = TourList.FirstOrDefault(t => t.Id == id.Value);

                    // GỌI API ĐÁNH GIÁ CỦA TOUR
                    var rateUrl = $"https://dulichquangnamdeploy.onrender.com/api/rates?entityType=tour&entityId={id.Value}";
                    var rateResponse = await client.GetAsync(rateUrl);
                    if (rateResponse.IsSuccessStatusCode)
                    {
                        var rateJson = await rateResponse.Content.ReadAsStringAsync();
                        TourRates = JsonConvert.DeserializeObject<List<Rate>>(rateJson) ?? new();
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
