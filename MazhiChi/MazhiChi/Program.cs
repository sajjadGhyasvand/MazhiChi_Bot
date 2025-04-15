using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.PostgreSql;
using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;
using MazhiChi.Data;
using MazhiChi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// چاپ Connection String و Base Directory جهت اطمینان از مقداردهی درست
Console.WriteLine("🔌 Connection String: " + configuration.GetConnectionString("DefaultConnection"));
Console.WriteLine("📁 Base Directory: " + Directory.GetCurrentDirectory());

// تنظیم DbContext با استفاده از PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

// استفاده از overload جدید برای Hangfire
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(opt =>
    {
        opt.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection"));
    }));
builder.Services.AddHangfireServer();

// خواندن اطلاعات ورود اینستاگرام از فایل تنظیمات
var instaUsername = configuration["InstagramSettings:Username"];
var instaPassword = configuration["InstagramSettings:Password"];

// ثبت IInstaApi به صورت Singleton و انجام ورود به صورت همزمان (synchronously)
builder.Services.AddSingleton<IInstaApi>(sp =>
{
    var api = InstaApiBuilder.CreateBuilder()
                .SetUser(new UserSessionData
                {
                    UserName = instaUsername,
                    Password = instaPassword
                })
                .UseLogger(new DebugLogger(LogLevel.Exceptions))
                .Build();

    // انجام ورود به اینستاگرام به صورت همزمان
    var loginResult = api.LoginAsync().GetAwaiter().GetResult();
    if (!loginResult.Succeeded)
    {
        throw new Exception("Failed to login to Instagram: " + loginResult.Info.Message);
    }
    Console.WriteLine("Instagram login successful.");
    return api;
});

// ثبت سرویس‌ها به صورت Scoped
builder.Services.AddScoped<InstagramService>();
builder.Services.AddScoped<ScraperService>();

var app = builder.Build();

// داشبورد Hangfire
app.UseHangfireDashboard();

// بررسی دیتابیس و انجام Scrape اولیه در صورت خالی بودن
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var scraper = scope.ServiceProvider.GetRequiredService<ScraperService>();

    if (!db.TargetUsers.Any())
    {
        Console.WriteLine("🔍 DataBase is Empty");
        await scraper.ScrapeFollowers("ranginkamon");
    }
    else
    {
        Console.WriteLine("✅ DataBase Has Data");
    }
}

// تنظیم Jobهای Hangfire
RecurringJob.AddOrUpdate<ScraperService>(
    "scrape-followers-daily",
    x => x.ScrapeFollowers("ranginkamon"),
    "0 9 * * *"    // هر روز ساعت 9 صبح اجرا شود
);

RecurringJob.AddOrUpdate<InstagramService>(
    "send-messages-every-30-minutes",
    x => x.SendMessagesToUnmessagedUsersAsync(1),  // فقط یک پیام در هر 30 دقیقه
    "*/30 * * * *"
);

app.Run();
