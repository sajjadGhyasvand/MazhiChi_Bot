using InstagramApiSharp.API.Builder;
using InstagramApiSharp.API;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazhiChi.Services
{
    public class InstagramService
    {
        private readonly IInstaApi _api;

        public InstagramService(string username, string password)
        {
            var userSession = new UserSessionData
            {
                UserName = username,
                Password = password
            };

            _api = InstaApiBuilder.CreateBuilder()
                .SetUser(userSession)
                .UseLogger(new DebugLogger(LogLevel.All))
                .Build();
        }

        public async Task<bool> LoginAsync()
        {
            var result = await _api.LoginAsync();
            if (!result.Succeeded)
            {
                Console.WriteLine("❌ Login failed: " + result.Info?.Message);
                return false;
            }

            Console.WriteLine("✅ Login successful!");
            return true;
        }

        public IInstaApi GetApi()
        {
            return _api;
        }
    }
}
