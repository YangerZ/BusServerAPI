using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class t_busline_shape
    {

        public string lineguid { get; set; }
        public int ordernumber { get; set; }
        public int direction { get; set; }
        public int startpid { get; set; }
        public int stype { get; set; }
        public string sname { get; set; }
        public int endpid { get; set; }
        public int etype { get; set; }
        public string ename { get; set; }
        public decimal length { get; set; }
       
    }
}
