using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class OrganizeInfo
    {
        //T_OrganizeInfo
        public string id { get; set; }
        //public string Name { get; set; }
        public string  parentguid { get; set; }
        public decimal Blevel { get; set; }
        public string OrderNumber { get; set; }
        public string Label { get; set; }
        public IEnumerable<OrganizeInfo> children { get; set; }

    }
}
