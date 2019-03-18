
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoAPI.Geometries;
namespace WebApplication1.Models
{
    public class t_plan_lineshape
    {
        public string planid { get; set; }
        public int ordernumber { get; set; }
        public int startpid { get; set; }
        public int endpid { get; set; }
        public double length { get; set; }
        public  LineString geom { get ; set; }
        //public  Coordinate[] points { get; set; }
        public List<double[]> points { get; set; }

    }
}
