using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class BusLine
    {
        //T_BusLine
        public string Lineguid { get; set; }
        public string Name { get; set; }
        public int IsDefault { get; set; }
        public int Type { get; set; }
        public int Direction { get; set; }
        public string First { get; set; }
        public string Last { get; set; }
        public string interval { get; set; }
        public decimal Mileage { get; set; }
        public string Time { get; set; }
        public string company { get; set; }
        public string parentguid { get; set; }

    }
}
