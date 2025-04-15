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

        public async Task ScrapeFollowers(string targetUsername)
        {
            var user = await _instaApi.UserProcessor.GetUserAsync(targetUsername);
            if (!user.Succeeded) return;

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
        }
    }
}

