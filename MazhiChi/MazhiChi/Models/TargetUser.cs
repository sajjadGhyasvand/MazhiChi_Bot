using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazhiChi.Models
{
    public class TargetUser
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public bool IsPrivate { get; set; }
        public int FollowersCount { get; set; }
        public bool IsMessaged { get; set; } = false;  // برای جلوگیری از ارسال دوباره پیام
        public DateTime? MessagedAt { get; set; }      // زمان ارسال پیام
    }
}
