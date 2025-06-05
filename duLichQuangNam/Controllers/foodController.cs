using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using duLichQuangNam.Models;

namespace duLichQuangNam.Controllers
{
    [ApiController]
    [Route("api/foods")]
    public class FoodsController : ControllerBase
    {
        private readonly string _connectionString;

        public FoodsController()
        {
            _connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")!;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Foods> foods = new();
            try
            {
                using MySqlConnection connection = new(_connectionString);
                connection.Open();

                string sql = @"
                    SELECT 
                        f.Id, f.Name, f.Price, f.Description, f.Deleted,
                        i.ImageId, i.EntityType, i.EntityId, i.ImgUrl, i.IsPrimary
                    FROM food f
                    LEFT JOIN img i ON i.EntityType = 'Food' AND i.EntityId = f.Id
                    WHERE f.deleted = 0
                    ORDER BY f.Id";

                using MySqlCommand command = new(sql, connection);
                using MySqlDataReader reader = command.ExecuteReader();

                Dictionary<int, Foods> foodDict = new();

                while (reader.Read())
                {
                    int foodId = reader.GetInt32(0);

                    if (!foodDict.ContainsKey(foodId))
                    {
                        var food = new Foods
                        {
                            Id = foodId,
                            Name = reader.GetString(1),
                            Price = reader.GetDecimal(2),
                            Description = reader.GetString(3),
                            Deleted = reader.GetBoolean(4),
                            Images = new List<Img>()
                        };
                        foodDict.Add(foodId, food);
                    }

                    if (!reader.IsDBNull(6))
                    {
                        var img = new Img
                        {
                            ImageId = reader.GetInt32(5),
                            EntityType = reader.GetString(6),
                            EntityId = reader.GetInt32(7),
                            ImgUrl = reader.GetString(8),
                            IsPrimary = reader.GetBoolean(9)
                        };
                        foodDict[foodId].Images.Add(img);
                    }
                }

                foods = foodDict.Values.ToList();
                return Ok(foods);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi truy vấn: {ex.Message}");
            }
        }

        [HttpPost("delete/{id}")]
        public IActionResult SoftDelete(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                string sql = "UPDATE food SET Deleted = 1 WHERE Id = @id";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id", id);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    return NotFound($"Không tìm thấy món ăn với ID = {id}");
                }

                return Ok($"Đã xóa mềm món ăn có ID = {id}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi truy vấn: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                using MySqlConnection connection = new(_connectionString);
                connection.Open();

                string sql = @"
                    SELECT 
                        f.Id, f.Name, f.Price, f.Description, f.Deleted,
                        i.ImageId, i.EntityType, i.EntityId, i.ImgUrl, i.IsPrimary
                    FROM food f
                    LEFT JOIN img i ON i.EntityType = 'Food' AND i.EntityId = f.Id
                    WHERE f.id = @id AND f.deleted = 0";

                using MySqlCommand command = new(sql, connection);
                command.Parameters.AddWithValue("@id", id);

                using MySqlDataReader reader = command.ExecuteReader();

                Foods? food = null;

                while (reader.Read())
                {
                    if (food == null)
                    {
                        food = new Foods
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Price = reader.GetDecimal(2),
                            Description = reader.GetString(3),
                            Deleted = reader.GetBoolean(4),
                            Images = new List<Img>()
                        };
                    }

                    if (!reader.IsDBNull(6))
                    {
                        var img = new Img
                        {
                            ImageId = reader.GetInt32(5),
                            EntityType = reader.GetString(6),
                            EntityId = reader.GetInt32(7),
                            ImgUrl = reader.GetString(8),
                            IsPrimary = reader.GetBoolean(9)
                        };
                        food.Images.Add(img);
                    }
                }

                if (food == null)
                {
                    return NotFound("Không tìm thấy món ăn");
                }

                return Ok(food);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi truy vấn: {ex.Message}");
            }
        }
    }
}