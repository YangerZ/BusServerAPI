using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class t_divisionnumber
    {
        //区域指标
        public int gid { get; set; }
        public float linelength { get; set; }
        public float linedensity { get; set; }
        public float roadcover { get; set; }
        public int buslinecount { get; set; }
        public float buslinelength { get; set; }
        public float buslinedensity { get; set; }
        public int stopcount { get; set; }
        public int changecount { get; set; }
        public float cover300 { get; set; }
        public float cover500 { get; set; }
        public float cover600 { get; set; }
        public int stationcount { get; set; }
        public float stationarea { get; set; }
        public int repaircount { get; set; }
        public DateTime createtime { get; set; }
    }
}
