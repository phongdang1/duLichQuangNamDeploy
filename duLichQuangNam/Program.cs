
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv;
using Microsoft.Extensions.Configuration;

// Tải các biến môi trường từ file .env (nếu có)
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// --- Cấu hình dịch vụ ---
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Cấu hình Cookie Authentication (thường dùng cho các ứng dụng web truyền thống với UI)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Error";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

// Lấy các biến môi trường cho JWT
var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

// Kiểm tra và đảm bảo các biến môi trường JWT đã được thiết lập
if (string.IsNullOrEmpty(jwtSecretKey))
{
    throw new InvalidOperationException("Biến môi trường 'JWT_SECRET_KEY' chưa được thiết lập.");
}
if (string.IsNullOrEmpty(jwtIssuer))
{
    throw new InvalidOperationException("Biến môi trường 'JWT_ISSUER' chưa được thiết lập.");
}
if (string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("Biến môi trường 'JWT_AUDIENCE' chưa được thiết lập.");
}

// Cấu hình JWT Bearer Authentication (thường dùng cho các API)
builder.Services.AddAuthentication() // Gọi AddAuthentication() để có thể thêm nhiều scheme
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
        };
    });

// Thêm dịch vụ ủy quyền (authorization)
builder.Services.AddAuthorization();

var app = builder.Build();

// --- Cấu hình HTTP request pipeline ---

// Trong môi trường phát triển, sử dụng Swagger để tài liệu hóa và kiểm thử API
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Chuyển hướng các mã trạng thái lỗi (ví dụ 404) đến một đường dẫn cụ thể
app.UseStatusCodePagesWithReExecute("/404");

// Cho phép phục vụ các file tĩnh (ví dụ: HTML, CSS, JavaScript, hình ảnh)
app.UseStaticFiles();

// Kích hoạt định tuyến (routing) để ánh xạ các yêu cầu HTTP đến các endpoint
app.UseRouting();

// Kích hoạt Authentication middleware.
// Middleware này phải được gọi trước Authorization middleware.
// Nó sẽ đọc thông tin xác thực từ request (ví dụ: cookie, header Authorization).
app.UseAuthentication();

// Kích hoạt Authorization middleware.
// Middleware này phải được gọi sau Authentication middleware.
// Nó sẽ sử dụng thông tin xác thực đã đọc để kiểm tra quyền truy cập.
app.UseAuthorization();

// Ánh xạ các controller API
app.MapControllers();
// Ánh xạ các Razor Pages
app.MapRazorPages();

// Chạy ứng dụng
app.Run();