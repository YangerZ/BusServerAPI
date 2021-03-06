﻿using System;
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
        #region 指标计算预处理
        [HttpGet, Route("Target/Create")]
        public JsonResult CalculateLineTarget_Pre()
        {
            try
            {
                DateTime dt = DateTime.Now;
                IEnumerable<t_busline_shape> distinctlines = mySpatialRepo.GetAll_T_BusLine_Shape();
                //mySpatialRepo.Delete_T_Division_BusLine("t_divisionnumber");只追加不删除
                foreach (var item in distinctlines)
                {
                    //返回值
                    string lineguid = item.lineguid;
                    int direct = 0;
                    //线段距离 站间距
                    var lines = mySpatialRepo.GetStationsBreakLength(lineguid, direct);
                    if (lines == null || lines.Count() == 0)
                    {//若线路未找到对应 shape rid  各结果返回0
                        t_linenumber linetarget1 = new t_linenumber();
                        linetarget1.lineguid = lineguid;
                        linetarget1.averagelength =0;
                        linetarget1.buslinecount = 0;
                        linetarget1.bendrate = 0;
                        linetarget1.c_lineguid = "";
                        linetarget1.coincidence = 0;
                        linetarget1.createtime = dt;
                        linetarget1.totallength =0;
                        linetarget1.stationcount =0;

                        linetarget1.department = 0;
                        linetarget1.school = 0;
                        linetarget1.hospital = 0;
                        linetarget1.community = 0;
                        linetarget1.commerce = 0;
                        linetarget1.scenicspot = 0;

                        mySpatialRepo.AddSingle_T_LineNumber(linetarget1);
                        continue;
                    }
                    //站点间线段总个数
                    decimal total = lines.Sum(d => d.length);
                    //站点数
                    int stationcount = mySpatialRepo.Query_StationNums(lineguid, direct);
                    //平均站间距
                    decimal average = total / (stationcount-1);
                    //1次换乘到达的线路数
                    var crossnums = mySpatialRepo.GetLineNumbersByOnce(lineguid, direct);
                    //非直线系数   空间直线距离/线段路线长度
                    decimal distance = mySpatialRepo.GetDistanceFromPoints(lines.First().startpid, lines.Last().endpid);

                    decimal coefficient = 0m;
                    if (distance != 0m)
                    {
                        coefficient = total / distance;
                    }
                    


                    IEnumerable<facilitygrp> fac_grps = mySpatialRepo.GetFacilityCountByLineBuffer(lineguid,500);
                    //线段重合率
                    //decimal repeatlength = mySpatialRepo.IntersectionBetweenTwoLines(lineguid, direct, lineguid2, direct2);
                    //decimal repeatRatio = repeatlength / total;
                    //需要记录数据
                    t_linenumber linetarget = new t_linenumber();
                    linetarget.lineguid = lineguid;
                    linetarget.averagelength = float.Parse(average.ToString());
                    linetarget.buslinecount = int.Parse(crossnums.ToString());
                    linetarget.bendrate = float.Parse(coefficient.ToString());
                    linetarget.c_lineguid = "";
                    linetarget.coincidence = 0;
                    linetarget.createtime = dt;
                    linetarget.totallength = float.Parse(total.ToString());
                    linetarget.stationcount = stationcount;
                    //缓冲区计算设施个数
                    linetarget.department = 0;
                    linetarget.school = 0;
                    linetarget.hospital = 0;
                    linetarget.community = 0;
                    linetarget.commerce = 0;
                    linetarget.scenicspot = 0;
                    if (fac_grps!=null&&fac_grps.Count()>0)
                    {
                        foreach (var fac in fac_grps)
                        {
                            if (fac.type == 0)
                            {
                                linetarget.department = fac.count;
                            }
                            if (fac.type == 1)
                            {
                                linetarget.school = fac.count;
                            }
                            if (fac.type == 2)
                            {
                                linetarget.hospital = fac.count;
                            }
                            if (fac.type == 3)
                            {
                                linetarget.community = fac.count;
                            }
                            if (fac.type == 4)
                            {
                                linetarget.commerce = fac.count;
                            }
                            if (fac.type == 5)
                            {
                                linetarget.scenicspot = fac.count;
                            }

                        }
                    }
                    mySpatialRepo.AddSingle_T_LineNumber(linetarget);
                }

                return Json(new
                {
                    success = "200",
                    data = true

                });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }

        #endregion

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
                int stationcount = mySpatialRepo.Query_StationNums(lineguid, direct);
                //平均站间距
                decimal average = total / (stationcount-1);
                //1次换乘到达的线路数
                var crossnums = mySpatialRepo.GetLineNumbersByOnce(lineguid, direct);
                //非直线系数   空间直线距离/线段路线长度
                decimal distance = mySpatialRepo.GetDistanceFromPoints(lines.First().startpid, lines.Last().endpid);
                decimal coefficient = total/distance;
                //线段重合率
                decimal repeatlength = mySpatialRepo.IntersectionBetweenTwoLines(lineguid, direct, lineguid2, direct2);
                decimal repeatRatio = repeatlength / total;
                //back 
                return Json(new
                {
                    success = "200",
                    data = new { pjzjj = average, khcxls = crossnums, fzxxs = coefficient, chcd= repeatlength,xdchl = repeatRatio }

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
                //站点间线段总个数  注意这个地方 应为线路中没有道路路口打断线段的干扰  所以每个线段就是间距
                decimal total =decimal.Parse(lines.Sum(d => d.length).ToString());
                //平均站间距
                decimal average = total / lines.Count();
                //1次换乘到达的线路数
                var crossnums = mySpatialRepo.GetPlanLineNumbersByOnce(lines);
                //非直线系数   空间直线距离/线段路线长度
                decimal distance = mySpatialRepo.GetDistanceFromPoints(lines.First().startpid, lines.Last().endpid);
                decimal coefficient = total/distance;
                //线段重合率
                decimal repeatlength = mySpatialRepo.IntersectionBetweenTwoLines(planid,lineguid,direct);
                decimal repeatRatio = repeatlength / total;
                //back 
                return Json(new
                {
                    success = "200",
                    data = new {lineinfos=lines, pjzjj = average, khcxls = crossnums, fzxxs = coefficient, chcd = repeatlength, xdchl = repeatRatio }

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