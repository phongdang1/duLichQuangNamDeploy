using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Authorization;


namespace duLichQuangNam.Pages
{
    [Authorize(Roles = "admin, adminDes")]
    public class CreateDestinationModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        public CreateDestinationModel(IConfiguration config, IWebHostEnvironment env)
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
            public string Type { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;

            [Required] public TimeSpan OpenTime { get; set; }
            [Required] public TimeSpan CloseTime { get; set; }

            public decimal? Price { get; set; }

            [EmailAddress]
            public string Mail { get; set; } = string.Empty;

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
                INSERT INTO Destination (Name,Description,Type,Location,Open_Time,Close_Time,Price,Mail,Deleted)
                VALUES (@Name,@Desc,@Type,@Loc,@Open,@Close,@Price,@Mail,0);
                SELECT LAST_INSERT_ID();", conn);

                insertCmd.Parameters.AddWithValue("@Name", Input.Name);
                insertCmd.Parameters.AddWithValue("@Desc", Input.Description);
                insertCmd.Parameters.AddWithValue("@Type", Input.Type);
                insertCmd.Parameters.AddWithValue("@Loc", Input.Location);
                insertCmd.Parameters.AddWithValue("@Open", Input.OpenTime);
                insertCmd.Parameters.AddWithValue("@Close", Input.CloseTime);
                insertCmd.Parameters.AddWithValue("@Price", (object?)Input.Price ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@Mail", Input.Mail);

                var newId = Convert.ToInt32(await insertCmd.ExecuteScalarAsync());


                if (Input.Images?.Count > 0)
                {
                    var root = Path.Combine(_env.WebRootPath, "uploads", "destinations", newId.ToString());
                    Directory.CreateDirectory(root);

                    for (int i = 0; i < Input.Images.Count; i++)
                    {
                        var img = Input.Images[i];
                        var fname = $"{Guid.NewGuid()}{Path.GetExtension(img.FileName)}";
                        var fpath = Path.Combine(root, fname);
                        var relUrl = $"/uploads/destinations/{newId}/{fname}";

                        await using (var fs = System.IO.File.Create(fpath))
                        {
                            await img.CopyToAsync(fs);
                        }

                        var imgCmd = new MySqlCommand(@"
                        INSERT INTO img (EntityType,EntityId,ImgUrl,IsPrimary)
                        VALUES ('destination',@Id,@Url,@IsPri);", conn);

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
                TempData["Error"] = "Error!!" + ex.Message;
                return Page();
            }
        }
    }
}