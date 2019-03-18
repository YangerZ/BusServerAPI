using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using Newtonsoft.Json.Linq;
using Npgsql.LegacyPostgis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class t_pointinfo
    {
        public int gid { get; set; }
        public int objectid { get; set; }
        public int pid { get; set; }
        public string name { get; set; }
        public int type { get; set; }
        public int __gid { get; set; }
        public int id1 { get; set; }
        public int id2 { get; set; }
        public Point geom { get; set; }
        
        public double[] coordinate {get;set;}
    }
}
