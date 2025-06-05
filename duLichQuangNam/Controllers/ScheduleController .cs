using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using duLichQuangNam.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace duLichQuangNam.Controllers
{
    [ApiController]
    [Route("api/schedules")]
    public class ScheduleController : ControllerBase
    {
        private readonly string _connectionString;

        public ScheduleController()
        {
            _connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")!;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Schedule> schedules = new();

            try
            {
                using MySqlConnection connection = new(_connectionString);
                connection.Open();

                string sql = @"
                    SELECT
                        s.id, s.user_id, s.name, s.start_date, s.end_date, s.description, s.created_at,
                        si.id AS item_id, si.schedule_id, si.entity_type, si.entity_id, si.day_order
                    FROM schedule s
                    LEFT JOIN schedule_items si ON si.schedule_id = s.id
                    ORDER BY s.id, si.day_order";

                using MySqlCommand command = new(sql, connection);
                using MySqlDataReader reader = command.ExecuteReader();

                Dictionary<int, Schedule> scheduleDict = new();

                while (reader.Read())
                {
                    int scheduleId = reader.GetInt32(0);

                    if (!scheduleDict.ContainsKey(scheduleId))
                    {
                        var schedule = new Schedule
                        {
                            Id = scheduleId,
                            UserId = reader.GetInt32(1),
                            Name = reader.GetString(2),
                            StartDate = reader.GetDateTime(3),
                            EndDate = reader.GetDateTime(4),
                            Description = reader.GetString(5),
                            CreatedAt = reader.GetDateTime(6),
                            ScheduleItems = new List<ScheduleItem>()
                        };
                        scheduleDict[scheduleId] = schedule;
                    }

                    if (!reader.IsDBNull(7))
                    {
                        var item = new ScheduleItem
                        {
                            Id = reader.GetInt32(7),
                            ScheduleId = reader.GetInt32(8),
                            EntityType = reader.GetString(9),
                            EntityId = reader.GetInt32(10),
                            DayOrder = reader.GetInt32(11)
                        };
                        scheduleDict[scheduleId].ScheduleItems.Add(item);
                    }
                }

                schedules = scheduleDict.Values.ToList();
                return Ok(schedules);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi truy vấn: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult Create([FromBody] Schedule model)
        {
            if (model == null || string.IsNullOrEmpty(model.Name) || model.UserId == 0)
                return BadRequest("Dữ liệu không hợp lệ.");

            try
            {
                using MySqlConnection connection = new(_connectionString);
                connection.Open();

                string checkScheduleSql = @"
                    SELECT COUNT(*)
                    FROM schedule
                    WHERE user_id = @user_id
                    AND start_date = @start_date";

                using MySqlCommand checkScheduleCmd = new(checkScheduleSql, connection);
                checkScheduleCmd.Parameters.AddWithValue("@user_id", model.UserId);
                checkScheduleCmd.Parameters.AddWithValue("@start_date", model.StartDate);

                int existingSchedules = Convert.ToInt32(checkScheduleCmd.ExecuteScalar());

                if (existingSchedules > 0)
                {
                    return Conflict("Đã có lịch trình được tạo cho ngày này.");
                }

                string insertScheduleSql = @"
                    INSERT INTO schedule (user_id, name, start_date, end_date, description, created_at)
                    VALUES (@user_id, @name, @start_date, @end_date, @description, NOW());
                    SELECT LAST_INSERT_ID();";

                using MySqlCommand insertScheduleCmd = new(insertScheduleSql, connection);
                insertScheduleCmd.Parameters.AddWithValue("@user_id", model.UserId);
                insertScheduleCmd.Parameters.AddWithValue("@name", model.Name);
                insertScheduleCmd.Parameters.AddWithValue("@start_date", model.StartDate);
                insertScheduleCmd.Parameters.AddWithValue("@end_date", model.EndDate);
                insertScheduleCmd.Parameters.AddWithValue("@description", model.Description ?? "");

                int scheduleId = Convert.ToInt32(insertScheduleCmd.ExecuteScalar());

                if (model.ScheduleItems != null && model.ScheduleItems.Count > 0)
                {
                    foreach (var item in model.ScheduleItems)
                    {
                        string insertItemSql = @"
                            INSERT INTO schedule_items (schedule_id, entity_type, entity_id, day_order)
                            VALUES (@schedule_id, @entity_type, @entity_id, @day_order)";
                        using MySqlCommand insertItemCmd = new(insertItemSql, connection);
                        insertItemCmd.Parameters.AddWithValue("@schedule_id", scheduleId);
                        insertItemCmd.Parameters.AddWithValue("@entity_type", item.EntityType);
                        insertItemCmd.Parameters.AddWithValue("@entity_id", item.EntityId);
                        insertItemCmd.Parameters.AddWithValue("@day_order", item.DayOrder);
                        insertItemCmd.ExecuteNonQuery();
                    }
                }

                return Ok(new { success = true, scheduleId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi tạo lịch trình: {ex.Message}");
            }
        }

        [HttpGet("user/{userId}/details")]
        public IActionResult GetUserScheduleWithDetails(int userId)
        {
            try
            {
                using MySqlConnection connection = new(_connectionString);
                connection.Open();

                string sql = @"
                    SELECT
                        s.id, s.user_id, s.name, s.start_date, s.end_date, s.description, s.created_at,
                        si.id AS item_id, si.entity_type, si.entity_id, si.day_order,

                        d.name AS destination_name, d.description AS destination_desc, d.location AS destination_location,
                        d.open_time AS destination_open_time, d.close_time AS destination_close_time, d.price AS destination_price,
                        d.mail AS destination_mail,

                        i_d.ImageId AS destination_img_id, i_d.ImgUrl AS destination_img_url, i_d.IsPrimary AS destination_img_primary,

                        sv.name AS service_name, sv.description AS service_desc, sv.location AS service_location,
                        sv.open_time AS service_open_time, sv.close_time AS service_close_time, sv.email AS service_email,
                        sv.website AS service_website, sv.phone AS service_phone, sv.main_service,

                        i_sv.ImageId AS service_img_id, i_sv.ImgUrl AS service_img_url, i_sv.IsPrimary AS service_img_primary

                    FROM schedule s
                    LEFT JOIN schedule_items si ON si.schedule_id = s.id
                    LEFT JOIN destination d ON si.entity_type = 'destination' AND si.entity_id = d.id
                    LEFT JOIN img i_d ON i_d.EntityType = 'Destination' AND i_d.EntityId = d.id

                    LEFT JOIN service sv ON (si.entity_type IN ('service', 'muasam', 'vanchuyen') AND si.entity_id = sv.id)
                    LEFT JOIN img i_sv ON i_sv.EntityType = 'Service' AND i_sv.EntityId = sv.id

                    WHERE s.user_id = @userId
                    ORDER BY s.id, si.day_order";

                using MySqlCommand command = new(sql, connection);
                command.Parameters.AddWithValue("@userId", userId);
                using MySqlDataReader reader = command.ExecuteReader();

                var schedules = new List<Schedule>();
                Schedule? currentSchedule = null;
                int lastScheduleId = -1;

                while (reader.Read())
                {
                    int scheduleId = reader.GetInt32(0);

                    if (scheduleId != lastScheduleId)
                    {
                        currentSchedule = new Schedule
                        {
                            Id = scheduleId,
                            UserId = reader.GetInt32(1),
                            Name = reader.GetString(2),
                            StartDate = reader.GetDateTime(3),
                            EndDate = reader.GetDateTime(4),
                            Description = reader.GetString(5),
                            CreatedAt = reader.GetDateTime(6),
                            ScheduleItems = new List<ScheduleItem>()
                        };
                        schedules.Add(currentSchedule);
                        lastScheduleId = scheduleId;
                    }

                    if (!reader.IsDBNull(7))
                    {
                        string entityType = reader.GetString(8);
                        int entityId = reader.GetInt32(9);
                        int dayOrder = reader.GetInt32(10);

                        var item = new ScheduleItem
                        {
                            Id = reader.GetInt32(7),
                            EntityType = entityType,
                            EntityId = entityId,
                            DayOrder = dayOrder
                        };

                        if (entityType == "destination" && !reader.IsDBNull(11))
                        {
                            var destination = new Destination
                            {
                                Id = entityId,
                                Name = reader.GetString(11),
                                Description = reader.GetString(12),
                                Location = reader.GetString(13),
                                OpenTime = reader.GetDateTime(14),
                                CloseTime = reader.GetDateTime(15),
                                Price = reader.GetDecimal(16),
                                Mail = reader.GetString(17),
                                Images = new List<Img>()
                            };

                            if (!reader.IsDBNull(18))
                            {
                                var img = new Img
                                {
                                    ImageId = reader.GetInt32(18),
                                    ImgUrl = reader.GetString(19),
                                    IsPrimary = reader.GetBoolean(20),
                                    EntityType = "Destination",
                                    EntityId = destination.Id
                                };
                                destination.Images.Add(img);
                            }

                            item.Destination = destination;
                        }
                        else if ((entityType == "muasam" || entityType == "vanchuyen" || entityType == "service") && !reader.IsDBNull(21))
                        {
                            var service = new Service
                            {
                                Id = entityId,
                                Name = reader.GetString(21),
                                Description = reader.GetString(22),
                                Location = reader.GetString(23),
                                OpenTime = reader.GetDateTime(24),
                                CloseTime = reader.GetDateTime(25),
                                Email = reader.GetString(26),
                                Website = reader.GetString(27),
                                Phone = reader.GetString(28),
                                MainService = reader.GetString(29),
                                Images = new List<Img>()
                            };

                            if (!reader.IsDBNull(30))
                            {
                                var img = new Img
                                {
                                    ImageId = reader.GetInt32(30),
                                    ImgUrl = reader.GetString(31),
                                    IsPrimary = reader.GetBoolean(32),
                                    EntityType = "Service",
                                    EntityId = service.Id
                                };
                                service.Images.Add(img);
                            }

                            item.Service = service;
                        }

                        currentSchedule?.ScheduleItems.Add(item);
                    }
                }

                return Ok(schedules);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi truy vấn chi tiết: {ex.Message}");
            }
        }

        [HttpGet("user/{userId}")]
        public IActionResult GetByUserId(int userId)
        {
            try
            {
                using MySqlConnection connection = new(_connectionString);
                connection.Open();

                string sql = @"
                    SELECT
                        s.id, s.user_id, s.name, s.start_date, s.end_date, s.description, s.created_at,
                        si.id AS item_id, si.schedule_id, si.entity_type, si.entity_id, si.day_order
                    FROM schedule s
                    LEFT JOIN schedule_items si ON si.schedule_id = s.id
                    WHERE s.user_id = @userId
                    ORDER BY s.id, si.day_order";

                using MySqlCommand command = new(sql, connection);
                command.Parameters.AddWithValue("@userId", userId);
                using MySqlDataReader reader = command.ExecuteReader();

                var schedules = new List<Schedule>();
                Schedule? currentSchedule = null;
                int lastScheduleId = -1;

                while (reader.Read())
                {
                    int scheduleId = reader.GetInt32(0);

                    if (scheduleId != lastScheduleId)
                    {
                        currentSchedule = new Schedule
                        {
                            Id = scheduleId,
                            UserId = reader.GetInt32(1),
                            Name = reader.GetString(2),
                            StartDate = reader.GetDateTime(3),
                            EndDate = reader.GetDateTime(4),
                            Description = reader.GetString(5),
                            CreatedAt = reader.GetDateTime(6),
                            ScheduleItems = new List<ScheduleItem>()
                        };
                        schedules.Add(currentSchedule);
                        lastScheduleId = scheduleId;
                    }

                    if (!reader.IsDBNull(7))
                    {
                        var item = new ScheduleItem
                        {
                            Id = reader.GetInt32(7),
                            ScheduleId = reader.GetInt32(8),
                            EntityType = reader.GetString(9),
                            EntityId = reader.GetInt32(10),
                            DayOrder = reader.GetInt32(11)
                        };
                        currentSchedule?.ScheduleItems.Add(item);
                    }
                }

                return Ok(schedules);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi truy vấn: {ex.Message}");
            }
        }
    }
}