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
                //路网
                var  net_length = mySpatialRepo.ST_RoadNetLength_Region(gid);
                var net_area = mySpatialRepo.GetRegionAreaById(gid);
                var net_density = net_length / net_area;
                //线路
                var busline_count = mySpatialRepo.ST_BusLineCount_Region(gid);
                var busline_length = mySpatialRepo.ST_BusLineLength_Region(gid);
                var busline_density= busline_length / net_area;
                //中途站
                var stop_count = mySpatialRepo.ST_BusStopCount_Region(gid);
                var stoptransfer_count =mySpatialRepo.ST_BusStopTransfer_Count(gid);
                var cover300 = mySpatialRepo.ST_BusStopCover_Region(gid, "buffer300");
                cover300 = cover300 / net_area;
                var cover500 = mySpatialRepo.ST_BusStopCover_Region(gid, "buffer500");
                cover500 = cover500 / net_area;
                var cover600 = mySpatialRepo.ST_BusStopCover_Region(gid, "buffer600");
                cover600 = cover600 / net_area;
                //场站
                var station_count = mySpatialRepo.ST_BusStationCount_Region(gid);
                var station_area = mySpatialRepo.ST_BusStationArea_Region(gid);
                var station_repair_count = mySpatialRepo.ST_BusStationRepairCount_Region(gid);

                return Json(new
                {
                    success = "200",
                    data = new {
                        //线网
                        netlength = net_length,
                        netdensity = net_density,
                        //线路
                        buslinecount = busline_count,
                        buslinelength = busline_length,
                        buslinedensity = busline_density,
                        //中途站
                        stopcount = stop_count,
                        stoptransfercount = stoptransfer_count,
                        cover300ratio = cover300,
                        cover500ratio = cover500,
                        cover600ratio = cover600,
                        //场站
                        stationcount = station_count,
                        stationarea = station_area,
                        stationrepaircount = station_repair_count
                    }

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
                var planid = regionparams["planid"].ToString();
                var lineguid= regionparams["lineguid"].ToString();
                //计算过程
                /*
                 * code
                 * 
                 */
                 //线网
                var net_length = mySpatialRepo.ST_Plan_RoadNetLength_Region(lineguid,planid,gid);
                var net_area = mySpatialRepo.GetRegionAreaById(gid);
                var net_density = net_length / net_area;
                //线路
                var busline_count = mySpatialRepo.ST_PlanBusLineCount_Region(gid, planid, lineguid);
                var busline_length = mySpatialRepo.ST_PlanBusLineLength_Region(gid, planid, lineguid);
                var busline_density = busline_length / net_area;
               
                //中途站
                var stop_count = mySpatialRepo.ST_Plan_BusStopCount_Region(lineguid, planid, gid);
                var stoptransfer_count = mySpatialRepo.ST_Plan_BusStopTransfer_Count(lineguid, planid, gid);
   
                var cover300 = mySpatialRepo.ST_BusStopCover_Region(planid,gid, "buffer300",300);
                cover300 = cover300 / net_area;
                var cover500 = mySpatialRepo.ST_BusStopCover_Region(planid, gid, "buffer500", 500);
                cover500 = cover500 / net_area;
                var cover600 = mySpatialRepo.ST_BusStopCover_Region(planid, gid, "buffer600", 600);
                cover600 = cover600 / net_area;
                //场站
                var station_count = mySpatialRepo.ST_BusStationCount_Region(gid);
                var station_area = mySpatialRepo.ST_BusStationArea_Region(gid);
                var station_repair_count = mySpatialRepo.ST_BusStationRepairCount_Region(gid);

                //返回值
                return Json(new
                {
                    success = "200",
                    data = new {
                        //线网
                        netlength = net_length,
                        netdensity = net_density,
                        //线路
                        buslinecount = busline_count,
                        buslinelength = busline_length,
                        buslinedensity = busline_density,
                        //中途站
                        stopcount = stop_count,
                        stoptransfercount = stoptransfer_count,
                        cover300ratio = cover300,
                        cover500ratio = cover500,
                        cover600ratio = cover600,
                        //场站
                        stationcount = station_count,
                        stationarea = station_area,
                        stationrepaircount = station_repair_count
                    }
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