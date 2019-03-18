using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class DivisionNumber
    {
       //T_DivisionNUmber
        public string Name { get; set; }
        public int Level { get; set; }
        public decimal Type { get; set; }
        public int GID { get; set; }
        public decimal LineLength { get; set; }
        public decimal LineDensity { get; set; }
        public decimal RoadCover { get; set; }
        public int BusLineCount { get; set; }
        public decimal BusLineLength { get; set; }
        public decimal BusLineDensity { get; set; }
        public int StopCount { get; set; }
        public int ChangeCount { get; set; }
        public decimal Cover300 { get; set; }
        public decimal Cover500 { get; set; }
        public decimal Cover600 { get; set; }
        public int StationCount { get; set; }
        public decimal StationArea { get; set; }
        public int RepairCount { get; set; }
    }
}