//using Microsoft.AspNetCore.Authentication.Cookies;
//using DotNetEnv;



//Env.Load();
//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllers();
//builder.Services.AddRazorPages();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
//builder.Services.AddHttpClient();
//builder.Services.AddHttpContextAccessor();
//builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.LoginPath = "/Login";
//        options.AccessDeniedPath = "/Error";
//        options.ExpireTimeSpan = TimeSpan.FromHours(2);
//    });

//builder.Services.AddAuthorization();

//var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseStatusCodePagesWithReExecute("/404");

//app.UseStaticFiles();
//app.UseRouting();

//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllers();
//app.MapRazorPages();

//app.Run();

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer; // Thêm namespace này cho JWT
using Microsoft.IdentityModel.Tokens; // Thêm namespace này cho SymmetricSecurityKey
using System.Text; // Thêm namespace này cho Encoding
using DotNetEnv; // Thư viện để tải biến môi trường từ file .env
using Microsoft.Extensions.Configuration; // Đảm bảo namespace này có sẵn

// Tải các biến môi trường từ file .env (nếu có)
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Thêm các dịch vụ vào container.
// Cấu hình Controllers để xử lý các API và Razor Pages để xử lý các trang web
builder.Services.AddControllers();
builder.Services.AddRazorPages();

// Thêm dịch vụ để khám phá các endpoint API (sử dụng cho Swagger/OpenAPI)
builder.Services.AddEndpointsApiExplorer();
// Thêm dịch vụ Swagger để tạo tài liệu API tự động
builder.Services.AddSwaggerGen();

// Thêm dịch vụ HttpClient để thực hiện các yêu cầu HTTP đi
builder.Services.AddHttpClient();
// Thêm dịch vụ HttpContextAccessor để truy cập HttpContext từ bất cứ đâu trong ứng dụng
builder.Services.AddHttpContextAccessor();
// Thêm IConfiguration như một Singleton để có thể inject cấu hình vào các dịch vụ khác
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Cấu hình Cookie Authentication (thường dùng cho các ứng dụng web truyền thống với UI)
// Đây là scheme xác thực mặc định nếu không có scheme nào khác được chỉ định rõ ràng cho một endpoint.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Đường dẫn chuyển hướng khi người dùng cần đăng nhập
        options.LoginPath = "/Login";
        // Đường dẫn chuyển hướng khi người dùng không có quyền truy cập
        options.AccessDeniedPath = "/Error";
        // Thời gian hết hạn của cookie xác thực
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

// Cấu hình JWT Bearer Authentication (thường dùng cho các API)
// Lưu ý: Nếu bạn muốn JWT là scheme mặc định cho API, bạn có thể thay đổi dòng trên
// hoặc chỉ định scheme này bằng [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] trên các controller/action.
builder.Services.AddAuthentication() // Gọi AddAuthentication mà không truyền scheme mặc định để có thể thêm nhiều scheme
    .AddJwtBearer(options =>
    {
        // Các tham số để xác thực token JWT
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // Xác thực người phát hành token
            ValidateAudience = true, // Xác thực đối tượng của token
            ValidateLifetime = true, // Xác thực thời gian sống của token (hết hạn)
            ValidateIssuerSigningKey = true, // Xác thực khóa ký của người phát hành

            // Lấy giá trị từ biến môi trường
            ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
            ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            // Khóa bí mật để giải mã và xác thực chữ ký token
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET_KEY")!))
        };
    });

// Thêm dịch vụ ủy quyền (authorization)
builder.Services.AddAuthorization();

var app = builder.Build();

// Cấu hình HTTP request pipeline.

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
