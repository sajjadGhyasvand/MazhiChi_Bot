using InstagramApiSharp.API;
using InstagramApiSharp;
using MazhiChi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MazhiChi.Data;
using Microsoft.EntityFrameworkCore;

namespace MazhiChi.Services
{

    public class ScraperService
    {
        private readonly IInstaApi _instaApi;
        private readonly ApplicationDbContext _dbContext;

        public ScraperService(IInstaApi instaApi, ApplicationDbContext dbContext)
        {
            _instaApi = instaApi;
            _dbContext = dbContext;
        }

        /* public async Task ScrapeFollowers(string targetUsername)
         {
             if (_instaApi == null)
             {
                 Console.WriteLine("Instagram API is not initialized.");
                 return;
             }

             try
             {
                 var userResult = await _instaApi.UserProcessor.GetUserAsync(targetUsername);
                 if (!userResult.Succeeded)
                 {
                     Console.WriteLine($"Error: {userResult.Info.Message}");
                     return;
                 }

                 var user = userResult.Value;
             }
             catch (Exception ex)
             {
                 Console.WriteLine($"Exception: {ex.Message}");
             }


             var followers = await _instaApi.UserProcessor.GetUserFollowersAsync(targetUsername, PaginationParameters.MaxPagesToLoad(1));
             if (!followers.Succeeded) return;

             foreach (var follower in followers.Value)
             {
                 if (_dbContext.TargetUsers.Any(u => u.Username == follower.UserName)) continue;

                 _dbContext.TargetUsers.Add(new TargetUser
                 {
                     Username = follower.UserName
                 });
             }

             await _dbContext.SaveChangesAsync();
         }*/
        public async Task ScrapeFollowers(string targetUsername)
        {
            // بررسی مقداردهی اولیه _instaApi
            if (_instaApi == null)
            {
                Console.WriteLine("Instagram API is not initialized.");
                return;
            }

            // تلاش برای دریافت اطلاعات کاربر
            try
            {
                var userResult = await _instaApi.UserProcessor.GetUserAsync(targetUsername);
                if (!userResult.Succeeded)
                {
                    Console.WriteLine($"Error in GetUserAsync: {userResult.Info.Message}");
                    return;
                }
                var user = userResult.Value;
                Console.WriteLine($"User found: {user.UserName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetUserAsync: {ex.Message}");
                return;
            }

            // دریافت لیست فالوورها
            var followersResult = await _instaApi.UserProcessor.GetUserFollowersAsync(targetUsername, PaginationParameters.MaxPagesToLoad(1));

            if (followersResult == null)
            {
                Console.WriteLine("Followers result is null.");
                return;
            }
            if (!followersResult.Succeeded)
            {
                Console.WriteLine("Error in GetUserFollowersAsync: " + followersResult.Info.Message);
                return;
            }

            // پردازش و ذخیره فالوورها
            foreach (var follower in followersResult.Value)
            {
                // اگر شیء follower یا UserName آن null باشد، از پردازش آن صرفنظر کن
                if (follower == null)
                {
                    Console.WriteLine("Found a null follower object. Skipping...");
                    continue;
                }
                if (string.IsNullOrEmpty(follower.UserName))
                {
                    Console.WriteLine("Found a follower with null or empty username. Skipping...");
                    continue;
                }

                // بررسی می‌کنیم که این کاربر قبلاً ذخیره نشده باشد.
                if (_dbContext.TargetUsers.Any(u => u.Username == follower.UserName))
                    continue;

                _dbContext.TargetUsers.Add(new TargetUser
                {
                    Username = follower.UserName
                });

                Console.WriteLine($"Added {follower.UserName} to TargetUsers.");
            }

            try
            {
                await _dbContext.SaveChangesAsync();
                Console.WriteLine("Changes saved to database.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception during SaveChangesAsync: " + ex.Message);
            }
        }

    }
}

