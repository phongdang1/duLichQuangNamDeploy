using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using duLichQuangNam.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

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

        [BindProperty(SupportsGet = true)]
        public int? id { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchName { get; set; }  // Thêm property nh?n t? query string

        public async Task OnGetAsync()
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync("https://localhost:8080/api/destinations");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var allDestinations = JsonConvert.DeserializeObject<List<Destination>>(jsonString) ?? new();

                if (!string.IsNullOrWhiteSpace(SearchName))
                {
                    // Chu?n hóa t? khóa tìm ki?m
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

                if (id.HasValue)
                {
                    SelectedDestination = DestinationList.FirstOrDefault(d => d.Id == id.Value);
                }
            }
        }

        // Hàm lo?i b? d?u ti?ng Vi?t
        private string RemoveVietnameseSigns(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // Chu?n hóa Unicode
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

            // Remove special characters (d?u câu)
            var noDiacritics = sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
            noDiacritics = Regex.Replace(noDiacritics, @"[^\w\s]", ""); // ch? gi? ch?, s?, kho?ng tr?ng

            return noDiacritics;
        }
    }
}
