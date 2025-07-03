using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using duLichQuangNam.Models;
using System.Data;

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

        // GET: /api/rates?entityType=food&entityId=5
        [HttpGet]
        public IActionResult GetByEntity([FromQuery] string entityType, [FromQuery] int entityId)
        {
            var rates = new List<Rate>();

            if (string.IsNullOrWhiteSpace(entityType) || entityId <= 0)
            {
                return BadRequest("Thiếu hoặc sai tham số entityType và entityId.");
            }

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                string sql = @"
                    SELECT Id, User_Id, Comment, Star, Deleted, Entity_Type, Entity_Id 
                    FROM rate 
                    WHERE Deleted = 0 AND Entity_Type = @entityType AND Entity_Id = @entityId";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@entityType", entityType);
                command.Parameters.AddWithValue("@entityId", entityId);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    rates.Add(new Rate
                    {
                        Id = reader.GetInt32("Id"),
                        UserId = reader.GetInt32("User_Id"),
                        Comment = reader.IsDBNull("Comment") ? null : reader.GetString("Comment"),
                        Star = reader.GetInt32("Star"),
                        Deleted = reader.GetBoolean("Deleted"),
                        EntityType = reader.GetString("Entity_Type"),
                        EntityId = reader.GetInt32("Entity_Id")
                    });
                }

                return Ok(rates);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi truy vấn: {ex.Message}");
            }
        }

        // POST: /api/rates
        [HttpPost]
        public IActionResult CreateRate([FromBody] Rate newRate)
        {
            if (newRate == null ||
                newRate.UserId <= 0 ||
                string.IsNullOrWhiteSpace(newRate.EntityType) ||
                newRate.EntityId <= 0 ||
                newRate.Star < 1 || newRate.Star > 5)
            {
                return BadRequest("Thông tin đánh giá không hợp lệ.");
            }

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                string insertSql = @"
                    INSERT INTO rate (User_Id, Comment, Star, Deleted, Entity_Type, Entity_Id)
                    VALUES (@userId, @comment, @star, 0, @entityType, @entityId)";

                using var command = new MySqlCommand(insertSql, connection);
                command.Parameters.AddWithValue("@userId", newRate.UserId);
                command.Parameters.AddWithValue("@comment", (object?)newRate.Comment ?? DBNull.Value);
                command.Parameters.AddWithValue("@star", newRate.Star);
                command.Parameters.AddWithValue("@entityType", newRate.EntityType);
                command.Parameters.AddWithValue("@entityId", newRate.EntityId);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    return Ok(new { message = "Đánh giá đã được thêm thành công." });
                }
                else
                {
                    return StatusCode(500, "Không thể thêm đánh giá.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi thêm đánh giá: {ex.Message}");
            }
        }
    }
}
