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

// تنظیمات DB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Hangfire
builder.Services.AddHangfire(config => config.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

// InstagramApiSharp باید اینجا تنظیم بشه
builder.Services.AddSingleton<IInstaApi>(sp =>
{
    var api = InstaApiBuilder.CreateBuilder().SetUser(new UserSessionData
    {
        UserName = "mazhi_chi",
        Password = "148635"
    }).Build();
    return api;
});

// سرویس‌ها
builder.Services.AddScoped<InstagramService>();
builder.Services.AddScoped<ScraperService>();

var app = builder.Build();

app.UseHangfireDashboard();

RecurringJob.AddOrUpdate<InstagramService>(
    "send-daily-messages",
    x => x.SendMessagesToUnmessagedUsersAsync(30),
    "*/10 * * * *"
);

app.Run();
