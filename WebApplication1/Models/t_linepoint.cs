using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql.LegacyPostgis;
using Npgsql;
namespace WebApplication1.Models
{
    public class t_linepoint
    {
        // Views T_LinePoint
       
        public int id { get; set; }
        public string lineguid { get; set; }
        public int ordernumber { get; set; }
        public int direction { get; set; }
        public int pid { get; set; }
        public string name { get; set; }
        public string linename { get; set; }
        
    }
    public class mypoint
    {
        
        public double X { get; set; }
        public double Y { get; set; }
        public uint SRID { get; set; }
         
    }
    
}
