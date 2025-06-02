using Microsoft.AspNetCore.Mvc.RazorPages;
using duLichQuangNam.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace duLichQuangNam.Pages
{
    [Authorize(Roles = "user")]
    public class MyScheduleModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public MyScheduleModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public List<Schedule> Schedules { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Login");
            }

            var response = await _httpClient.GetFromJsonAsync<List<Schedule>>(
                $"https://localhost:7270/api/schedules/user/{userId}/details");

            if (response != null)
            {
                Schedules = response;
            }

            return Page();
        }
    }
}
