using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using duLichQuangNam.Models;

namespace duLichQuangNam.Controllers
{
    [ApiController]
    [Route("api/tours")]
    public class TourController : ControllerBase
    {
        private readonly string _connectionString;

        public TourController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        // GET: /api/tours
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Tour> tours = new();
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string sql = @"
                    SELECT 
                        t.id, t.type, t.name, t.description, t.detail, t.note, t.deleted,
                        i.imageId, i.entityType, i.entityId, i.imgUrl, i.isPrimary
                    FROM tour t
                    LEFT JOIN img i ON i.entityType = 'Tour' AND i.entityId = t.id
                    WHERE t.deleted = 0
                    ORDER BY t.id";

                using SqlCommand command = new(sql, connection);
                using SqlDataReader reader = command.ExecuteReader();

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

                    if (!reader.IsDBNull(8))
                    {
                        var img = new Img
                        {
                            ImageId = reader.GetInt32(7),
                            EntityType = reader.GetString(8),
                            EntityId = reader.GetInt32(9),
                            ImgUrl = reader.GetString(10),
                            IsPrimary = reader.GetBoolean(11)
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
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                string sql = "UPDATE tour SET deleted = 1 WHERE id = @id";

                using var command = new SqlCommand(sql, connection);
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
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string sql = @"
                    SELECT 
                        t.id, t.type, t.name, t.description, t.detail, t.note, t.deleted,
                        i.imageId, i.entityType, i.entityId, i.imgUrl, i.isPrimary
                    FROM tour t
                    LEFT JOIN img i ON i.entityType = 'Tour' AND i.entityId = t.id
                    WHERE t.id = @id AND t.deleted = 0";

                using SqlCommand command = new(sql, connection);
                command.Parameters.AddWithValue("@id", id);

                using SqlDataReader reader = command.ExecuteReader();

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

                    if (!reader.IsDBNull(8))
                    {
                        var img = new Img
                        {
                            ImageId = reader.GetInt32(7),
                            EntityType = reader.GetString(8),
                            EntityId = reader.GetInt32(9),
                            ImgUrl = reader.GetString(10),
                            IsPrimary = reader.GetBoolean(11)
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
