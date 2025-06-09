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

            var apiUrl = $"https://localhost:8080/api/Users/{userIdClaim}";
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
                return StatusCode(500, "Kh�ng th? l?y th�ng tin ng??i d�ng t? API.");
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

            var apiUrl = $"https://localhost:8080/api/Users/update/{userIdClaim}";
            try
            {
                var response = await _httpClient.PutAsJsonAsync(apiUrl, User1);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/UserProfile");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Kh�ng th? c?p nh?t th�ng tin ng??i d�ng.");
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
