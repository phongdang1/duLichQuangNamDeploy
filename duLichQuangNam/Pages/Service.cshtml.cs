using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using duLichQuangNam.Models;
using System.Net.Http;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

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
