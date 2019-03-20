using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using WebApplication1.Repos;

namespace WebApplication1.Controllers
{
    [Route("bus/[controller]")]
    public class RegionController : Controller
    {
        public DbConfig dbcofig;
        public SpatialRepo mySpatialRepo = new SpatialRepo();

        public RegionController(IOptions<DbConfig> config)
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
        public JsonResult CalculateRegionTarget([FromBody] JObject regionparams)
        {
            try
            {
                var gid = int.Parse(regionparams["gid"].ToString());
                //计算过程
                /*
                 * code
                 * 
                 */
                //返回值
                var  net_length = mySpatialRepo.ST_RoadNetLength_Region(gid);
                var net_area = mySpatialRepo.GetRegionAreaById(gid);
                var net_density = net_length / net_area;
                var busline_count = mySpatialRepo.ST_BusLineCount_Region(gid);
                var busline_length = mySpatialRepo.ST_BusLineLength_Region(gid);
                var busline_density= busline_length / net_area;

                return Json(new
                {
                    success = "200",
                    data = new { }

                });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        // 规划线路计算实时指标
        [HttpPost("PlanTarget")]
        public JsonResult CalculatePlanRegionTarget([FromBody] JObject  regionparams)
        {
            try
            {
                var gid =int.Parse(regionparams["gid"].ToString());
                var planid = regionparams["panid"].ToString();
                var lineguid= regionparams["lineguid"].ToString();
                //计算过程
                /*
                 * code
                 * 
                 */

                //返回值
                return Json(new
                {
                    success = "200",
                    data = new {   }

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