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
var configuration = builder.Configuration;

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

// ✅ استفاده از overload جدید برای جلوگیری از هشدار deprecation
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(opt =>
    {
        opt.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection"));
    }));
builder.Services.AddHangfireServer();

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

builder.Services.AddScoped<InstagramService>();
builder.Services.AddScoped<ScraperService>();

var app = builder.Build();

// داشبورد Hangfire
app.UseHangfireDashboard();

// 📌 اگر دیتابیس خالی بود، یک بار Scraper اجرا کن
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var scraper = scope.ServiceProvider.GetRequiredService<ScraperService>();

    if (!db.TargetUsers.Any())
    {
        Console.WriteLine("🔍 دیتابیس خالی است. اجرای Scraper برای پر کردن لیست...");
        await scraper.ScrapeFollowers("baboone_.store");
    }
    else
    {
        Console.WriteLine("✅ دیتابیس از قبل دارای اطلاعات است.");
    }
}

// 📅 برنامه‌ریزی روزانه ساعت ۹ صبح برای Scraper
RecurringJob.AddOrUpdate<ScraperService>(
    "scrape-followers-daily",
    x => x.ScrapeFollowers("baboone_.store"),
    "0 9 * * *"
);

// ✉️ ارسال پیام هر ۳۰ دقیقه
RecurringJob.AddOrUpdate<InstagramService>(
    "send-messages-every-30-minutes",
    x => x.SendMessagesToUnmessagedUsersAsync(1),
    "*/30 * * * *"
);

app.Run();
