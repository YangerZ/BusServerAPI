using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApplication1.Models;
using WebApplication1.Repos;
using Npgsql;
using Npgsql.LegacyPostgis;
using WebApplication1.ParamsObj;
using Newtonsoft.Json.Linq;

namespace WebApplication1.Controllers
{
    [Route("bus/[controller]")]
    public class SingleLineController : Controller
    {

        public DbConfig dbcofig;
        public SpatialRepo mySpatialRepo = new SpatialRepo();

        public SingleLineController(IOptions<DbConfig> config)
        {
            // Or to temporarily use legacy PostGIS on a single connection only:
            dbcofig = config.Value;
            mySpatialRepo.connectionString = dbcofig.PostgresqlConnection;
        }

        public IActionResult Index()
        {
            return View();
        }
        // GET api/values
        [HttpGet]
        public JsonResult Get()
        {
            return Json(new { success = "200", data = "Ok" });
        }


        #region 指标计算

        [HttpPost("Target")]
        public JsonResult GetLineTargetValues([FromBody] LineTargetParam lineparams)
        {
            try
            {
                string lineguid = Convert.ToString(lineparams.guid_cur);
                int direct = Convert.ToInt32(lineparams.direct_cur);
                string lineguid2 = "";
                int direct2 = 0;
                if (lineparams.guid_sel != null)
                {
                    lineguid2 = Convert.ToString(lineparams.guid_sel);
                    direct2 = Convert.ToInt32(lineparams.direct_sel);
                }
              
                //线段距离 站间距
                var lines = mySpatialRepo.GetStationsBreakLength(lineguid, direct);
                if (lines == null ||lines.Count()==0)
                {//若线路未找到对应 shape rid  各结果返回0
                      return Json(new { success = "200",
                          data =
                    new { pjzjj = 0, khcxls = 0, fzxxs = 0, xdchl = 0 }
                      });
                }
                //站点间线段总个数
                decimal total = lines.Sum(d => d.length);
                //平均站间距
                decimal average = total / lines.Count();
                //1次换乘到达的线路数
                var crossnums = mySpatialRepo.GetLineNumbersByOnce(lineguid, direct);
                //非直线系数   空间直线距离/线段路线长度
                decimal distance = mySpatialRepo.GetDistanceFromPoints(lines.First().startpid, lines.Last().endpid);
                decimal coefficient = distance / total;
                //线段重合率
                decimal repeatlength = mySpatialRepo.IntersectionBetweenTwoLines(lineguid, direct, lineguid2, direct2);
                decimal repeatRatio = repeatlength / total;
                //back 
                return Json(new
                {
                    success = "200",
                    data = new { pjzjj = average, khcxls = crossnums, fzxxs = coefficient, xdchl = repeatRatio }

                });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        // 规划线路计算实时指标
        [HttpPost("PlanTarget")]
        public JsonResult GetPlanLineTargetValues([FromBody] JObject planlineparams)
        {
            try
            {
                string planid = planlineparams["planid"].ToString();
                string lineguid = planlineparams["lineguid"].ToString();
                int direct = int.Parse(planlineparams["direction"].ToString());
                //planid = "f297fe24-4ac1-45d5-94ea-8dfdff851566";
                //lineguid = "EE172CA1-4C44-4AD2-A53F-966F62AD3F03";
                //direct = 0;
                //线段距离 站间距
                var lines = mySpatialRepo.GetPlanLineInfo(planid);
                if (lines == null || lines.Count() == 0)
                {//若线路未找到对应 shape rid  各结果返回0
                    return Json(new
                    {
                        success = "200",
                        data =
                  new { lineinfos= lines, pjzjj = 0, khcxls = 0, fzxxs = 0, xdchl = 0 }
                    });
                }
                //站点间线段总个数
                decimal total =decimal.Parse(lines.Sum(d => d.length).ToString());
                //平均站间距
                decimal average = total / lines.Count();
                //1次换乘到达的线路数
                var crossnums = mySpatialRepo.GetPlanLineNumbersByOnce(lines);
                //非直线系数   空间直线距离/线段路线长度
                decimal distance = mySpatialRepo.GetDistanceFromPoints(lines.First().startpid, lines.Last().endpid);
                decimal coefficient = distance / total;
                //线段重合率
                decimal repeatlength = mySpatialRepo.IntersectionBetweenTwoLines(planid,lineguid,direct);
                decimal repeatRatio = repeatlength / total;
                //back 
                return Json(new
                {
                    success = "200",
                    data = new {lineinfos=lines, pjzjj = average, khcxls = crossnums, fzxxs = coefficient, xdchl = repeatRatio }

                });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        #endregion
    }
}