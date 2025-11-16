using AdminDashboard.TransportDBContext;
using AdminDashboard.Services;
using AdminDashboard.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using CloudinaryDotNet;

var builder = WebApplication.CreateBuilder(args);

// ===================== ĐĂNG KÝ DATABASE =====================
builder.Services.AddDbContext<Db27524Context>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    )
);

// ===================== ĐĂNG KÝ MVC, RAZOR, SIGNALR =====================
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// ===================== ĐĂNG KÝ SERVICE =====================
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IVnpayService, VnpayService>();
builder.Services.AddScoped<IPaginationService, PaginationService>();
builder.Services.AddHttpClient();

// ===================== CLOUDINARY / LOCAL IMAGE SERVICE =====================
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
    Console.WriteLine($"✅ Đã đăng ký Cloudinary service: {cloudName}");
}
else
{
    builder.Services.AddScoped<IImageService, LocalImageService>();
    Console.WriteLine("⚠️ Đang dùng LocalImageService - chỉ nên dùng cho development");
}

// ===================== COOKIE AUTHENTICATION =====================
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

// ===================== XÂY DỰNG APP =====================
var app = builder.Build();

// ===================== MIDDLEWARE PIPELINE =====================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ⚠️ Authentication phải trước Authorization
app.UseAuthentication();
app.UseAuthorization();

// ===================== MAP HUB & ROUTES =====================
app.MapHub<ChatHub>("/chathub"); // ChatHub realtime

app.MapRazorPages();

app.MapControllerRoute(
    name: "Areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home_User}/{action=Home_User}/{id?}"
);

app.MapControllerRoute(
    name: "chat_user",
    pattern: "{controller=ChatUser}/{action=Index}/{id?}"
);


app.Run();


