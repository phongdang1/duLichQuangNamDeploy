using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using duLichQuangNam.Models;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;

namespace duLichQuangNam.Pages
{
    [Authorize(Roles = "user")]
    public class ScheduleModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public ScheduleModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public List<Destination> DestinationList { get; set; } = new();
        public List<Destination> PagedDestinations { get; set; } = new();
        public Destination? SelectedDestination { get; set; }

        public List<Service> ServiceList { get; set; } = new();
        public List<Service> PagedServices { get; set; } = new();

        [BindProperty(SupportsGet = true)] public int? id { get; set; }
        [BindProperty(SupportsGet = true)] public int page { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int servicePage { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public string? DestinationSearch { get; set; }
        [BindProperty(SupportsGet = true)] public string? ServiceSearch { get; set; }

        public int PageSize { get; set; } = 6;

        public int TotalDestinationPages { get; set; }
        public int TotalServicePages { get; set; }

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

            var destinationResponse = await client.GetAsync("https://localhost:8080/api/destinations");
            if (destinationResponse.IsSuccessStatusCode)
            {
                var json = await destinationResponse.Content.ReadAsStringAsync();
                DestinationList = JsonConvert.DeserializeObject<List<Destination>>(json) ?? new();

                if (!string.IsNullOrWhiteSpace(DestinationSearch))
                {
                    var searchNormalized = RemoveVietnameseSigns(DestinationSearch).ToLower();
                    DestinationList = DestinationList
                        .Where(d => d.Name != null && RemoveVietnameseSigns(d.Name).ToLower().Contains(searchNormalized))
                        .ToList();
                }

                TotalDestinationPages = (int)Math.Ceiling(DestinationList.Count / (double)PageSize);
                PagedDestinations = DestinationList
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                if (id.HasValue)
                {
                    SelectedDestination = DestinationList.FirstOrDefault(d => d.Id == id.Value);
                }
            }

            var serviceResponse = await client.GetAsync("https://localhost:8080/api/services");
            if (serviceResponse.IsSuccessStatusCode)
            {
                var json = await serviceResponse.Content.ReadAsStringAsync();
                ServiceList = JsonConvert.DeserializeObject<List<Service>>(json) ?? new();

                if (!string.IsNullOrWhiteSpace(ServiceSearch))
                {
                    var searchNormalized = RemoveVietnameseSigns(ServiceSearch).ToLower();
                    ServiceList = ServiceList
                        .Where(s => s.Name != null && RemoveVietnameseSigns(s.Name).ToLower().Contains(searchNormalized))
                        .ToList();
                }

                TotalServicePages = (int)Math.Ceiling(ServiceList.Count / (double)PageSize);
                PagedServices = ServiceList
                    .Skip((servicePage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();
            }
        }
    }
}
