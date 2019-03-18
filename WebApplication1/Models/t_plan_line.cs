using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class t_plan_line
    {
        public string planid { get; set; }
        public string planname { get; set; }
        public int plantype { get; set; }
        public string linename { get; set; }
        public string lineguid { get; set; }
        public DateTime createtime { get; set; }
        public string creator { get; set; }
        public string userid { get; set; }

    }
}
