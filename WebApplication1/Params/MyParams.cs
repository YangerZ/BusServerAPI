using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.ParamsObj
{
    public class MyParams
    {

    }
    public class LineTargetParam
    {
        public string guid_cur;
        public int direct_cur;
        public string guid_sel;
        public int direct_sel;

    }
     
    public class unionline
    {
        public double length { get; set; }
         public Geometry geom { get; set; }
    }
}
