using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class t_userinfo
    {
        public int id { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string duty { get; set; }
        public string userid { get; set; }
        public string role { get; set; }
        public string realname { get; set; }
        public string other { get; set; }
        public string guestid { get; set; }
        public string func { get; set; }
    }
    public class t_userinfo_trans
    {
        
        public string duty { get; set; }
        public string userid { get; set; }
        public string role { get; set; }
        public string realname { get; set; }
        public string other { get; set; }
        public string guestid { get; set; }
        public string func { get; set; }
    }
}
