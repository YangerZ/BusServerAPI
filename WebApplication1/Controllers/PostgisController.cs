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
using Newtonsoft.Json.Linq;
using NetTopologySuite.Geometries;
using GeoAPI.Geometries;

namespace WebApplication1.Controllers
{
    [Route("bus/[controller]")]
    public class PostgisController : Controller
    {
        public DbConfig dbcofig;
        public PostDBRepo myPostRepo = new PostDBRepo();

        public PostgisController(IOptions<DbConfig> config)
        {
            // Or to temporarily use legacy PostGIS on a single connection only:
            dbcofig = config.Value;
            myPostRepo.connectionString = dbcofig.PostgresqlConnection;
        }

        public IActionResult Index()
        {
            return View();
        }
        // GET api/values
        [HttpGet]
        public JsonResult Get()
        {
            return Json(new { success = "200", data = "123" });
        }

        #region Test Area table
        [HttpGet, Route("AllAreas")]
        public JsonResult GetAllAreas()
        {

            try
            {
                var temp = myPostRepo.GetAreaAllTable();
                return Json(new { success = "200", data = temp });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }

        }
        //Get bus/spatial/guid
        [HttpGet("Area/{gid}")]
        public JsonResult GetSigleDataByID(int gid)
        {
            Area temp = null;
            try
            {
                temp = myPostRepo.GetSingle_T_Area(gid);
                return Json(new { success = "200", data = temp });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        //Post bus/spatial/area body formdata
        //[HttpPost]
        [HttpGet, Route("Area/Add")]
        public JsonResult AddArea()//[FromBody] Area area)
        {
            bool addstatus = false;
            try
            {
                Area area = new Area();
                area.gid = 1000;
                area.objectid = 1000;
                area.sqkm = 0.23m;
                area.sqmi = 0.24m;
                area.continent = "ceshi";

                area.geom = null;
                addstatus = myPostRepo.AddSingle_T_Area(area);
                return Json(new { success = "200", data = addstatus });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }

        //[HttpPut("{gid}")]
        [HttpGet, Route("Area/Update")]
        public JsonResult UpdateArea()//int gid ,[FromBody] Area area)
        {
            bool updatestatus = false;
            try
            {
                Area area = new Area();
                area.gid = 1000;
                area.objectid = 2000;
                area.sqkm = 0.88m;
                area.sqmi = 0.44m;
                area.continent = "myxiugai";
                area.geom = null;
                updatestatus = myPostRepo.Update_T_Area(area);
                return Json(new { success = "200", data = updatestatus });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }

        [HttpDelete("{gid}")]
        public JsonResult DeleteArea(int gid)
        {

            bool delstatus = false;
            try
            {
                delstatus = myPostRepo.Delete_T_Area(gid);
                return Json(new { success = "200", data = delstatus });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        #endregion

        #region  Test T_LinePoint View
        //Get
        [HttpGet, Route("AllLinePoints")]
        public JsonResult GetAllLinePoints()
        {

            // return null ;
            try
            {
                var obj = myPostRepo.GetLinePointAllTable1();
                return Json(new { success = "200", data = obj });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }

        }
        //Get bus/spatial/guid
        [HttpGet("LinePoint/{gid}")]
        public JsonResult GetSigleByID(string lineguid)
        {
            t_linepoint temp = null;
            try
            {
                temp = myPostRepo.GetSingle_T_LinePoint(lineguid);
                return Json(new { success = "200", data = temp });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        //Post bus/spatial/area body formdata
        //[HttpPost]
        [HttpGet, Route("LinePoint/Add")]
        public JsonResult AddLinePoint()//[FromBody] Area area)
        {
            bool addstatus = false;
            try
            {
                t_linepoint linepoint = new t_linepoint();
                //area.gid = 1000;
                //area.objectid = 1000;
                //area.sqkm = 0.23m;
                //area.sqmi = 0.24m;
                //area.continent = "ceshi";

                // linepoint.geom = null;
                addstatus = myPostRepo.AddSingle_T_LinePoint(linepoint);
                return Json(new { success = "200", data = addstatus });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }

        //[HttpPut("{gid}")]
        [HttpGet, Route("LinePoint/Update")]
        public JsonResult UpdateLinePoint()//int gid ,[FromBody] Area area)
        {
            bool updatestatus = false;
            try
            {
                t_linepoint linepoint = new t_linepoint();
                //area.gid = 1000;
                //area.objectid = 2000;
                //area.sqkm = 0.88m;
                //area.sqmi = 0.44m;
                //area.continent = "myxiugai";
                //area.geom = null;
                updatestatus = myPostRepo.Update_T_LinePoint(linepoint);
                return Json(new { success = "200", data = updatestatus });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }

        [HttpDelete("LinePoint/{gid}")]
        public JsonResult DeleteLinePoint(string lineguid)
        {

            bool delstatus = false;
            try
            {
                delstatus = myPostRepo.Delete_T_LinePoint(lineguid);
                return Json(new { success = "200", data = delstatus });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        #endregion

        #region 需要计算的区域和公交线路的预处理
        [HttpGet]
        [Route("AreaLine/Create")]
        public async Task<IActionResult> CreateLineAreaRelationTable()
        {
            bool isadded = false;
            try
            {

                IEnumerable<t_division> divisions=myPostRepo.Get_T_Division();

                foreach (var divi in divisions)
                {
                    if (divi.gid == 116) continue;//全部区域的就不算了太麻烦了
                   IEnumerable<t_division_busline> divisionbuslines=  myPostRepo.GetBusLineFromDB(divi.gid);
                   if (divisionbuslines == null || divisionbuslines.Count() == 0) continue;
                     isadded=myPostRepo.AddBusLinesWithAreaID(divisionbuslines);
                }
                return Json(new { success = "200", data = isadded });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        #endregion

        #region 车辆线路信息 组织机构信息
        [HttpGet, Route("GetBusLines")]
        public JsonResult GetAllBusLine()
        {
            List<BusLine> temp = null;
            try
            {
                temp = myPostRepo.GetAll_T_BusLine();
                return Json(new { success = "200", data = temp });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        [HttpPost, Route("BusLines/Query")]
        public JsonResult QueryBusLines([FromBody] JObject param_plan)
        {
            try
            {
                List<BusLine> temp = null;
                string express = param_plan["express"].ToString();
                temp = myPostRepo.Query_T_BusLine(express);
                return Json(new { success = "200", data = temp });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        [HttpGet, Route("GetOrganizeInfos")]
        public JsonResult GetOrganizeInfo()
        {
            List<OrganizeInfo> temp = null;
            try
            {
                temp = myPostRepo.GetAll_T_OrganizeInfo();
                return Json(new { success = "200", data = temp });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        #endregion
        
        #region 站点信息数据
        [HttpGet, Route("GetPointInfos")]
        public JsonResult GetAllPointInfo()
        {
            IEnumerable<t_pointinfo> temp = null;
            try
            {
                temp = myPostRepo.Get_T_PointInfoTable();
                return Json(new { success = "200", data = temp });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        [HttpGet("PointInfo/{pid}")] 
        public JsonResult GetSiglePointInfoByID(int pid)
        {
            t_pointinfo temp = null;
            try
            {
                temp = myPostRepo.Get_T_PointInfoByID(pid);
                return Json(new { success = "200", data = temp });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        [HttpPost("PointInfo/Add")]
        public JsonResult AddPointInfo([FromBody] JObject param_pointinfo)
        {
            bool addstatus = false;
            try
            {
                t_pointinfo pointinfo = new t_pointinfo();
                pointinfo.gid = int.Parse(param_pointinfo["gid"].ToString());
                pointinfo.objectid = int.Parse(param_pointinfo["objectid"].ToString());
                pointinfo.pid = int.Parse(param_pointinfo["pid"].ToString());
                pointinfo.name = param_pointinfo["name"].ToString();
                pointinfo.type = int.Parse(param_pointinfo["type"].ToString());
                pointinfo.__gid = int.Parse(param_pointinfo["__gid"].ToString());
                pointinfo.id1 = int.Parse(param_pointinfo["id1"].ToString());
                pointinfo.id2 = int.Parse(param_pointinfo["id2"].ToString());
                var geom=JObject.Parse(param_pointinfo["geometry"].ToString());
                var point = geom["flatCoordinates"];
                var x = double.Parse(point[0].ToString());
                var y = double.Parse(point[1].ToString());
                pointinfo.coordinate = new double[2];
                pointinfo.coordinate[0] = x;
                pointinfo.coordinate[1] = y;
                addstatus = myPostRepo.AddSingle_T_PointInfo(pointinfo);

                return Json(new { success = "200", status = addstatus,data= pointinfo });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", status = addstatus, error = ex.ToString() });
            }
        }
        [HttpPost("PointInfo/Set")]
        public JsonResult UpdatePointInfo([FromBody] JObject param_pointinfo)
        {
            bool updatestatus = false;
            try
            {
                t_pointinfo pointinfo = new t_pointinfo();
                pointinfo.objectid = int.Parse(param_pointinfo["objectid"].ToString());
                pointinfo.pid = int.Parse(param_pointinfo["pid"].ToString());
                pointinfo.name = param_pointinfo["name"].ToString();
                pointinfo.type = int.Parse(param_pointinfo["type"].ToString());
                pointinfo.__gid = int.Parse(param_pointinfo["__gid"].ToString());
                pointinfo.id1 = int.Parse(param_pointinfo["id1"].ToString());
                pointinfo.id2 = int.Parse(param_pointinfo["id2"].ToString());
                var geom = JObject.Parse(param_pointinfo["geometry"].ToString());
                var point = geom["flatCoordinates"];
             
                var x = double.Parse(point[0].ToString());
                var y = double.Parse(point[1].ToString());
                pointinfo.coordinate = new double[2];
                pointinfo.coordinate[0] = x;
                pointinfo.coordinate[1] = y;
                updatestatus = myPostRepo.Update_T_PointInfo(pointinfo);
                return Json(new { success = "200", status = updatestatus, data = updatestatus });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", status = updatestatus, error = ex.ToString() });
            }
        }
        [HttpDelete("PointInfo/{pid}")]
        public JsonResult DeletePointInfo(string pid)
        {

            bool delstatus = false;
            try
            {
                delstatus = myPostRepo.Delete_T_PointInfoByID(pid);
                return Json(new { success = "200", data = delstatus });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        #endregion

        #region 计算指标结果表
        //Query
        [HttpGet, Route("AllLineTarget")]
        public JsonResult GetAllLineTargetResults()
        {
            // return null ;
            try
            {
                var obj = myPostRepo.GetAll_T_LineNumber();
                return Json(new { success = "200", data = obj });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }

        }
        //Get bus/spatial/guid
        [HttpPost("LineTarget")]
        public JsonResult GetSigleLineTargetByID([FromBody] JObject param_target)
        {

            try
            {
                string gid = param_target["gid"].ToString();
                string cid = param_target["cid"].ToString();
                var temp = myPostRepo.GetSingle_T_LineNumber(gid, cid);
                return Json(new { success = "200", data = temp });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        [HttpGet, Route("AllAreaTarget")]
        public JsonResult GetAllAreaTargetResults()
        {
            // return null ;
            try
            {
                var obj = myPostRepo.GetT_DivisionNumberAll();
                return Json(new { success = "200", data = obj });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }

        }
        //Get bus/spatial/guid
        [HttpGet("AreaTarget/{gid}")]
        public JsonResult GetAreaTargetByID(string gid)
        {
            try
            {
                var temp = myPostRepo.GetSingle_T_DivisionNumber(gid);
                return Json(new { success = "200", data = temp });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        //Add

        //Json 前端传入结构
        // {  
        // "lineguid":"1234567",
        // "averagelength" : "0.23",
        // "buslinecount" :20,
        // "bendrate" : 0.6,
        // "c_lineguid":"12345678",
        // "coincidence":0.4
        //}
        // {
        //      "gid":123,
        //    "linelength":23.5,
        //        "linedensity":34.4,
        //        "roadcover":23.4,
        //        "buslinecount":34,
        //        "buslinelength":20.4,
        //        "buslinedensity":12.2,
        //        "stopcount":21,
        //        "changecount":10,
        //        "cover300":12.4,
        //        "cover500":45.6,
        //        "cover600":20.1,
        //        "stationcount":10,
        //        "stationarea":10.4,
        //        "repaircount":1,
        //};
        //线指标结果
        [HttpPost("LineTarget/Add")]
        public JsonResult AddLineTargetResult([FromBody] JObject param_target)
        {
            bool addstatus = false;
            try
            {
                t_linenumber linetarget = new t_linenumber();
                linetarget.lineguid = param_target["lineguid"].ToString();
                linetarget.averagelength = float.Parse(param_target["averagelength"].ToString());
                linetarget.buslinecount = int.Parse(param_target["buslinecount"].ToString());
                linetarget.bendrate = float.Parse(param_target["bendrate"].ToString());
                linetarget.c_lineguid = param_target["c_lineguid"].ToString();
                linetarget.coincidence = float.Parse(param_target["coincidence"].ToString());
                linetarget.createtime = DateTime.Now;
                addstatus = myPostRepo.AddSingle_T_LineNumber(linetarget);
                return Json(new { success = "200", data = addstatus });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        //区域指标结果
        [HttpPost("AreaTarget/Add")]
        public JsonResult AddAreaTargetResult([FromBody] JObject param_target)
        {
            bool addstatus = false;
            try
            {
                t_divisionnumber areatarget = new t_divisionnumber();
                areatarget.gid = int.Parse(param_target["gid"].ToString());
                areatarget.linelength = float.Parse(param_target["linelength"].ToString());
                areatarget.linedensity = float.Parse(param_target["linedensity"].ToString()); ;
                areatarget.roadcover = float.Parse(param_target["roadcover"].ToString());
                areatarget.buslinecount = int.Parse(param_target["buslinecount"].ToString());
                areatarget.buslinelength = float.Parse(param_target["buslinelength"].ToString());
                areatarget.buslinedensity = float.Parse(param_target["buslinedensity"].ToString());
                areatarget.stopcount = int.Parse(param_target["stopcount"].ToString());
                areatarget.changecount = int.Parse(param_target["changecount"].ToString());
                areatarget.cover300 = float.Parse(param_target["cover300"].ToString());
                areatarget.cover500 = float.Parse(param_target["cover500"].ToString());
                areatarget.cover600 = float.Parse(param_target["cover600"].ToString());
                areatarget.stationcount = int.Parse(param_target["stationcount"].ToString());
                areatarget.stationarea = float.Parse(param_target["stationarea"].ToString());
                areatarget.repaircount = int.Parse(param_target["repaircount"].ToString());
                areatarget.createtime = DateTime.Now;
                addstatus = myPostRepo.AddSingle_T_DivisionNumber(areatarget, "t_divisionnumber");

                return Json(new { success = "200", data = addstatus });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        #endregion

        #region 方案信息表
        //查询全部方案信息
        [HttpGet, Route("AllPlans")]
        public JsonResult GetAllPlanInfoResults()
        {
            // return null ;
            try
            {
                IEnumerable<t_plan_line> queryall= myPostRepo.GetPlanInfoAllTable();
                return Json(new { success = "200", data = queryall });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }

        }

        [HttpPost, Route("PlanInfo/Query")]
        public JsonResult QueryPlanInfos([FromBody] JObject param_plan)
        {
            try
            {
                IEnumerable<t_plan_line> temp = null;
                string express = param_plan["express"].ToString();
                temp = myPostRepo.Query_T_PlanInfo(express);
                return Json(new { success = "200", data = temp });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        //Get bus/spatial/guid
        [HttpGet, Route("PlanInfo/{planid}")]
        public JsonResult GetSiglePlanInfotByID(string planid)
        {
            try
            {
                var temp = myPostRepo.GetSingle_T_PlanInfo(planid);
                return Json(new { success = "200", data = temp });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        //{
        //    "planname":"计划不如变化快",
        //    "type":0,
        //   "linename":"",
        //   "creator":"user",
        //    "lineguid":""
        //}
        //添加方案信息  是否和线路一起添加？
        [HttpPost("PlanInfo/Add")]
        public JsonResult AddPlanInfoResult([FromBody] JObject param_plan)
        {
            bool addstatus = false;
            try
            {
                t_plan_line planinfo = new t_plan_line();
                planinfo.planid = Guid.NewGuid().ToString();
                planinfo.planname = param_plan["planname"].ToString();
                planinfo.plantype = int.Parse(param_plan["type"].ToString());
                planinfo.linename = param_plan["linename"].ToString();
                planinfo.lineguid = param_plan["lineguid"].ToString();
                planinfo.createtime = DateTime.Now;
                planinfo.creator = param_plan["creator"].ToString();
                int direct = int.Parse(param_plan["direction"].ToString());
                addstatus = myPostRepo.AddSingle_T_PlanInfo(planinfo);
                if(planinfo.plantype==1)
                {
                    //查询lineguid的站点列表和line线路
                    bool addstatus1=myPostRepo.AddPlanPointListFromCurrentPoint(planinfo.planid,planinfo.lineguid,direct);
                    bool addstatus2= myPostRepo.AddPlanLineShapeFromCurrentLineShape(planinfo.planid, planinfo.lineguid, direct);
                    addstatus = addstatus1 && addstatus2;
                    
                }
                
                //var cur=myPostRepo.GetSingle_T_PlanInfo(planinfo.planid);
                if (addstatus)
                {
                    return Json(new { success = "200", data = planinfo });
                }
                else
                {
                    return Json(new { success = "404", data = addstatus });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        //删除方案信息  是否删除对应线路
        [HttpGet, Route("PlanInfo/Del/{planid}")]
        public JsonResult DeletePlanInfo(string planid)
        {

            bool delstatus = false;
            try
            {
                delstatus = myPostRepo.Delete_T_PlanInfo(planid);
                return Json(new { success = "200", data = delstatus });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        //更新方案信息 
        [HttpPost("PlanInfo/Set")]
        public JsonResult UpdatePlanInfo([FromBody] JObject param_plan)
        {

            bool updatestatus = false;
            try
            {
                t_plan_line planinfo = new t_plan_line();
                planinfo.planid = param_plan["planid"].ToString();
                planinfo.planname = param_plan["planname"].ToString();
                planinfo.plantype = int.Parse(param_plan["type"].ToString());
                planinfo.linename = param_plan["linename"].ToString();
                planinfo.lineguid = param_plan["lineguid"].ToString();
                planinfo.createtime = DateTime.Now;
                planinfo.creator = param_plan["creator"].ToString();
                int direct = int.Parse(param_plan["direction"].ToString());
                updatestatus = myPostRepo.Update_T_PlanInfo(planinfo);
                //var cur=myPostRepo.GetSingle_T_PlanInfo(planinfo.planid);
                if (updatestatus)
                {
                    return Json(new { success = "200", data = planinfo });
                }
                else
                {
                    return Json(new { success = "404", data = updatestatus });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        #endregion

        #region 方案站点列表
        //Get bus/spatial/guid
        [HttpGet, Route("ALLPlanPointList")]
        public JsonResult GetALLPlanPointListByID()
        {
            try
            {
                var temp = myPostRepo.GetPlanPointListAllTable();
                return Json(new { success = "200", data = temp });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        [HttpGet, Route("PlanPointList/{planid}")]
        public JsonResult GetPlanPointListByID(string planid)
        {
            try
            {
                var temp = myPostRepo.GetPlanPointListByID(planid);
                return Json(new { success = "200", data = temp });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        //{
        //   planid:"wqewqewqeqwe",
        //   points:[
        //      {"pointnumber":3,"pid":6},
        //      {"pointnumber":4,"pid":7},
        //      {"pointnumber":5,"pid":8},
        //      {"pointnumber":6,"pid":9},
        //      {"pointnumber":7,"pid":10},
        //    ]
        //}
        [HttpPost("PlanPointList/Add")]
        public JsonResult AddPlanPointList([FromBody] JObject param_pointlist)
        {
            bool addstatus = false;
            try
            {
                string planid= param_pointlist["planid"].ToString();
                List<t_planpoint_info> pointlist = new List<t_planpoint_info>();
                var points = param_pointlist["points"];
                if(points==null||points.Count()==0)
                {
                    return Json(new { success = "200", data = addstatus });
                }
                foreach (var item in points)
                {
                    t_planpoint_info point = new t_planpoint_info();
                    point.planid = planid;
                    point.pointnumber = int.Parse(item["pointnumber"].ToString());
                    point.pid = int.Parse(item["pid"].ToString());
                    pointlist.Add(point);
                }
                addstatus = myPostRepo.AddSingle_T_PointList(pointlist);
                return Json(new { success = "200", data = addstatus });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        [HttpGet, Route("PlanPointList/Del/{planid}")]
        public JsonResult DeletePlanPointList(string planid)
        {

            bool delstatus = false;
            try
            {
                delstatus = myPostRepo.Delete_T_PlanPointListByID(planid);
                return Json(new { success = "200", data = delstatus });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        [HttpPost("PlanPointList/Set")]
        public JsonResult UpdatePlanPointList([FromBody] JObject param_pointlist)
        {
            bool updatestatus = false;
            try
            {
                string planid = param_pointlist["planid"].ToString();
                List<t_planpoint_info> pointlist = new List<t_planpoint_info>();
                var points = param_pointlist["points"];
                if (points == null || points.Count() == 0)
                {
                    return Json(new { success = "200", data = updatestatus });
                }
                foreach (var item in points)
                {
                    t_planpoint_info point = new t_planpoint_info();
                    point.planid = planid;
                    point.pointnumber = int.Parse(item["pointnumber"].ToString());
                    point.pid = int.Parse(item["pid"].ToString());
                    pointlist.Add(point);
                }
                updatestatus = myPostRepo.Update_T_PlanPointListByID(planid,pointlist);
                return Json(new { success = "200", data = updatestatus });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }

        #endregion

        #region 方案线路
        //查询全部方案信息
        [HttpGet, Route("AllPlanLine")]
        public JsonResult GetAllPlanLineResults()
        {
            // return null ;
            try
            {
                var obj = myPostRepo.GetPlanLineShapeAllTable();
                return Json(new { success = "200", data = obj });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }

        }
        //Get bus/spatial/guid
        [HttpGet, Route("PlanLine/{planid}")]
        public JsonResult GetSiglePlanLineByID(string planid)
        {
            try
            {
                var temp = myPostRepo.Get_T_Plan_LineshapeByID(planid);
                return Json(new { success = "200", data = temp });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        //{
        //    "planname":"计划不如变化快",
        //    "type":0,
        //   "linename":"",
        //   "creator":"user",
        //    "lineguid":""
        //}
        //添加方案信息  是否和线路一起添加？
        [HttpPost("PlanLine/Add")]
        public JsonResult AddPlanLineList([FromBody] JObject param_planline)
        {
            bool addstatus = false;
            try
            {
                List<t_plan_lineshape> lineshapes = new List<t_plan_lineshape>();
                var lists = param_planline["lines"];
                if(lists==null||lists.Count()==0)
                {
                    return Json(new { success = "200", data = addstatus });
                }
                 
                foreach (var item in lists)
                {
                    t_plan_lineshape planline = new t_plan_lineshape();
                    planline.planid = item["planid"].ToString();
                    planline.ordernumber = int.Parse(item["ordernumber"].ToString());
                    planline.startpid = int.Parse(item["startpid"].ToString());
                    planline.endpid = int.Parse(item["endpid"].ToString());
                    planline.length = double.Parse(item["length"].ToString());
                    var geom = item["geometry"];
                    var lines = geom["flatCoordinates"];
                   
                    Coordinate[] pointlist = new Coordinate[lines.Count()/2];
                    var i = 0;
                    var j = 0;
                    foreach (var point in lines)
                    {
                        if(i%2==1)
                        {
                            var x = double.Parse(lines[i-1].ToString());
                            var y = double.Parse(lines[i].ToString());
                            Coordinate temppoint = new Coordinate(x,y);
                            pointlist[j] = temppoint;
                            j++;
                        }
                        i++;
                    }
                    

                    planline.geom = new LineString(pointlist);
                    planline.geom.SRID = 3857;
                    planline.length = planline.geom.Length;
                    lineshapes.Add(planline);
                }
                
                addstatus = myPostRepo.AddSingle_T_Plan_Lineshape(lineshapes,3857);
                return Json(new { success = "200", data = addstatus });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        //删除对应线路
        [HttpGet, Route("PlanLine/Del/{planid}")]
        public JsonResult DeletePlanLine(string planid)
        {

            bool delstatus = false;
            try
            {
                delstatus = myPostRepo.Delete_T_Plan_LineshapeByID(planid);
                return Json(new { success = "200", data = delstatus });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        [HttpPost("PlanLine/Set")]
        public JsonResult UpdatePlanLine([FromBody] JObject param_planline)
        {
            bool updatestatus = false;
            try
            {
                List<t_plan_lineshape> lineshapes = new List<t_plan_lineshape>();
                var lists = param_planline["lines"];
                var planid= param_planline["planid"].ToString();
                if (lists == null || lists.Count() == 0)
                {
                    return Json(new { success = "200", data = updatestatus });
                }

                foreach (var item in lists)
                {
                    t_plan_lineshape planline = new t_plan_lineshape();
                    planline.planid = item["planid"].ToString();
                    planline.ordernumber = int.Parse(item["ordernumber"].ToString());
                    planline.startpid = int.Parse(item["startpid"].ToString());
                    planline.endpid = int.Parse(item["endpid"].ToString());
                    planline.length = double.Parse(item["length"].ToString());
                    var geom = item["geometry"];
                    var lines = geom["flatCoordinates"];

                    Coordinate[] pointlist = new Coordinate[lines.Count() / 2];
                    var i = 0;
                    var j = 0;
                    foreach (var point in lines)
                    {
                        if (i % 2 == 1)
                        {
                            var x = double.Parse(lines[i - 1].ToString());
                            var y = double.Parse(lines[i].ToString());
                            Coordinate temppoint = new Coordinate(x, y);
                            pointlist[j] = temppoint;
                            j++;
                        }
                        i++;
                    }

                    planline.geom = new LineString(pointlist);
                    planline.length = planline.geom.Length;
                    planline.geom.SRID = 3857;
                    lineshapes.Add(planline);
                }

                updatestatus = myPostRepo.Update_T_Plan_Lineshape(planid,lineshapes,3857);
                
                return Json(new { success = "200", data = updatestatus });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        #endregion

    }
}