using InstagramApiSharp.API.Builder;
using InstagramApiSharp.API;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstagramApiSharp;
using MazhiChi.Data;
using MazhiChi.Models;
using Microsoft.EntityFrameworkCore;

namespace MazhiChi.Services
{
    public class InstagramService
    {
        private readonly IInstaApi _instaApi;
        private readonly ApplicationDbContext _dbContext;

        public InstagramService(IInstaApi instaApi, ApplicationDbContext dbContext)
        {
            _instaApi = instaApi;
            _dbContext = dbContext;
        }

        public async Task SendMessagesToUnmessagedUsersAsync(int dailyLimit)
        {
            var now = DateTime.Now;
            if (now.Hour < 8 || now.Hour >= 24)
            {
                Console.WriteLine("⏸️ Out Of Context.");
                await Task.Delay(TimeSpan.FromMinutes(30));
                return;
            }

            var usersToMessage = await _dbContext.TargetUsers
                .Where(u => !u.IsMessaged)
                .OrderBy(u => u.Id)
                .Take(dailyLimit)
                .ToListAsync();

            int sent = 0;

            foreach (var user in usersToMessage)
            {
                now = DateTime.Now;
                if (now.Hour < 8 || now.Hour >= 24)
                    break;

                // دریافت اطلاعات کاربر برای گرفتن userId
                var userResult = await _instaApi.UserProcessor.GetUserAsync(user.Username);
                if (!userResult.Succeeded)
                {
                    Console.WriteLine($"❌ Get Data is Failed: {user.Username}");
                    continue;
                }

                var userId = userResult.Value.Pk.ToString();
                // var message = "سلام دوست خوبم! خوشحال می‌شیم به فروشگاه ما سر بزنی ✨🛍️\nInstagram: @yourpage";
                var message = "سلام دوست قشنگم!😍🎀\n" +
                   "اومدیم یه چیز بامزه و رنگی بهت نشون بدیم! 😍\n" +
                   "پر از لوازم تحریر خفن 😍، استیکرهای باحال ✨، دفترای خاص 📝 و کلی چیز جیگرررر! 🍭💥\n" +
                   "اگه دنبال حس خوب و انرژی مثبتی، پیج ما دقیقاً همونجاست که باید باشی! 🧡\n" +
                   "بیااااا کنارمون، با یه فالو کوچولو بهمون دلگرمی بده 💫\n" +
                   "👈 منتظر دیدنت هستیم! 😇\n" +
                   "📌@mazhi_chi";

                var result = await _instaApi.MessagingProcessor.SendDirectTextAsync(userId,null, message);
                if (result.Succeeded)
                {
                    user.IsMessaged = true;
                    user.MessagedAt = DateTime.Now;
                    await _dbContext.SaveChangesAsync();
                    sent++;

                    Console.WriteLine($"✅  {user.Username} send to.");

                    var delay = new Random().Next(120, 300); // تاخیر بین ۲ تا ۵ دقیقه
                    await Task.Delay(TimeSpan.FromSeconds(delay));
                }
                else
                {
                    Console.WriteLine($"❌ ا,esaage send failed{user.Username}  .");
                }
            }

            Console.WriteLine($"🎯Today send  ({sent}) message.");
        }

    }

}
