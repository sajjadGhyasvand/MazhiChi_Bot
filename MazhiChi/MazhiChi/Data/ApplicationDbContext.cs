using MazhiChi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazhiChi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<UserProfile> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=78.135.89.34;Port=5432;Database=instabot;Username=mazhichi_user;Password=1374@Sajjad@1374");
        }
    }
}
