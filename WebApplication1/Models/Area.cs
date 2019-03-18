using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql.LegacyPostgis;
using Npgsql;

namespace WebApplication1.Models
{
    
    public class Area
    {
        public int gid { get; set; }
        public int objectid { get; set; }
        public string continent { get; set; }
        public decimal sqmi { get; set; }
        public decimal sqkm { get; set; }
        public PostgisGeometry geom { get; set; }
        
       
    }
}
