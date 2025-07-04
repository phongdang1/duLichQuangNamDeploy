using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient; // Change from Microsoft.Data.SqlClient
using duLichQuangNam.Models;
using Microsoft.AspNetCore.Authorization;

namespace duLichQuangNam.Controllers
{
    [ApiController]
    [Route("api/stays")]
    public class StayController : ControllerBase
    {
        private readonly string _connectionString;

        public StayController()
        {
            _connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")!;
        }

        // GET: /api/stays
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Stay> stays = new();
            try
            {
                // Use MySqlConnection instead of SqlConnection
                using MySqlConnection connection = new(_connectionString);
                connection.Open();

                string sql = @"
                    SELECT
                        s.Id, s.Name, s.Price, s.Type, s.Service_Stay, s.Address,
                        s.Description, s.Mail, s.Website, s.Phone, s.Deleted,
                        i.ImageId, i.EntityType, i.EntityId, i.ImgUrl, i.IsPrimary
                    FROM stay s
                    LEFT JOIN img i ON i.EntityType = 'Stay' AND i.EntityId = s.Id
                    WHERE s.deleted = 0
                    ORDER BY s.Id";

                // Use MySqlCommand instead of SqlCommand
                using MySqlCommand command = new(sql, connection);
                // Use MySqlDataReader instead of SqlDataReader
                using MySqlDataReader reader = command.ExecuteReader();

                Dictionary<int, Stay> stayDict = new();

                while (reader.Read())
                {
                    int stayId = reader.GetInt32(0);

                    if (!stayDict.ContainsKey(stayId))
                    {
                        var stay = new Stay
                        {
                            Id = stayId,
                            Name = reader.GetString(1),
                            Price = (int)reader.GetDecimal(2),
                            Type = reader.GetString(3),
                            ServiceStay = reader.GetString(4),
                            Address = reader.GetString(5),
                            Description = reader.GetString(6),
                            Mail = reader.GetString(7),
                            Website = reader.GetString(8),
                            Phone = reader.GetString(9),
                            Deleted = reader.GetBoolean(10),
                            Images = new List<Img>()
                        };
                        stayDict.Add(stayId, stay);
                    }

                    // Check if ImageId is DBNull before trying to read it
                    if (!reader.IsDBNull(11)) // Check for column index of ImageId
                    {
                        var img = new Img
                        {
                            ImageId = reader.GetInt32(11),
                            EntityType = reader.GetString(12),
                            EntityId = reader.GetInt32(13),
                            ImgUrl = reader.GetString(14),
                            IsPrimary = reader.GetBoolean(15)
                        };
                        stayDict[stayId].Images.Add(img);
                    }
                }

                stays = stayDict.Values.ToList();
                return Ok(stays);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi truy vấn: {ex.Message}");
            }
        }

        // POST: /api/stays/delete/{id}
        [HttpPost("delete/{id}")]
        public IActionResult SoftDelete(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                string sql = "UPDATE stay SET Deleted = 1 WHERE Id = @id";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id", id);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    return NotFound($"Không tìm thấy nơi lưu trú với ID = {id}");
                }

                return Ok($"Đã xoá mềm nơi lưu trú có ID = {id}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi truy vấn: {ex.Message}");
            }
        }

        // GET: /api/stays/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                using MySqlConnection connection = new(_connectionString);
                connection.Open();

                string sql = @"
                    SELECT
                        s.Id, s.Name, s.Price, s.Type, s.Service_Stay, s.Address,
                        s.Description, s.Mail, s.Website, s.Phone, s.Deleted,
                        i.ImageId, i.EntityType, i.EntityId, i.ImgUrl, i.IsPrimary
                    FROM stay s
                    LEFT JOIN img i ON i.EntityType = 'Stay' AND i.EntityId = s.Id
                    WHERE s.id = @id AND s.deleted = 0";

                using MySqlCommand command = new(sql, connection);
                command.Parameters.AddWithValue("@id", id);

                using MySqlDataReader reader = command.ExecuteReader();

                Stay? stay = null;

                while (reader.Read())
                {
                    if (stay == null)
                    {
                        stay = new Stay
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Price = (int)reader.GetDecimal(2),
                            Type = reader.GetString(3),
                            ServiceStay = reader.GetString(4),
                            Address = reader.GetString(5),
                            Description = reader.GetString(6),
                            Mail = reader.GetString(7),
                            Website = reader.GetString(8),
                            Phone = reader.GetString(9),
                            Deleted = reader.GetBoolean(10),
                            Images = new List<Img>()
                        };
                    }

                    // Check if ImageId is DBNull before trying to read it
                    if (!reader.IsDBNull(11)) // Check for column index of ImageId
                    {
                        var img = new Img
                        {
                            ImageId = reader.GetInt32(11),
                            EntityType = reader.GetString(12),
                            EntityId = reader.GetInt32(13),
                            ImgUrl = reader.GetString(14),
                            IsPrimary = reader.GetBoolean(15)
                        };
                        stay.Images.Add(img);
                    }
                }

                if (stay == null)
                {
                    return NotFound("Không tìm thấy nơi lưu trú");
                }

                return Ok(stay);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi truy vấn: {ex.Message}");
            }
        }
    }
}