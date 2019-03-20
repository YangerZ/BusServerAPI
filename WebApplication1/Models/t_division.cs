using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class t_division
    {
        public int objectid { get; set; }
        public int gid { get; set; }
        public string name { get; set; }
        public int type { get; set; }
        public decimal area { get; set; }
        public int parentId { get; set; }
        public int level { get; set; }
        public int ordernumber { get; set; }
        //public Polygon geom {get;set;}
    }
}
