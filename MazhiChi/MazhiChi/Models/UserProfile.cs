using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazhiChi.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public bool IsMessaged { get; set; }
    }
}
