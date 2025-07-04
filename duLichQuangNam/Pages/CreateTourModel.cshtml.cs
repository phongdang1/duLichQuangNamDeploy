using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using MySql.Data.MySqlClient;
using duLichQuangNam.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace duLichQuangNam.Pages
{
    [Authorize(Roles = "admin, adminTour")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin, adminTour")]
    public class CreateTourModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        public CreateTourModel(IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
        }

        [BindProperty] public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [StringLength(100)] public string Type { get; set; } = string.Empty;

            [Required, StringLength(200)]
            public string Name { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;
            public string Detail { get; set; } = string.Empty;
            public string Note { get; set; } = string.Empty;

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
                    INSERT INTO tour (Type,Name,Description,Detail,Note,Deleted)
                    VALUES (@Type,@Name,@Desc,@Detail,@Note,0);
                    SELECT LAST_INSERT_ID();", conn);

                cmd.Parameters.AddWithValue("@Type", Input.Type);
                cmd.Parameters.AddWithValue("@Name", Input.Name);
                cmd.Parameters.AddWithValue("@Desc", Input.Description);
                cmd.Parameters.AddWithValue("@Detail", Input.Detail);
                cmd.Parameters.AddWithValue("@Note", Input.Note);

                var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                if (Input.Images?.Count > 0)
                {
                    var root = Path.Combine(_env.WebRootPath, "uploads", "tours", newId.ToString());
                    Directory.CreateDirectory(root);

                    for (int i = 0; i < Input.Images.Count; i++)
                    {
                        var img = Input.Images[i];
                        var fname = $"{Guid.NewGuid()}{Path.GetExtension(img.FileName)}";
                        var fpath = Path.Combine(root, fname);
                        var relUrl = $"/uploads/tours/{newId}/{fname}";

                        await using (var fs = System.IO.File.Create(fpath))
                        {
                            await img.CopyToAsync(fs);
                        }

                        var imgCmd = new MySqlCommand(@"
                            INSERT INTO img (EntityType,EntityId,ImgUrl,IsPrimary)
                            VALUES ('tour',@Id,@Url,@IsPri);", conn);

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