using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using duLichQuangNam.Models;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace duLichQuangNam.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public UsersController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _configuration = configuration;
        }

        // POST: /Users/register
        [HttpPost("register")]
        public IActionResult Register([FromBody] Users newUser)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string checkUsernameSql = "SELECT COUNT(*) FROM users WHERE username = @username";
                using SqlCommand checkUsernameCommand = new(checkUsernameSql, connection);
                checkUsernameCommand.Parameters.AddWithValue("@username", newUser.UserName);
                int usernameExists = (int)checkUsernameCommand.ExecuteScalar();

                if (usernameExists > 0)
                {
                    return BadRequest(new { message = "Username đã tồn tại" });
                }

                string checkMailSql = "SELECT COUNT(*) FROM users WHERE mail = @mail";
                using SqlCommand checkMailCommand = new(checkMailSql, connection);
                checkMailCommand.Parameters.AddWithValue("@mail", newUser.Mail);
                int mailExists = (int)checkMailCommand.ExecuteScalar();

                if (mailExists > 0)
                {
                    return BadRequest(new { message = "Mail đã tồn tại" });
                }

                newUser.Password = HashPassword(newUser.Password);

                string insertSql = @"
                    INSERT INTO users (name, mail, password, phone, role, age, gender, deleted, address, username)
                    VALUES (@name, @mail, @password, @phone, @role, @age, @gender, 0, @address, @username)";
                using SqlCommand command = new(insertSql, connection);
                command.Parameters.AddWithValue("@name", newUser.Name);
                command.Parameters.AddWithValue("@mail", newUser.Mail);
                command.Parameters.AddWithValue("@password", newUser.Password);
                command.Parameters.AddWithValue("@phone", newUser.Phone);
                command.Parameters.AddWithValue("@role", newUser.Role);
                command.Parameters.AddWithValue("@age", newUser.Age);
                command.Parameters.AddWithValue("@gender", newUser.Gender);
                command.Parameters.AddWithValue("@address", newUser.Address);
                command.Parameters.AddWithValue("@username", newUser.UserName);

                command.ExecuteNonQuery();
                return Ok(new { message = "Đăng ký người dùng thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi truy vấn: {ex.Message}");
            }
        }
        // PUT: /api/users/update/{id}
        [HttpPut("update/{id:int}")]
        public IActionResult UpdateUser(int id, [FromBody] Users updatedUser)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                // Kiểm tra xem user có tồn tại và chưa bị xóa
                string checkSql = "SELECT COUNT(*) FROM users WHERE id = @id AND deleted = 0";
                using SqlCommand checkCmd = new(checkSql, connection);
                checkCmd.Parameters.AddWithValue("@id", id);
                int userExists = (int)checkCmd.ExecuteScalar();

                if (userExists == 0)
                {
                    return NotFound(new { message = "Người dùng không tồn tại hoặc đã bị xóa." });
                }

                // Kiểm tra username có bị trùng với user khác không (nếu username được phép sửa)
                string checkUsernameSql = "SELECT COUNT(*) FROM users WHERE username = @username AND id <> @id";
                using SqlCommand checkUsernameCmd = new(checkUsernameSql, connection);
                checkUsernameCmd.Parameters.AddWithValue("@username", updatedUser.UserName);
                checkUsernameCmd.Parameters.AddWithValue("@id", id);
                int usernameExists = (int)checkUsernameCmd.ExecuteScalar();

                if (usernameExists > 0)
                {
                    return BadRequest(new { message = "Username đã tồn tại." });
                }

                // Kiểm tra mail có bị trùng với user khác không (nếu mail được phép sửa)
                string checkMailSql = "SELECT COUNT(*) FROM users WHERE mail = @mail AND id <> @id";
                using SqlCommand checkMailCmd = new(checkMailSql, connection);
                checkMailCmd.Parameters.AddWithValue("@mail", updatedUser.Mail);
                checkMailCmd.Parameters.AddWithValue("@id", id);
                int mailExists = (int)checkMailCmd.ExecuteScalar();

                if (mailExists > 0)
                {
                    return BadRequest(new { message = "Mail đã tồn tại." });
                }

                // Câu lệnh UPDATE
                string updateSql = @"
            UPDATE users SET
                name = @name,
                mail = @mail,
                phone = @phone,
                age = @age,
                gender = @gender,
                address = @address,
                username = @username,
                role = @role
            WHERE id = @id";

                using SqlCommand updateCmd = new(updateSql, connection);
                updateCmd.Parameters.AddWithValue("@name", updatedUser.Name);
                updateCmd.Parameters.AddWithValue("@mail", updatedUser.Mail);
                updateCmd.Parameters.AddWithValue("@phone", updatedUser.Phone);
                updateCmd.Parameters.AddWithValue("@age", updatedUser.Age);
                updateCmd.Parameters.AddWithValue("@gender", updatedUser.Gender);
                updateCmd.Parameters.AddWithValue("@address", updatedUser.Address);
                updateCmd.Parameters.AddWithValue("@username", updatedUser.UserName);
                updateCmd.Parameters.AddWithValue("@role", updatedUser.Role);
                updateCmd.Parameters.AddWithValue("@id", id);

                int rowsAffected = updateCmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    return Ok(new { message = "Cập nhật thông tin người dùng thành công." });
                }
                else
                {
                    return StatusCode(500, new { message = "Cập nhật thông tin thất bại." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi truy vấn: {ex.Message}");
            }
        }

        // POST: /Users/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] UsersLoginViewModel login)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string checkUserSql = "SELECT * FROM users WHERE username = @username";
                using SqlCommand checkUserCommand = new(checkUserSql, connection);
                checkUserCommand.Parameters.AddWithValue("@username", login.UserName);
                using SqlDataReader reader = checkUserCommand.ExecuteReader();

                if (!reader.Read())
                {
                    return Unauthorized(new { message = "Username không tồn tại" });
                }

                var storedPassword = reader["password"].ToString()!;

                if (VerifyPassword(login.Password, storedPassword))
                {
                    var user = new Users
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Name = reader["name"].ToString() ?? string.Empty,
                        Mail = reader["mail"].ToString() ?? string.Empty,
                        Role = reader["role"].ToString() ?? string.Empty,
                        Phone = reader["phone"].ToString() ?? string.Empty,
                        Address = reader["address"].ToString() ?? string.Empty,
                        Age = Convert.ToInt32(reader["age"]),
                        Gender = reader["gender"].ToString() ?? string.Empty,
                        Deleted = Convert.ToBoolean(reader["deleted"]),
                        UserName = reader["username"].ToString() ?? string.Empty
                    };

                    var token = GenerateJwtToken(user);
                    return Ok(new { token, id = user.Id, role = user.Role });
                }

                return Unauthorized(new { message = "Mật khẩu không đúng" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi truy vấn: {ex.Message}");
            }
        }
        // GET: /Users/{id}
        [HttpGet("{id:int}")]
        public IActionResult GetUserById(int id)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string sql = "SELECT id, name, mail, phone, role, age, gender, address, username, deleted " +
                             "FROM users WHERE id = @id AND deleted = 0";
                using SqlCommand cmd = new(sql, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using SqlDataReader reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    return NotFound(new { message = "Không tìm thấy người dùng" });
                }

                var user = new Users
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Name = reader["name"]?.ToString() ?? string.Empty,
                    Mail = reader["mail"]?.ToString() ?? string.Empty,
                    Phone = reader["phone"]?.ToString() ?? string.Empty,
                    Role = reader["role"]?.ToString() ?? string.Empty,
                    Age = reader.GetInt32(reader.GetOrdinal("age")),
                    Gender = reader["gender"]?.ToString() ?? string.Empty,
                    Address = reader["address"]?.ToString() ?? string.Empty,
                    UserName = reader["username"]?.ToString() ?? string.Empty,
                    Deleted = reader.GetBoolean(reader.GetOrdinal("deleted"))
                };

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi truy vấn: {ex.Message}");
            }
        }
        // POST: api/users/delete/{id}
        [HttpPost("delete/{id}")]
        public IActionResult SoftDeleteUser(int id)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string sql = "UPDATE users SET deleted = 1 WHERE id = @id";

                using SqlCommand command = new(sql, connection);
                command.Parameters.AddWithValue("@id", id);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    return NotFound($"Không tìm thấy người dùng với ID = {id}");
                }

                return Ok($"Người dùng với ID = {id} đã được đánh dấu là đã xóa.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi xóa người dùng: {ex.Message}");
            }
        }

        // GET: /Users
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string sql = "SELECT id, name, mail, phone, role, age, gender, address, username, deleted FROM users WHERE deleted = 0";
                using SqlCommand cmd = new(sql, connection);

                using SqlDataReader reader = cmd.ExecuteReader();

                var users = new List<Users>();

                while (reader.Read())
                {
                    var user = new Users
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        Name = reader["name"]?.ToString() ?? string.Empty,
                        Mail = reader["mail"]?.ToString() ?? string.Empty,
                        Phone = reader["phone"]?.ToString() ?? string.Empty,
                        Role = reader["role"]?.ToString() ?? string.Empty,
                        Age = reader.GetInt32(reader.GetOrdinal("age")),
                        Gender = reader["gender"]?.ToString() ?? string.Empty,
                        Address = reader["address"]?.ToString() ?? string.Empty,
                        UserName = reader["username"]?.ToString() ?? string.Empty,
                        Deleted = reader.GetBoolean(reader.GetOrdinal("deleted"))
                    };
                    users.Add(user);
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi truy vấn: {ex.Message}");
            }
        }


        // POST: /Users/change-password
        [HttpPost("ChangePassword")]
        public IActionResult ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

               
                string selectSql = "SELECT password FROM users WHERE username = @username";
                using SqlCommand selectCommand = new(selectSql, connection);
                selectCommand.Parameters.AddWithValue("@username", model.UserName);

                var storedPasswordObj = selectCommand.ExecuteScalar();
                if (storedPasswordObj == null)
                {
                    return NotFound(new { message = "Username không tồn tại" });
                }

                string storedPassword = storedPasswordObj.ToString()!;

                
                if (!VerifyPassword(model.OldPassword, storedPassword))
                {
                    return Unauthorized(new { message = "Mật khẩu cũ không đúng" });
                }

                
                string newHashedPassword = HashPassword(model.NewPassword);

              
                string updateSql = "UPDATE users SET password = @newPassword WHERE username = @username";
                using SqlCommand updateCommand = new(updateSql, connection);
                updateCommand.Parameters.AddWithValue("@newPassword", newHashedPassword);
                updateCommand.Parameters.AddWithValue("@username", model.UserName);

                int rowsAffected = updateCommand.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    return Ok(new { message = "Đổi mật khẩu thành công" });
                }
                else
                {
                    return StatusCode(500, new { message = "Cập nhật mật khẩu thất bại" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi truy vấn: {ex.Message}");
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string inputPassword, string storedPassword)
        {
            var hashedInputPassword = HashPassword(inputPassword);
            return hashedInputPassword == storedPassword;
        }

        private string GenerateJwtToken(Users user)
        {
            var secretKey = _configuration["Jwt:SecretKey"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("name", user.Name),
                new Claim("role", user.Role),
                new Claim("username", user.UserName)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }


}
