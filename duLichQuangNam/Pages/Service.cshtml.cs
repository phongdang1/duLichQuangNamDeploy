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
                var response = await client.GetAsync($"https://localhost:7270/api/services/{id.Value}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    ServiceDetail = JsonConvert.DeserializeObject<Service>(json);
                }
                else
                {
                    ServiceDetail = null;
                }
            }
            else
            {
                var response = await client.GetAsync("https://localhost:7270/api/services");
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
