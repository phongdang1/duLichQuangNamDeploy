using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient; // Changed from Microsoft.Data.SqlClient
using duLichQuangNam.Models;
using Microsoft.Extensions.Configuration; // Ensure this is present for IConfiguration

namespace duLichQuangNam.Controllers
{
    [ApiController]
    [Route("api/tours")]
    public class TourController : ControllerBase
    {
        private readonly string _connectionString;

        public TourController()
        {
            _connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")!;
        }

        // GET: /api/tours
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Tour> tours = new();
            try
            {
                using MySqlConnection connection = new(_connectionString); // Changed to MySqlConnection
                connection.Open();

                string sql = @"
                    SELECT 
                        t.id, t.type, t.name, t.description, t.detail, t.note, t.deleted,
                        i.imageId, i.entityType, i.entityId, i.imgUrl, i.isPrimary
                    FROM tour t
                    LEFT JOIN img i ON i.entityType = 'Tour' AND i.entityId = t.id
                    WHERE t.deleted = 0
                    ORDER BY t.id";

                using MySqlCommand command = new(sql, connection); // Changed to MySqlCommand
                using MySqlDataReader reader = command.ExecuteReader(); // Changed to MySqlDataReader

                Dictionary<int, Tour> tourDict = new();

                while (reader.Read())
                {
                    int tourId = reader.GetInt32(0);

                    if (!tourDict.ContainsKey(tourId))
                    {
                        var tour = new Tour
                        {
                            Id = tourId,
                            Type = reader.GetString(1),
                            Name = reader.GetString(2),
                            Description = reader.GetString(3),
                            Detail = reader.GetString(4),
                            Note = reader.GetString(5),
                            Deleted = reader.GetBoolean(6),
                            Images = new List<Img>()
                        };
                        tourDict.Add(tourId, tour);
                    }

                    // Check if image data exists before trying to read it
                    // The IsDBNull check for reader.IsDBNull(8) is still valid,
                    // but make sure all subsequent accesses (9, 10, 11) also handle null if they can be null
                    // based on your 'img' table schema for entityType, entityId, imgUrl, isPrimary.
                    // Assuming imgUrl and isPrimary can be null from a LEFT JOIN if no match.
                    if (!reader.IsDBNull(reader.GetOrdinal("imageId"))) // Use column name or ordinal for safety
                    {
                        var img = new Img
                        {
                            ImageId = reader.GetInt32(reader.GetOrdinal("imageId")),
                            EntityType = reader.GetString(reader.GetOrdinal("entityType")),
                            EntityId = reader.GetInt32(reader.GetOrdinal("entityId")),
                            ImgUrl = reader.GetString(reader.GetOrdinal("imgUrl")),
                            IsPrimary = reader.GetBoolean(reader.GetOrdinal("isPrimary"))
                        };
                        tourDict[tourId].Images.Add(img);
                    }
                }

                tours = tourDict.Values.ToList();
                return Ok(tours);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi truy vấn: {ex.Message}");
            }
        }

        // POST: /api/tours/delete/{id}
        [HttpPost("delete/{id}")]
        public IActionResult SoftDelete(int id)
        {
            try
            {
                using MySqlConnection connection = new(_connectionString); // Changed to MySqlConnection
                connection.Open();

                string sql = "UPDATE tour SET deleted = 1 WHERE id = @id";

                using MySqlCommand command = new(sql, connection); // Changed to MySqlCommand
                command.Parameters.AddWithValue("@id", id);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    return NotFound($"Không tìm thấy tour với ID = {id}");
                }

                return Ok($"Đã xoá mềm tour có ID = {id}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi xoá tour: {ex.Message}");
            }
        }

        // GET: /api/tours/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                using MySqlConnection connection = new(_connectionString); // Changed to MySqlConnection
                connection.Open();

                string sql = @"
                    SELECT 
                        t.id, t.type, t.name, t.description, t.detail, t.note, t.deleted,
                        i.imageId, i.entityType, i.entityId, i.imgUrl, i.isPrimary
                    FROM tour t
                    LEFT JOIN img i ON i.entityType = 'Tour' AND i.entityId = t.id
                    WHERE t.id = @id AND t.deleted = 0";

                using MySqlCommand command = new(sql, connection); // Changed to MySqlCommand
                command.Parameters.AddWithValue("@id", id);

                using MySqlDataReader reader = command.ExecuteReader(); // Changed to MySqlDataReader

                Tour? tour = null;

                while (reader.Read())
                {
                    if (tour == null)
                    {
                        tour = new Tour
                        {
                            Id = reader.GetInt32(0),
                            Type = reader.GetString(1),
                            Name = reader.GetString(2),
                            Description = reader.GetString(3),
                            Detail = reader.GetString(4),
                            Note = reader.GetString(5),
                            Deleted = reader.GetBoolean(6),
                            Images = new List<Img>()
                        };
                    }

                    if (!reader.IsDBNull(reader.GetOrdinal("imageId"))) // Using column name for robustness
                    {
                        var img = new Img
                        {
                            ImageId = reader.GetInt32(reader.GetOrdinal("imageId")),
                            EntityType = reader.GetString(reader.GetOrdinal("entityType")),
                            EntityId = reader.GetInt32(reader.GetOrdinal("entityId")),
                            ImgUrl = reader.GetString(reader.GetOrdinal("imgUrl")),
                            IsPrimary = reader.GetBoolean(reader.GetOrdinal("isPrimary"))
                        };
                        tour.Images.Add(img);
                    }
                }

                if (tour == null)
                {
                    return NotFound("Không tìm thấy tour");
                }

                return Ok(tour);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi truy vấn: {ex.Message}");
            }
        }
    }
}