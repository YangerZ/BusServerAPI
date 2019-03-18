using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class t_planlineinfo
    {
        public string planid { get; set; }
        public int ordernumber { get; set; }
        public int startpid { get; set; }
        public int endpid { get; set; }
        public double length { get; set; }
        //public LineString geom { get; set; }
        //public  Coordinate[] points { get; set; }
        public int type { get; set; }
        public string name { get; set; }
        public int pid { get; set; }
        public List<double[]> points { get; set; }
    }
}
