using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using duLichQuangNam.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace duLichQuangNam.Pages
{

    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public UsersLoginViewModel LoginData { get; set; } = new UsersLoginViewModel();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsJsonAsync("https://dulichquangnamdeploy.onrender.com/api/Users/login", LoginData);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonDocument.Parse(responseBody);

                    var token = jsonDoc.RootElement.GetProperty("token").GetString();
                    var id = jsonDoc.RootElement.GetProperty("id").GetInt32();
                    var role = jsonDoc.RootElement.GetProperty("role").GetString();

<<<<<<< HEAD

=======
                   
>>>>>>> 304d7952140cc9b60aadaae4bb49477f9a4dfdc8
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                        new Claim(ClaimTypes.Role, role),
                        new Claim("token", token ?? "")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
<<<<<<< HEAD
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
                    };


=======
                        IsPersistent = true, 
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
                    };

                    
>>>>>>> 304d7952140cc9b60aadaae4bb49477f9a4dfdc8
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

<<<<<<< HEAD

                    return RedirectToPage("/Index");
=======
                    
                    return RedirectToPage("/Index"); 
>>>>>>> 304d7952140cc9b60aadaae4bb49477f9a4dfdc8
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error Username or Password");
                    return Page();
                }
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Error system");
                return Page();
            }
        }
    }
}