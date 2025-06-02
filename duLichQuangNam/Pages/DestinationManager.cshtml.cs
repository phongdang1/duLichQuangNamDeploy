using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using duLichQuangNam.Models;
using System.Net.Http.Json;

namespace duLichQuangNam.Pages
{
    public class DestinationManagerModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DestinationManagerModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<Destination> Destinations { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? ErrorMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SuccessMessage { get; set; }

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient();
            try
            {
                var response = await client.GetAsync("https://localhost:7270/api/destinations"); // ??i l?i URL API c?a b?n
                if (response.IsSuccessStatusCode)
                {
                    var list = await response.Content.ReadFromJsonAsync<List<Destination>>();
                    if (list != null)
                    {
                        Destinations = list;
                    }
                }
                else
                {
                    ErrorMessage = $"Error take data: {response.ReasonPhrase}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error connect API: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var client = _httpClientFactory.CreateClient();

            try
            {
                var response = await client.PostAsync($"https://localhost:7270/api/destinations/delete/{id}", null); // POST soft delete
                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = $"Deleted Successfully ID = {id}";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    ErrorMessage = $"Not Found ID = {id}";
                }
                else
                {
                    ErrorMessage = $"Error deleted: {response.ReasonPhrase}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error connect API: {ex.Message}";
            }

            return RedirectToPage(new { ErrorMessage, SuccessMessage });
        }
    }
}
