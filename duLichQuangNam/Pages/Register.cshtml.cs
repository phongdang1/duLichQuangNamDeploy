using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using duLichQuangNam.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;

namespace duLichQuangNam.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RegisterModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public UsersRegistrationViewModel RegisterData { get; set; } = new UsersRegistrationViewModel();

        public string Message { get; set; }

        public void OnGet()
        {
            
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                
                var client = _httpClientFactory.CreateClient();

               
                var response = await client.PostAsJsonAsync("https://dulichquangnamdeploy.onrender.com/api/Users/register", RegisterData);

               
                if (response.IsSuccessStatusCode)
                {
                    
                    return RedirectToPage("/Login");
                }
                else
                {
                   
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonDocument.Parse(responseBody);
                    var errorMessage = jsonDoc.RootElement.GetProperty("message").GetString();

                    Message = errorMessage;
                    return Page();
                }
            }
            catch
            {
             
                Message = "Error system";
                return Page();
            }
        }
    }
}
