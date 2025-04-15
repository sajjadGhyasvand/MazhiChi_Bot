using Hangfire;
using Hangfire.PostgreSql;
using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramBot.Data;
using InstagramBot.Services;
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

RecurringJob.AddOrUpdate<InstagramService>(
    "send-messages-every-30-minutes",
    x => x.SendMessagesToUnmessagedUsersAsync(1), // فقط یک پیام در هر ۳۰ دقیقه
    "*/30 * * * *"
);

app.Run();
