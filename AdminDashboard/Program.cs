using AdminDashboard.TransportDBContext;
using AdminDashboard.Services;
using AdminDashboard.Hubs;
using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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
    });

var app = builder.Build();

// Pipeline
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

// Routes & SignalR
app.MapHub<ChatHub>("/chathub");
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Chat}/{action=Index}/{id?}"
);

app.Run();
