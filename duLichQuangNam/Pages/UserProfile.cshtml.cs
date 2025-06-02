using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using duLichQuangNam.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace duLichQuangNam.Pages
{
    [Authorize(Roles = "user")]
    public class UserProfileModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public UserProfileModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [BindProperty]
        public Users? User1 { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool isEdit { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToPage("/Login");
            }

            var apiUrl = $"https://localhost:7270/api/Users/{userIdClaim}";
            try
            {
                var user = await _httpClient.GetFromJsonAsync<Users>(apiUrl);
                if (user == null)
                {
                    return NotFound();
                }

                User1 = user;
                return Page();
            }
            catch (HttpRequestException)
            {
                return StatusCode(500, "Không th? l?y thông tin ng??i dùng t? API.");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                isEdit = true;
                return Page();
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || User1 == null)
            {
                return RedirectToPage("/Login");
            }

            var apiUrl = $"https://localhost:7270/api/Users/update/{userIdClaim}";
            try
            {
                var response = await _httpClient.PutAsJsonAsync(apiUrl, User1);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/UserProfile");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Không th? c?p nh?t thông tin ng??i dùng.");
                    isEdit = true;
                    return Page();
                }
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "L?i k?t n?i t?i API.");
                isEdit = true;
                return Page();
            }
        }
    }
}
