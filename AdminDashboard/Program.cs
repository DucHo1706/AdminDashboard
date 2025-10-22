using AdminDashboard.TransportDBContext;
using AdminDashboard.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký DbContext TRƯỚC khi build
builder.Services.AddDbContext<Db27524Context>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure() // Thêm retry
    )
);

// Add services to the container
builder.Services.AddControllersWithViews();

// Đăng ký các dịch vụ
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOtpService, OtpService>();

// 🔑 Thêm Authentication & Cookie
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Auth/Login";        // Trang login
        options.LogoutPath = "/Auth/Logout";      // Trang logout
        options.AccessDeniedPath = "/Auth/AccessDenied"; // Khi bị từ chối
        options.ExpireTimeSpan = TimeSpan.FromHours(2);  // Cookie sống 2h
    });

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//  Bắt buộc: Authentication phải trước Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


/*  Giải thích

builder.Services.AddAuthentication(...).AddCookie(...) → đăng ký Cookie Authentication.

app.UseAuthentication();  middleware kiểm tra cookie, bắt buộc trước UseAuthorization().

Sau khi login, ASP.NET Core sẽ tự tạo cookie lưu session đăng nhập. 
*/