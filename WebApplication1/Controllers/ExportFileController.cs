using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EPPlus.Core.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApplication1.Models;
using WebApplication1.Repos;

namespace WebApplication1.Controllers
{
    [Route("bus/[controller]")]
    public class ExportFileController : Controller
    {
        private System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo();
        public DbConfig dbcofig;
        public PostDBRepo myPostRepo = new PostDBRepo();
        public ExportFileController(IOptions<DbConfig> config)
        {
            // Or to temporarily use legacy PostGIS on a single connection only:
            dbcofig = config.Value;
            myPostRepo.connectionString = dbcofig.PostgresqlConnection;
        }
        public IActionResult Index()
        {
            return View();
        }


        //[HttpGet]
        //[Route("Export/{filter}")]
        //public async Task<IActionResult> Test()
        //{
        //    var task = await Export();
        //    return Json(new { success = "404", error = ""});
        //}

        //public  async  Task<IActionResult> Export()
        //{
        //    string fileName = $"{Guid.NewGuid().ToString()}.xlsx";
        //    //测试数据
        //    PersonDto pd1 = new PersonDto { FirstName="asd", LastName="sadsad",  NotMapped=0.34m, YearBorn=2009};
        //    PersonDto pd2 = new PersonDto { FirstName = "asd", LastName = "sadsad", NotMapped = 0.34m, YearBorn = 2009 };
        //    PersonDto pd3 = new PersonDto { FirstName = "asd", LastName = "sadsad", NotMapped = 0.34m, YearBorn = 2009 };
        //    List<PersonDto> persons = new List<PersonDto>();
        //    persons.Add(pd1);
        //    persons.Add(pd2);
        //    persons.Add(pd3);
        //    // Convert list into ExcelPackage
        //    byte[] exportByte = persons.ToXlsx(true);
        //    IActionResult ac=  File(exportByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        //    return  ac;
        
        //}

        [HttpGet]
        [Route("AllLineTarget/Export/{type}")]
        public async Task<IActionResult> ExportAllTargetTable(int type)
        {
            nfi.NumberDecimalDigits = 2;
            string fileName = String.Empty;
            byte[] exportByte = null;
            try
            {
                //区域指标
                if (type == 0)
                {
                    IEnumerable<t_divisionnumber_exportview> exportareas = myPostRepo.Get_V_ALL_AreaTarget();
                    if (exportareas == null || exportareas.Count() == 0)
                    {
                        return Json(new { success = "200", data = fileName });
                    }
                    List<t_divisionnumber_exportview_filter> filters = new List<t_divisionnumber_exportview_filter>();
                    foreach (var item in exportareas)
                    {
                        t_divisionnumber_exportview_filter temp = new t_divisionnumber_exportview_filter();
                        temp.buslinecount = item.buslinecount;
                        temp.buslinedensity = item.buslinedensity.ToString("N", nfi);
                        temp.buslinelength = item.buslinelength.ToString("N", nfi);
                        temp.changecount = item.changecount;
                        temp.cover300 = (item.cover300 * 100).ToString("N", nfi);
                        temp.cover500 = (item.cover500 * 100).ToString("N", nfi);
                        temp.cover600 = (item.cover600 * 100).ToString("N", nfi);
                        temp.dtype = item.dtype;
                        temp.gid = item.gid;
                        temp.linedensity = item.linedensity.ToString("N", nfi);
                        temp.linelength = item.linelength.ToString("N", nfi);
                        temp.name = item.name;
                        temp.repaircount = item.repaircount;
                        temp.roadcover = (item.roadcover * 100).ToString("N", nfi);
                        temp.stationarea = item.stationarea.ToString("N", nfi);
                        temp.stationcount = item.stationcount;
                        temp.stopcount = item.stopcount;
                        filters.Add(temp);
                    }
                    exportByte = filters.ToXlsx();
                }
                //线路指标
                if (type == 1)
                {
                    List<t_linenumber_exportview_filter> filters = new List<t_linenumber_exportview_filter>();
                    IEnumerable<t_linenumber_exportview> exportlines = myPostRepo.Get_V_ALL_LineTarget();
                    if (exportlines == null || exportlines.Count() == 0)
                    {
                        return Json(new { success = "200", data = fileName });
                    }
                    foreach (var item in exportlines)
                    {
                        t_linenumber_exportview_filter temp = new t_linenumber_exportview_filter();
                        temp.averagelength = (item.averagelength * 100).ToString("N", nfi);
                        temp.bendrate = (item.bendrate * 100).ToString("N", nfi);
                        temp.buslinecount = item.buslinecount;
                        temp.stationcount = item.stationcount;
                        temp.totallength = item.totallength.ToString("N", nfi);
                        temp.linename = item.linename;
                        filters.Add(temp);
                    }
                    exportByte = filters.ToXlsx();
                }
                fileName = $"{Guid.NewGuid().ToString()}.xlsx";
                return File(exportByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        //[HttpGet]
        //[Route("AllLineTarget/Export/{type}")]
        //public async Task<IActionResult> ExportAllTargetTable(int type)
        //{
        //    string fileName =String.Empty;
        //    byte[] exportByte = null;
        //    try
        //    {
        //        //区域指标
        //        if (type == 0)
        //        {
        //            IEnumerable<t_divisionnumber_exportview> exportareas = myPostRepo.Get_V_ALL_AreaTarget();
        //            if (exportareas == null || exportareas.Count() == 0)
        //            {
        //                return Json(new { success = "200", data = fileName});
        //            }
        //            exportByte = exportareas.ToXlsx();
        //        }
        //        //线路指标
        //        if (type == 1)
        //        {
        //            IEnumerable<t_linenumber_exportview> exportlines = myPostRepo.Get_V_ALL_LineTarget();
        //            if (exportlines == null || exportlines.Count() == 0)
        //            {
        //                return Json(new { success = "200", data = fileName });
        //            }
        //            exportByte = exportlines.ToXlsx();
        //        }
        //        fileName = $"{Guid.NewGuid().ToString()}.xlsx";
        //        return File(exportByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        //    }
        //    catch(Exception ex)
        //    {
        //        return Json(new { success = "404", error = ex.ToString() });
        //    }
        //}
    }
}