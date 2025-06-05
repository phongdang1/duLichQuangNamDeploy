using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient; // Change from Microsoft.Data.SqlClient
using duLichQuangNam.Models;

namespace duLichQuangNam.Controllers
{
    [ApiController]
    [Route("api/services")]
    public class ServiceController : ControllerBase
    {
        private readonly string _connectionString;

        public ServiceController()
        {
            _connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")!;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Service> services = new();

            try
            {
                // Use MySqlConnection instead of SqlConnection
                using MySqlConnection connection = new(_connectionString);
                connection.Open();

                string sql = @"
                    SELECT
                        s.Id, s.Name, s.Location, s.Type, s.Open_Time, s.Close_Time,
                        s.Email, s.Website, s.Phone, s.Main_Service, s.Deleted, s.description,
                        i.ImageId, i.EntityType, i.EntityId, i.ImgUrl, i.IsPrimary
                    FROM service s
                    LEFT JOIN img i
                        ON i.EntityType = 'Service' AND i.EntityId = s.Id
                    WHERE s.Deleted = 0
                    ORDER BY s.Id";

                // Use MySqlCommand instead of SqlCommand
                using MySqlCommand command = new(sql, connection);
                // Use MySqlDataReader instead of SqlDataReader
                using MySqlDataReader reader = command.ExecuteReader();

                Dictionary<int, Service> serviceDict = new();

                while (reader.Read())
                {
                    int serviceId = reader.GetInt32(0);

                    if (!serviceDict.ContainsKey(serviceId))
                    {
                        var service = new Service
                        {
                            Id = serviceId,
                            Name = reader.GetString(1),
                            Location = reader.GetString(2),
                            Type = reader.GetString(3),
                            OpenTime = reader.GetDateTime(4),
                            CloseTime = reader.GetDateTime(5),
                            Email = reader.GetString(6),
                            Website = reader.GetString(7),
                            Phone = reader.GetString(8),
                            MainService = reader.GetString(9),
                            Deleted = reader.GetBoolean(10),
                            Description = reader.GetString(11),
                            Images = new List<Img>()
                        };

                        serviceDict.Add(serviceId, service);
                    }

                    // Check if ImageId is DBNull before trying to read it
                    if (!reader.IsDBNull(12)) // Column index for ImageId in the query
                    {
                        var image = new Img
                        {
                            ImageId = reader.GetInt32(12),
                            EntityType = reader.GetString(13),
                            EntityId = reader.GetInt32(14),
                            ImgUrl = reader.GetString(15),
                            IsPrimary = reader.GetBoolean(16)
                        };

                        serviceDict[serviceId].Images.Add(image);
                    }
                }

                services = serviceDict.Values.ToList();
                return Ok(services);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi truy vấn CSDL: {ex.Message}");
            }
        }

        // POST: api/services/delete/{id}
        [HttpPost("delete/{id}")]
        public IActionResult DeleteService(int id)
        {
            try
            {
                // Use MySqlConnection instead of SqlConnection
                using MySqlConnection connection = new(_connectionString);
                connection.Open();

                string sql = "UPDATE service SET Deleted = 1 WHERE Id = @id";

                // Use MySqlCommand instead of SqlCommand
                using MySqlCommand command = new(sql, connection);
                command.Parameters.AddWithValue("@id", id);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    return NotFound($"Không tìm thấy dịch vụ với ID = {id}");
                }

                return Ok($"Dịch vụ với ID = {id} đã được đánh dấu là đã xóa.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi xóa dịch vụ: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                // Use MySqlConnection instead of SqlConnection
                using MySqlConnection connection = new(_connectionString);
                connection.Open();

                string sql = @"
                    SELECT
                        s.Id, s.Name, s.Location, s.Type, s.Open_Time, s.Close_Time,
                        s.Email, s.Website, s.Phone, s.Main_Service, s.Deleted, s.Description,
                        i.ImageId, i.EntityType, i.EntityId, i.ImgUrl, i.IsPrimary
                    FROM service s
                    LEFT JOIN img i
                        ON i.EntityType = 'Service' AND i.EntityId = s.Id
                    WHERE s.Id = @id AND s.Deleted = 0";

                // Use MySqlCommand instead of SqlCommand
                using MySqlCommand command = new(sql, connection);
                command.Parameters.AddWithValue("@id", id);

                // Use MySqlDataReader instead of SqlDataReader
                using MySqlDataReader reader = command.ExecuteReader();

                Service? service = null;

                while (reader.Read())
                {
                    if (service == null)
                    {
                        service = new Service
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Location = reader.GetString(2),
                            Type = reader.GetString(3),
                            OpenTime = reader.GetDateTime(4),
                            CloseTime = reader.GetDateTime(5),
                            Email = reader.GetString(6),
                            Website = reader.GetString(7),
                            Phone = reader.GetString(8),
                            MainService = reader.GetString(9),
                            Deleted = reader.GetBoolean(10),
                            Description = reader.GetString(11),
                            Images = new List<Img>()
                        };
                    }

                    if (!reader.IsDBNull(12)) // Column index for ImageId in the query
                    {
                        var img = new Img
                        {
                            ImageId = reader.GetInt32(12),
                            EntityType = reader.GetString(13),
                            EntityId = reader.GetInt32(14),
                            ImgUrl = reader.GetString(15),
                            IsPrimary = reader.GetBoolean(16)
                        };
                        service.Images.Add(img);
                    }
                }

                if (service == null)
                {
                    return NotFound($"Không tìm thấy dịch vụ với ID = {id}");
                }

                return Ok(service);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi truy vấn CSDL: {ex.Message}");
            }
        }
    }
}