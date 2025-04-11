

using InstagramApiSharp;
using InstagramApiSharp.Classes;
using MazhiChi.Data;
using MazhiChi.Models;
using MazhiChi.Services;

namespace MazhiChi.Scheduler;

public class FollowScheduler
{
    private readonly InstagramService _service;

    public FollowScheduler(InstagramService service)
    {
        _service = service;
    }

    public async Task ScrapeAndFollowAsync(string targetUsername)
    {
        var api = _service.GetApi();
        var followers = await api.UserProcessor
            .GetUserFollowersAsync(targetUsername, PaginationParameters.MaxPagesToLoad(5));

        foreach (var user in followers.Value)
        {
            if (!user.IsPrivate)
            {
                await api.UserProcessor.FollowUserAsync(user.Pk);

                using var db = new ApplicationDbContext();
                db.Users.Add(new UserProfile
                {
                    Username = user.UserName,
                    UserId = user.Pk.ToString(),
                    FollowDate = DateTime.Now,
                    IsFollowedBack = false
                });
                await db.SaveChangesAsync();
            }
        }
    }
}
