using Hangfire;
using Hangfire.PostgreSql;
using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using MazhiChi.Data;
using MazhiChi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// 🔧 خواندن تنظیمات از appsettings.json
var configuration = builder.Configuration;

// تنظیمات DB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

// Hangfire
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

// تنظیم IInstaApi با خواندن مشخصات از appsettings
var instaUsername = configuration["InstagramSettings:Username"];
var instaPassword = configuration["InstagramSettings:Password"];

builder.Services.AddSingleton<IInstaApi>(sp =>
{
    var api = InstaApiBuilder.CreateBuilder().SetUser(new UserSessionData
    {
        UserName = instaUsername,
        Password = instaPassword
    }).Build();
    return api;
});

// سرویس‌ها
builder.Services.AddScoped<InstagramService>();
builder.Services.AddScoped<ScraperService>();

var app = builder.Build();

// داشبورد Hangfire
app.UseHangfireDashboard();

// 🔁 اجرای Scraper هر 30 دقیقه برای آپدیت فالورها
RecurringJob.AddOrUpdate<ScraperService>(
    "scrape-followers-daily",
    x => x.ScrapeFollowers("baboone_.store"), // نام پیج موردنظر رو اینجا بذار
    "0 9 * * *"
);

// 🔁 اجرای ارسال پیام هر ۳۰ دقیقه
RecurringJob.AddOrUpdate<InstagramService>(
    "send-messages-every-30-minutes",
    x => x.SendMessagesToUnmessagedUsersAsync(1), // فقط یک پیام در هر ۳۰ دقیقه
    "*/30 * * * *"
);

// بررسی دیتابیس و اجرای Scrape در صورت نیاز
await RunInitialScrapeIfNeeded(app);

// اجرای اپلیکیشن
app.Run();

// متد بررسی و اسکرپ اولیه
static async Task RunInitialScrapeIfNeeded(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var scraperService = scope.ServiceProvider.GetRequiredService<ScraperService>();

    // بررسی اگر دیتابیس خالی بود
    if (!dbContext.TargetUsers.Any())
    {
        Console.WriteLine("📥 دیتابیس خالیه، در حال اسکرپ اولیه...");
        await scraperService.ScrapeFollowers("baboone_.store");
    }
    else
    {
        Console.WriteLine("✅ دیتابیس از قبل پر شده، Scrape فقط طبق برنامه زمان‌بندی انجام میشه.");
    }
}
