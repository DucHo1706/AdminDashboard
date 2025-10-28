<<<<<<< HEAD
using AdminDashboard.TransportDBContext;
using AdminDashboard.Services;
using AdminDashboard.Hubs;
using CloudinaryDotNet;
=======
﻿using AdminDashboard.TransportDBContext;
using AdminDashboard.Services;
using Microsoft.AspNetCore.Http;
>>>>>>> master
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

<<<<<<< HEAD
// DbContext
builder.Services.AddDbContext<Db27524Context>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    )
);

// MVC, RazorPages, SignalR
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IVnpayService, VnpayService>();

// Cloudinary / LocalImageService
var cloudinaryConfig = builder.Configuration.GetSection("Cloudinary");
var cloudName = cloudinaryConfig["CloudName"];
var apiKey = cloudinaryConfig["ApiKey"];
var apiSecret = cloudinaryConfig["ApiSecret"];

if (!string.IsNullOrEmpty(cloudName) &&
    !string.IsNullOrEmpty(apiKey) &&
    !string.IsNullOrEmpty(apiSecret))
{
    var account = new Account(cloudName, apiKey, apiSecret);
    var cloudinary = new Cloudinary(account);
    builder.Services.AddSingleton(cloudinary);
    builder.Services.AddScoped<IImageService, CloudinaryImageService>();
}
else
{
    builder.Services.AddScoped<IImageService, LocalImageService>();
}

// Authentication & Cookie
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
=======
// Đăng ký DbContext TRƯỚC khi build
builder.Services.AddDbContext<Db27524Context>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure() // Thêm retry
    )
);

// Add services to the container
builder.Services.AddControllersWithViews();

// Đăng ký EmailService
builder.Services.AddScoped<IEmailService, EmailService>();

// 🔑 Thêm Authentication & Cookie
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Auth/Login";        // Trang login
        options.LogoutPath = "/Auth/Logout";      // Trang logout
        options.AccessDeniedPath = "/Auth/AccessDenied"; // Khi bị từ chối
        options.ExpireTimeSpan = TimeSpan.FromHours(2);  // Cookie sống 2h
>>>>>>> master
    });

var app = builder.Build();

<<<<<<< HEAD
// Pipeline
=======
// Configure the HTTP request pipeline
>>>>>>> master
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
<<<<<<< HEAD
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Routes & SignalR
app.MapHub<ChatHub>("/chathub");
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Chat}/{action=Index}/{id?}"
);

app.Run();
=======

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
>>>>>>> master
