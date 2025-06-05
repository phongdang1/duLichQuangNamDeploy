using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using MySql.Data.MySqlClient;
using duLichQuangNam.Models;
using Microsoft.AspNetCore.Authorization;

namespace duLichQuangNam.Pages
{
    [Authorize(Roles = "admin")]
    public class CreateServiceModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        public CreateServiceModel(IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
        }

        [BindProperty] public InputModel Input { get; set; } = new();


        public class InputModel
        {
            [Required, StringLength(200)]
            public string Name { get; set; } = string.Empty;

            public string Location { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;

            [Required] public TimeSpan OpenTime { get; set; }
            [Required] public TimeSpan CloseTime { get; set; }

            public string Email { get; set; } = string.Empty;
            public string Website { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string MainService { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;

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
                await using var conn = new MySqlConnection(Environment.GetEnvironmentVariable("DEFAULT_CONNECTION"));
                await conn.OpenAsync();

                var cmd = new MySqlCommand(@"
                    INSERT INTO service (Name,Location,Type,Open_Time,Close_Time,Email,Website,Phone,Main_Service,Description,Deleted)
                    VALUES (@Name,@Loc,@Type,@Open,@Close,@Email,@Web,@Phone,@Main,@Desc,0);
                    SELECT LAST_INSERT_ID();", conn);

                cmd.Parameters.AddWithValue("@Name", Input.Name);
                cmd.Parameters.AddWithValue("@Loc", Input.Location);
                cmd.Parameters.AddWithValue("@Type", Input.Type);
                cmd.Parameters.AddWithValue("@Open", Input.OpenTime);
                cmd.Parameters.AddWithValue("@Close", Input.CloseTime);
                cmd.Parameters.AddWithValue("@Email", Input.Email);
                cmd.Parameters.AddWithValue("@Web", Input.Website);
                cmd.Parameters.AddWithValue("@Phone", Input.Phone);
                cmd.Parameters.AddWithValue("@Main", Input.MainService);
                cmd.Parameters.AddWithValue("@Desc", Input.Description);

                var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                if (Input.Images?.Count > 0)
                {
                    var root = Path.Combine(_env.WebRootPath, "uploads", "services", newId.ToString());
                    Directory.CreateDirectory(root);

                    for (int i = 0; i < Input.Images.Count; i++)
                    {
                        var img = Input.Images[i];
                        var fname = $"{Guid.NewGuid()}{Path.GetExtension(img.FileName)}";
                        var fpath = Path.Combine(root, fname);
                        var relUrl = $"/uploads/services/{newId}/{fname}";

                        await using (var fs = System.IO.File.Create(fpath))
                        {
                            await img.CopyToAsync(fs);
                        }

                        var imgCmd = new MySqlCommand(@"
                            INSERT INTO img (EntityType,EntityId,ImgUrl,IsPrimary)
                            VALUES ('service',@Id,@Url,@IsPri);", conn);

                        imgCmd.Parameters.AddWithValue("@Id", newId);
                        imgCmd.Parameters.AddWithValue("@Url", relUrl);
                        imgCmd.Parameters.AddWithValue("@IsPri", i == Input.PrimaryIndex);
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
}