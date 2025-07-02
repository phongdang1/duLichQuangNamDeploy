using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using duLichQuangNam.Models;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;

namespace duLichQuangNam.Pages
{
    [Authorize(Roles = "adminservice,admin")]
    public class ServiceManagerModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ServiceManagerModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<Service> Services { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? ErrorMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SuccessMessage { get; set; }

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient();

            try
            {
                var response = await client.GetAsync("https://dulichquangnamdeploy.onrender.com/api/services"); 
                if (response.IsSuccessStatusCode)
                {
                    var list = await response.Content.ReadFromJsonAsync<List<Service>>();
                    if (list != null)
                    {
                        Services = list;
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
                var response = await client.PostAsync($"https://dulichquangnamdeploy.onrender.com/api/services/delete/{id}", null); 
                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = $"Deleted successfully ID = {id}";
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
