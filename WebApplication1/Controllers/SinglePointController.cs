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

namespace WebApplication1.Controllers
{
    [Route("bus/[controller]")]
    public class SinglePointController : Controller
    {
        public DbConfig dbcofig;
        public SpatialRepo mySpatialRepo = new SpatialRepo();

        public SinglePointController(IOptions<DbConfig> config)
        {
            // Or to temporarily use legacy PostGIS on a single connection only:
            dbcofig = config.Value;
            mySpatialRepo.connectionString = dbcofig.PostgresqlConnection;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public JsonResult Get()
        {
            return Json(new { success = "200", data = "Ok" });
        }

 
        #region 指标计算
        [HttpGet,Route("Target/{pid}")]
        public JsonResult GetPointTargetValues(int pid)
        {
            try
            {
                //计算换乘线路数
                var lines = mySpatialRepo.SumCrossingLines(pid);
                var sumlines = 0;
                if (lines == null)
                {
                    sumlines = 0;
                }
                else
                {
                    sumlines = lines.Count();
                }
                //计算换成线路上的站点数
                var sumpoints = mySpatialRepo.SumRelatedPoints(pid, lines);
                //30米缓冲区查询后的个站点和线路数 变更
                //var bufferlines = mySpatialRepo.SumCrossingLinesByBuffer(pid, 300.0);
                int bufferlinecount = 0;
                //if (bufferlines != null)
                //{
                //    bufferlinecount = bufferlines.Count();
                //}
                int bufferpointcount = 0;
                //var bufferpoints = mySpatialRepo.SumRelatedPoints(pid,bufferlines);
                //if (bufferpoints != null)
                //{
                //    bufferpointcount = bufferpoints.Count();
                //}

                var bufferpoints=mySpatialRepo.SumPointsByBuffer(pid, 500.0);
                if(bufferpoints!=null)
                {
                    bufferpointcount = bufferpoints.Count();
                }
                //计算最邻近的路口
                var nearestpoint =mySpatialRepo.FindNearestRoad(pid);
                //back 
                return Json(new
                {
                    success = "200",
                    data =
                    new { hcxl=sumlines,
                        hczd =sumpoints.Count(),
                        hcxl_30 = bufferlinecount,
                        hczd_30 = bufferpointcount,
                        zjlk =nearestpoint}
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