using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using duLichQuangNam.Models;

namespace duLichQuangNam.Controllers
{
    [ApiController]
    [Route("api/rates")]
    public class RateController : ControllerBase
    {
        private readonly string _connectionString;

        public RateController()
        {
            _connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")!;
        }

        // GET: /api/rates
        [HttpGet]
        public IActionResult GetAll()
        {
            var rates = new List<Rate>();

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                string sql = "SELECT Id, UserId, Comment, Star, Deleted FROM rate WHERE Deleted = 0";

                using var command = new MySqlCommand(sql, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    rates.Add(new Rate
                    {
                        Id = reader.GetInt32(0),
                        UserId = reader.GetInt32(1),
                        Comment = reader.IsDBNull(2) ? null : reader.GetString(2),
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
