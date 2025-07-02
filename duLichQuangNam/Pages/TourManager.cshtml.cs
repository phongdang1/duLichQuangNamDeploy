using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using duLichQuangNam.Models;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;

namespace duLichQuangNam.Pages
{
    [Authorize(Roles = "adminTour,admin")]
    public class TourManagerModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TourManagerModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<Tour> Tours { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? ErrorMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SuccessMessage { get; set; }

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient();

            try
            {
                var response = await client.GetAsync("https://dulichquangnamdeploy.onrender.com/api/tours");
                if (response.IsSuccessStatusCode)
                {
                    var list = await response.Content.ReadFromJsonAsync<List<Tour>>();
                    if (list != null)
                    {
                        Tours = list;
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
                var response = await client.PostAsync($"https://dulichquangnamdeploy.onrender.com/api/tours/delete/{id}", null); 
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
