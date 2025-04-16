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
        public DbSet<TargetUser> TargetUsers { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TargetUser>()
                .Property(e => e.MessagedAt)
                .HasConversion(
                    v => v, // هنگام ذخیره‌سازی تغییری نمی‌دهیم (فرض بر این است که مقدار وارد شده UTC است)
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null // هنگام خواندن، Kind رو به UTC مشخص می‌کنیم
                );
        }
    }


}
