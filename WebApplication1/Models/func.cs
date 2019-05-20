using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class func
    {
        //public int id { get; set; }
        public string num { get; set; }
        public string icon { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        //public string role { get; set; }
        public string pid { get; set; }
        public IEnumerable<func> children { get; set; }
    }
    public class pfunc
    {
        public string num { get; set; }
        public string icon { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        //public string role { get; set; }
        public string pid { get; set; }
         
    }
}
