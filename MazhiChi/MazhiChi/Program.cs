using MazhiChi.Scheduler;
using MazhiChi.Services;

var username = "یوزرنیم اینستا";
var password = "پسورد اینستا";
var targetUsername = "پیج هدف";

var instaService = new InstagramService(username, password);
var success = await instaService.LoginAsync();

if (!success)
{
    Console.WriteLine("Login Failed");
    return;
}

var scheduler = new FollowScheduler(instaService);
await scheduler.ScrapeAndFollowAsync(targetUsername);

Console.WriteLine("Done!");
