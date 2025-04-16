using InstagramApiSharp.API;
using InstagramApiSharp.Classes;
using MazhiChi.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MazhiChi.Services
{
    public class InstagramService
    {
        private readonly IInstaApi _instaApi;
        private readonly ApplicationDbContext _dbContext;
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public InstagramService(IInstaApi instaApi, ApplicationDbContext dbContext)
        {
            _instaApi = instaApi;
            _dbContext = dbContext;
        }

        public async Task SendMessagesToUnmessagedUsersAsync()
        {
            if (!await _lock.WaitAsync(0))
            {
                Console.WriteLine("⚠️ Job already running. Skipping...");
                return;
            }

            try
            {
                var now = DateTime.UtcNow;
                var iranStart = new TimeSpan(4, 30, 0);  // 08:00 IR
                var iranEnd = new TimeSpan(20, 30, 0);   // 00:00 IR

                if (now.TimeOfDay < iranStart || now.TimeOfDay > iranEnd)
                {
                    Console.WriteLine("⏸️ Out of messaging hours.");
                    return;
                }

                var user = await _dbContext.TargetUsers
                    .Where(u => !u.IsMessaged)
                    .OrderBy(u => u.Id)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    Console.WriteLine("❌ No user to message.");
                    return;
                }

                var userResult = await _instaApi.UserProcessor.GetUserAsync(user.Username);
                if (!userResult.Succeeded)
                {
                    Console.WriteLine($"❌ Failed to fetch user data: {user.Username}");
                    return;
                }

                var userId = userResult.Value.Pk.ToString();

                var message = "سلام دوست قشنگم!😍🎀\n" +
                              "اومدیم یه چیز بامزه و رنگی بهت نشون بدیم! 😍\n" +
                              "پر از لوازم تحریر خفن 😍، استیکرهای باحال ✨، دفترای خاص 📝 و کلی چیز جیگرررر! 🍭💥\n" +
                              "اگه دنبال حس خوب و انرژی مثبتی، پیج ما دقیقاً همونجاست که باید باشی! 🧡\n" +
                              "بیااااا کنارمون، با یه فالو کوچولو بهمون دلگرمی بده 💫\n" +
                              "👈 منتظر دیدنت هستیم! 😇\n" +
                              "📌@mazhi_chi";

                var result = await _instaApi.MessagingProcessor.SendDirectTextAsync(userId, null, message);
                if (result.Succeeded)
                {
                    // Attach user to the context to make sure it's tracked
                    _dbContext.TargetUsers.Attach(user);
                    user.IsMessaged = true;
                    user.MessagedAt = DateTime.Now;

                    try
                    {
                        await _dbContext.SaveChangesAsync();
                        Console.WriteLine($"✅ {user.Username} updated in DB.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❗ DB update error for {user.Username}: {ex.Message}");
                    }

                }
                else
                {
                    Console.WriteLine($"❌ Failed to send message to {user.Username}");
                }
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
