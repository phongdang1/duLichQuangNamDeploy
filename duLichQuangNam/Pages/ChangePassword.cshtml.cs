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

    public class ChangePasswordModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public ChangePasswordModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [BindProperty]
        public ChangePasswordViewModel Input { get; set; } = new();

        public string? Message { get; set; }
        public bool IsSuccess { get; set; }

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

                Input.UserName = user.UserName; 
                return Page();
            }
            catch (HttpRequestException)
            {
                return StatusCode(500, "Error");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToPage("/Login");
            }

            var apiUrlGetUser = $"https://localhost:7270/api/Users/{userIdClaim}";
            var user = await _httpClient.GetFromJsonAsync<Users>(apiUrlGetUser);
            if (user == null)
            {
                ModelState.AddModelError("", "Error");
                return Page();
            }
            Input.UserName = user.UserName;


            var apiUrlChangePass = "https://localhost:7270/api/Users/ChangePassword";

            var response = await _httpClient.PostAsJsonAsync(apiUrlChangePass, Input);

            if (response.IsSuccessStatusCode)
            {
                Message = "Success";
                IsSuccess = true;
            }
            else
            {
                var error = await response.Content.ReadFromJsonAsync<ApiError>();
                Message = error?.Message ?? "Fail";
                IsSuccess = false;
            }

            return Page();
        }

        public class ApiError
        {
            public string Message { get; set; } = string.Empty;
        }
    }
}
