using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Authentication.JwtBearer;

[Authorize(Roles = "admin, adminFood")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin, adminFood")]
public class CreateFoodModel : PageModel
{
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    public CreateFoodModel(IConfiguration config, IWebHostEnvironment env)
    {
        _config = config;
        _env = env;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Fee >= 0")]
        public decimal Price { get; set; }

        public IFormFileCollection? Images { get; set; }
        public int PrimaryIndex { get; set; } = 0;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Error data";
            return Page();
        }

        try
        {
            using var conn = new MySqlConnection(Environment.GetEnvironmentVariable("DEFAULT_CONNECTION"));
            await conn.OpenAsync();

            var insertCmd = new MySqlCommand(@"
                INSERT INTO food (Name, Description, Price, Deleted)
                VALUES (@Name, @Desc, @Price, 0);
                SELECT LAST_INSERT_ID();", conn);

            insertCmd.Parameters.AddWithValue("@Name", Input.Name);
            insertCmd.Parameters.AddWithValue("@Desc", Input.Description);
            insertCmd.Parameters.AddWithValue("@Price", Input.Price);

            var newId = Convert.ToInt32(await insertCmd.ExecuteScalarAsync());

            if (Input.Images?.Count > 0)
            {
                var root = Path.Combine(_env.WebRootPath, "uploads", "foods", newId.ToString());
                Directory.CreateDirectory(root);

                for (int i = 0; i < Input.Images.Count; i++)
                {
                    var img = Input.Images[i];
                    var fname = $"{Guid.NewGuid()}{Path.GetExtension(img.FileName)}";
                    var fpath = Path.Combine(root, fname);
                    var relUrl = $"/uploads/foods/{newId}/{fname}";

                    await using (var fs = System.IO.File.Create(fpath))
                    {
                        await img.CopyToAsync(fs);
                    }

                    var imgCmd = new MySqlCommand(@"
                        INSERT INTO img (EntityType, EntityId, ImgUrl, IsPrimary)
                        VALUES ('food', @Id, @Url, @IsPri);", conn);

                    imgCmd.Parameters.AddWithValue("@Id", newId);
                    imgCmd.Parameters.AddWithValue("@Url", relUrl);
                    imgCmd.Parameters.AddWithValue("@IsPri", i == Input.PrimaryIndex ? 1 : 0);
                    await imgCmd.ExecuteNonQueryAsync();
                }
            }

            TempData["Success"] = "Success!";
            return Page();
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Error: " + ex.Message;
            return Page();
        }
    }
}
