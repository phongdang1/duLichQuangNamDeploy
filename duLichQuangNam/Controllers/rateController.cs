using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using duLichQuangNam.Models;

namespace duLichQuangNam.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RateController : ControllerBase
    {
        private readonly string _connectionString;

        public RateController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        // GET: /Rate
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Rate> rates = new();

            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string sql = "SELECT * FROM rate WHERE deleted = 0";
                using SqlCommand command = new(sql, connection);
                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    rates.Add(new Rate
                    {
                        Id = reader.GetInt32(0),
                        UserId = reader.GetInt32(1),
                        Comment = reader.GetString(2),
                        Star = reader.GetInt32(3),
                        Deleted = reader.GetBoolean(4)
                    });
                }

                return Ok(rates);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi truy vấn: {ex.Message}");
            }
        }
    }
}
