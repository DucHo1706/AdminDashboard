using AdminDashboard.Services;
using AdminDashboard.TransportDBContext;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using CloudinaryDotNet;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký DbContext
builder.Services.AddDbContext<Db27524Context>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    )
);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ĐĂNG KÝ CLOUDINARY SERVICE - ƯU TIÊN DÙNG CLOUDINARY
var cloudinaryConfig = builder.Configuration.GetSection("Cloudinary");
var cloudName = cloudinaryConfig["CloudName"];
var apiKey = cloudinaryConfig["ApiKey"];
var apiSecret = cloudinaryConfig["ApiSecret"];

if (!string.IsNullOrEmpty(cloudName) &&
    !string.IsNullOrEmpty(apiKey) &&
    !string.IsNullOrEmpty(apiSecret))
{
    // SỬ DỤNG CLOUDINARY - TỐT NHẤT CHO PRODUCTION
    var account = new Account(cloudName, apiKey, apiSecret);
    var cloudinary = new Cloudinary(account);

    builder.Services.AddSingleton(cloudinary);
    builder.Services.AddScoped<IImageService, CloudinaryImageService>();

    Console.WriteLine($" Đã đăng ký Cloudinary service: {cloudName}");
}
else
{
    // FALLBACK: Dùng LocalImageService nếu không có cấu hình Cloudinary
    builder.Services.AddScoped<IImageService, LocalImageService>();
    Console.WriteLine(" Đang dùng LocalImageService - Chỉ nên dùng cho development");
}

builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

builder.Services.AddScoped<IVnpayService, VnpayService>();
builder.Services.AddHostedService<AdminDashboard.Services.TrangThaiChuyenXeService>();
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
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();