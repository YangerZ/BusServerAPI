﻿using System;
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
        public float totallength { get; set; }
        public int stationcount { get; set; }
        //缓冲区内设施站点个数
        public int department { get; set; }
        public int school { get; set; }
        public int hospital { get; set; }
        public int community { get; set; }
        public int commerce { get; set; }
        public int scenicspot { get; set; }


    }
}
