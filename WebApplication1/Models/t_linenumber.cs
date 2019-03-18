using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class t_linenumber
    {
        //线路指标计算结果
        public string lineguid { get; set; }//主键
        public float averagelength { get; set; }
        public int buslinecount { get; set; }
        public float bendrate { get; set; }
        public string c_lineguid { get; set; }
        public float coincidence { get; set; }
        public DateTime createtime { get; set; }
       
    }
}
