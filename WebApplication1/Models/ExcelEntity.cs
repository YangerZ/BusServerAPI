using EPPlus.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class ExcelEntity
    {
    }
    public class PersonDto
    {
        [ExcelTableColumn("姓")]
        [Required(ErrorMessage = "First name cannot be empty.")]
        [MaxLength(50, ErrorMessage = "First name cannot be more than {1} characters.")]
        public string FirstName { get; set; }

        [ExcelTableColumn("名称")]
        public string LastName { get; set; }

        [ExcelTableColumn(3)]
        [Range(1900, 2050, ErrorMessage = "Please enter a value bigger than {1}")]
        public int YearBorn { get; set; }

        public decimal NotMapped { get; set; }
    }

    public class t_linenumber_exportview
    {
        [ExcelTableColumn("线路名称")]
        public string linename { get; set; }
        [ExcelTableColumn("平均站间距")]
        public float averagelength { get; set; }
        [ExcelTableColumn("1次换乘可到达线路数")]
        public int buslinecount { get; set; }
        [ExcelTableColumn("非直线系数")]
        public float bendrate { get; set; }
        [ExcelTableColumn("线路长度(km)")]
        public float totallength { get; set; }
        [ExcelTableColumn("站点个数")]
        public int stationcount { get; set; }
        //[ExcelTableColumn("比较线路")]
        //public string clinename { get; set; }
        //[ExcelTableColumn("重合率")]
        //public float coincidence { get; set; }

    }
    public class t_divisionnumber_exportview
    {
        [ExcelTableColumn("区域名称")]
        public string name { get; set; }
        [ExcelTableColumn("级别")]
        public string dtype { get; set; }//主键
        [ExcelTableColumn("区域ID")]
        public int gid { get; set; }
        [ExcelTableColumn("线网总长度")]
        public float linelength { get; set; }
        [ExcelTableColumn("线网密度")]
        public float linedensity { get; set; }
        [ExcelTableColumn("道路通行覆盖率")]
        public float roadcover { get; set; }
        [ExcelTableColumn("总线路数")]
        public int buslinecount { get; set; }
        [ExcelTableColumn("线路总长度")]
        public float buslinelength { get; set; }
        [ExcelTableColumn("线路密度")]
        public float buslinedensity { get; set; }
        [ExcelTableColumn("中途站个数")]
        public int stopcount { get; set; }
        [ExcelTableColumn("可换乘站个数")]
        public int changecount { get; set; }
        [ExcelTableColumn("300米覆盖率")]
        public float cover300 { get; set; }
        [ExcelTableColumn("500米覆盖率")]
        public float cover500 { get; set; }
        [ExcelTableColumn("600米覆盖率")]
        public float cover600 { get; set; }
        [ExcelTableColumn("场站个数")]
        public int stationcount { get; set; }
        [ExcelTableColumn("场站总面积")]
        public float stationarea { get; set; }
        [ExcelTableColumn("修保站个数")]
        public int repaircount { get; set; }
 
    }


    public class t_linenumber_exportview_filter
    {
        [ExcelTableColumn("线路名称")]
        public string linename { get; set; }
        [ExcelTableColumn("平均站间距(m)")]
        public string averagelength { get; set; }
        [ExcelTableColumn("1次换乘可到达线路数(条)")]
        public int buslinecount { get; set; }
        [ExcelTableColumn("非直线系数(%)")]
        public string bendrate { get; set; }
        [ExcelTableColumn("线路长度(km)")]
        public string totallength { get; set; }
        [ExcelTableColumn("站点个数")]
        public int stationcount { get; set; }
        //[ExcelTableColumn("比较线路")]
        //public string clinename { get; set; }
        //[ExcelTableColumn("重合率(%)")]
        //public string coincidence { get; set; }

    }
    public class t_divisionnumber_exportview_filter
    {
        [ExcelTableColumn("区域名称")]
        public string name { get; set; }
        [ExcelTableColumn("级别")]
        public string dtype { get; set; }//主键
        [ExcelTableColumn("区域ID")]
        public int gid { get; set; }
        [ExcelTableColumn("线网总长度(m)")]
        public string linelength { get; set; }
        [ExcelTableColumn("线网密度(km/km2)")]
        public string linedensity { get; set; }
        [ExcelTableColumn("道路通行覆盖率(%)")]
        public string roadcover { get; set; }
        [ExcelTableColumn("总线路数(条)")]
        public int buslinecount { get; set; }
        [ExcelTableColumn("线路总长度(km)")]
        public string buslinelength { get; set; }
        [ExcelTableColumn("线路密度(km/km2)")]
        public string buslinedensity { get; set; }
        [ExcelTableColumn("中途站个数(个)")]
        public int stopcount { get; set; }
        [ExcelTableColumn("可换乘站个数(个)")]
        public int changecount { get; set; }
        [ExcelTableColumn("300米覆盖率(%)")]
        public string cover300 { get; set; }
        [ExcelTableColumn("500米覆盖率(%)")]
        public string cover500 { get; set; }
        [ExcelTableColumn("600米覆盖率(%)")]
        public string cover600 { get; set; }
        [ExcelTableColumn("场站个数(个)")]
        public int stationcount { get; set; }
        [ExcelTableColumn("场站总面积(m2)")]
        public string stationarea { get; set; }
        [ExcelTableColumn("修保站个数(个)")]
        public int repaircount { get; set; }
    }
}
