using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using duLichQuangNam.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace duLichQuangNam.Pages
{
    public class StayModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public StayModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public List<Stay> StayList { get; set; } = new();
        public Stay? SelectedStay { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? id { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchName { get; set; }

        public async Task OnGetAsync()
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync("https://localhost:8080/api/stays");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var allStays = JsonConvert.DeserializeObject<List<Stay>>(jsonString) ?? new();

                if (!string.IsNullOrWhiteSpace(SearchName))
                {
                    var normalizedSearch = RemoveVietnameseSigns(SearchName).ToLower();

                    StayList = allStays
                        .Where(s =>
                            RemoveVietnameseSigns(s.Name).ToLower().Contains(normalizedSearch)
                            || RemoveVietnameseSigns(s.Address ?? "").ToLower().Contains(normalizedSearch)
                        )
                        .ToList();
                }
                else
                {
                    StayList = allStays;
                }

                if (id.HasValue)
                {
                    SelectedStay = StayList.FirstOrDefault(s => s.Id == id.Value);
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
