using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using Npgsql;
using System.Text;
using WebApplication1.Repos;
using static System.Data.CommandType;
using WebApplication1.Models;
using NetTopologySuite.Geometries;
using GeoAPI.Geometries;

namespace WebApplication1.Repos
{
    public class SpatialRepo : IDisposable
    {
        public string connectionString { get; set; }
        public SpatialRepo()
        {
            //State is useful or not will be error
            NpgsqlConnection.GlobalTypeMapper.UseLegacyPostgis();
        }
        //come on postgis i 
        private NpgsqlConnection Connection
        {
            get
            {
                return new NpgsqlConnection(connectionString);
            }
        }
        public void Dispose()
        {
            Connection.Close();
        }

        #region 单线指标计算
        //计算线段及其间距
        public IEnumerable<t_busline_shape> GetStationsBreakLength(string lineguid, int direct)
        {
            
            IEnumerable<t_busline_shape> res = null;
            string sql = "SELECT * FROM  t_busline_shape where lineguid='" + lineguid + "' and direction=" + direct + " order by ordernumber asc";

            using (var connection = Connection)
            {
                connection.Open();
                res = connection.Query<t_busline_shape>(sql);

            }
            return res;
        }
        //计算一次换成经过的线路个数
        public int GetLineNumbersByOnce(string lineguid, int direct)
        {
            IEnumerable<t_linepoint> res = null;
            string sql = "SELECT * FROM  t_linepoint where lineguid='" + lineguid + "' and direction=" + direct + " order by ordernumber asc";
            List<int> datas = new List<int>();
            using (var connection = Connection)
            {
                connection.Open();
                res = connection.Query<t_linepoint>(sql);
                StringBuilder distinctstr = new StringBuilder();
                if(res==null||res.Count()==0)
                {
                    return 0;
                }
                foreach (t_linepoint item in res)
                {
                    datas.Add(item.pid);
                }
                //String.Join(",",datas)
                string countnum = "select   distinct lineguid  from t_linepoint where pid  in (" + String.Join(",", datas) + ")";
                //
                IEnumerable<string> lines = connection.Query<string>(countnum);
                if(lines==null)
                {
                    return 0;
                }
                return lines.Count();
            }

        }
        //计算两点间距离
        public decimal GetDistanceFromPoints(int pid_first, int pid_last)
        {
            decimal res = 0m;
            string sql = @"SELECT ST_Distance(" +
                "(select geom from t_pointinfo where pid=" + pid_first + " )::geography ," +
                "(select geom from t_pointinfo where pid=" + pid_last + " )::geography )";
            using (var con = Connection)
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(sql, con))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        object t = reader.GetValue(0);
                        Decimal.TryParse(t.ToString(),out res);
                        break;
                    }
            }
            return res;
        }
        //计算两条线路中  重复线段的长度
        public decimal IntersectionBetweenTwoLines(string lineguid_1, int direct1, string lineguid_2, int direct2)
        {
            decimal res = 0.0m;
            //string sql = "select sum(length) from t_roadcollection where rid in("
            //              + "select rid  from t_routelinemap "
            //              + "where (lineguid = '" + lineguid_1 + "' and direction = " + direct1 + ") or(lineguid = '" + lineguid_2 + "' and direction = " + direct2
            //              + ") group by rid having count(*) > 1) ";
            string sql1 =
                "select sum(length) from t_roadcollection where rid in("
                +"select C.rid from ("
                +"SELECT A.id,A.rid FROM t_routelinemap as A INNER JOIN t_routelinemap as B ON A.rid = B.rid"
                + "  where A.lineguid = '" + lineguid_1 + "' and A.direction = " + direct1 + " and B.lineguid = '" + lineguid_2 + "' and B.direction = " + direct2+" ) as C)";
            using (var con = Connection)
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(sql1, con))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())

                    {
                        object t = reader.GetValue(0);
                        
                        Decimal.TryParse(t.ToString(),out res);
                       
                        
                        break;
                    }
            }
            return res;

        }
        #endregion

        #region 单点计算指标
        //计算可换乘的公交线路数
        public IEnumerable<string> SumCrossingLines(int pid)
        {

            IEnumerable<string> lines = null;
            string sql = "select distinct lineguid from t_linepoint where pid = " + pid;
            using (var con = Connection)
            {
                con.Open();
                lines = con.Query<string>(sql);
            }
            
            return lines;
        }
        //统计可到达的站点数
        public IEnumerable<string> SumRelatedPoints(int pid, IEnumerable<string> lines)
        {
            IEnumerable<string> points = null;
            List<string> temp = new List<string>();
            string sql = "select distinct pid from t_linepoint where lineguid in (";
            lines.ToList().ForEach(x => temp.Add("'" + x.ToString() + "' "));
            string cond = String.Join(",", temp.ToList());
            using (var con = Connection)
            {
                con.Open();
                sql = sql + cond + " )";
                points = con.Query<string>(sql);
            }

            return points;
        }

        //计算 30米缓冲区可换乘的公交线路数
        public IEnumerable<string> SumCrossingLinesByBuffer(int pid, double radius)
        {

            IEnumerable<string> lines = null;
            //boolean ST_DWithin(geography gg1, geography gg2, double precision distance_meters);
            string sql = "SELECT distinct t_linepoint.lineguid FROM t_linepoint "
                       + " WHERE ST_DWithin(t_linepoint.geom::geography, (select geom from t_linepoint where pid = " + pid + " and direction = 0)::geography," + radius + ")";
            using (var con = Connection)
            {

                con.Open();
                lines = con.Query<string>(sql);
            }

            return lines;
        }
        //统计 30米缓冲区 可到达的站点数
        public IEnumerable<string> SumRelatedPointsByBuffer(int pid, IEnumerable<string> lines)
        {
            IEnumerable<string> points = null;
            List<string>  temp = new List<string>();
            string sql = "select distinct pid from t_linepoint where lineguid in (";
            lines.ToList().ForEach(x => temp.Add("'" + x.ToString() + "' "));
            string cond = String.Join(",", temp.ToList());
            using (var con = Connection)
            {
                con.Open();
                sql = sql + cond + " )";
                points = con.Query<string>(sql);
            }

            return points;
        }
        //最近路口距离
        public decimal FindNearestRoad(int pid)
        {
            decimal distance = 0.0m;

            string sql_guids = "select distinct lineguid from t_busline_shape where startpid="+pid;
            string sql_lines = "select * from t_busline_shape where lineguid in"
                    + " (select distinct lineguid from t_busline_shape where startpid="+pid+") ";//
            IEnumerable<string> lineguids = null;
            IEnumerable<t_busline_shape> lines = null;
            List<decimal> res = new List<decimal>();
            using (var con = Connection)
            {
                con.Open();
                lineguids = con.Query<string>(sql_guids);
                lines = con.Query<t_busline_shape>(sql_lines);
            }
            foreach (var item in lineguids)
            {
                //正向查找路口
                var grplines1 = from t in lines
                               where t.lineguid == item.ToString() && t.startpid >= pid
                               orderby t.ordernumber ascending
                               select t;
                var dd1 = findLinesToCross(grplines1);
                if (dd1!= -1.0m)
                {
                    res.Add(dd1);
                }
                //反向查找路口
                var grplines2 = from t in lines
                               where t.lineguid == item.ToString() && t.startpid<= pid
                               orderby t.ordernumber descending
                               select t;
                var dd2 = findLinesToCross(grplines2);
                if (dd2 != -1.0m)
                {
                    res.Add(dd2);
                }
                
            }
            //按公交线路进行分组
            distance = res.Min();
            return distance;
        }
        public decimal findLinesToCross(IOrderedEnumerable<t_busline_shape> grplines)
        {
            decimal t = 0.0m;
            List<t_busline_shape> lines = new List<t_busline_shape>();
            foreach (var item in grplines)
            {
                lines.Add(item);
                //路口节点
                if (item.etype == 2)
                {
                    t = lines.Sum<t_busline_shape>(x => x.length);
                    break;
                }// 
                if (item.etype == 1)
                {
                    t = -1.0m;
                    break;
                }
            }
            return t;
        }
        #endregion

        #region 规划线路指标计算
        public IEnumerable<t_planlineinfo> GetPlanLineInfo(string planid)
        {
            string querysql = "Select *  from t_planlineinfo where planid='"+planid+"'";
            IEnumerable<t_planlineinfo> results = null;
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                results = connection.Query<t_planlineinfo>(querysql);
                return results;
            }
             
        }
        //计算一次换成经过的线路个数
        public int GetPlanLineNumbersByOnce(IEnumerable<t_planlineinfo> planlineinfos)
        {
            IEnumerable<t_linepoint> res = null;
            List<int> datas = new List<int>();
            using (var connection = Connection)
            {
                connection.Open();
                StringBuilder distinctstr = new StringBuilder();
                if (planlineinfos == null || planlineinfos.Count() == 0)
                {
                    return 0;
                }
                foreach (t_planlineinfo item in planlineinfos)
                {
                    datas.Add(item.pid);
                }
                //String.Join(",",datas)
                string countnum = "select   distinct lineguid  from t_linepoint where pid  in (" + String.Join(",", datas) + ")";
                //
                IEnumerable<string> lines = connection.Query<string>(countnum);
                if (lines == null)
                {
                    return 0;
                }
                return lines.Count();
            }

        }
        //计算线路的重合率
        public decimal IntersectionBetweenTwoLines(string planid, string lineguid, int direct)
        {
         
            decimal res = 0.0m;
            string sql1= " (select ST_UNION(geom)  from t_plan_lineshape where planid='" + planid + "')::geography";
            string buffer = " (ST_Buffer("+sql1+",30,'endcap=square join=round'))::geography";
            string sql2= " (select ST_UNION(geom) from t_busline_shape  where lineguid='" + lineguid + "' and direction="+direct+ ")::geography";
            string Intersection = "(SELECT ST_Intersection(" +buffer+","+sql2+"))::geography";
            string length = "select ST_Length(" + Intersection + " ,false)";
            using (var con = Connection)
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(length, con))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        object t = reader.GetValue(0) ;
                        decimal.TryParse(t.ToString(), out res);
                        break;
                    }
            }
            return res;

        }
        #endregion

        #region 总体指标计算
        //code
        //线网
        public decimal GetRegionAreaById(int gid)
        {
            var area = 0.0m;
            string querysql = "Select  * from t_division where gid=" + gid;
            t_division result = null;
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                result = connection.Query<t_division>(querysql).FirstOrDefault();
                if(result!=null)
                {
                    area = result.area;
                }
            }
            return area;
        }
        public decimal ST_RoadNetLength_Region(int gid)
        {
            //select ST_Length(
            //            (
            //                select  st_intersection(
            //                    (select geom from t_division where gid = 116)::geometry,
            //           (select ST_Union(geom) from t_roadcollection) ::geometry
            //       )
            //    )::geography,false
            //)
            decimal totallength = 0.0m;
            string regiongeom = "(select geom from t_division where gid ="+gid+")::geometry";
            string unionroadline = "(select ST_Union(geom) from t_roadcollection)::geometry";
            string intersection = "(select  st_intersection(" + unionroadline +"," +regiongeom+"))::geography";
            string calsql = "select ST_Length(" + intersection + ",false)";
            //execute db sql
            using (var con = Connection)
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(calsql, con))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        object t = reader.GetValue(0);
                        decimal.TryParse(t.ToString(), out totallength);
                        break;
                    }
            }
            return totallength;
        }
        //线路
        public int ST_BusLineCount_Region(int gid)
        {
            //            select count(*)  from
            //(
            // SELECT distinct busline.lineguid AS guid
            // FROM t_busline_shape busline, (select * from t_division where gid = 116)   region
            // WHERE  st_intersects(region.geom, busline.geom) = 't'
            // ) as t
           int total = 0;
            //intersects:1 true,0 false
            string regiongeom = "(select * from t_division where gid ="+gid+") region";
            string intersects = "SELECT distinct busline.lineguid AS guid FROM t_busline_shape busline, " + regiongeom +
                    " WHERE  st_intersects(region.geom, busline.geom) = 't'";
            string calsql = "select count(*)  from ("+ intersects + ") as t";
            using (var con = Connection)
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(calsql, con))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        object t = reader.GetValue(0);
                        int.TryParse(t.ToString(), out total);
                        break;
                    }
            }
            return total;
        }
        public decimal ST_BusLineLength_Region(int gid)
        {
            decimal total = 0.0m;
            //   select ST_Length(
            //   (
            //       select st_intersection(
            //           (select geom from t_division where gid = 116)::geometry,
            //           (select ST_Union(geom) from t_busline_shape where direction = 0) ::geometry
            //       )
            //    )::geography,false
            //)
           
            string regiongeom = "(select geom from t_division where gid = "+gid+")::geometry";
            string linegeoms = "(select ST_Union(geom) from t_busline_shape where direction = 0) ::geometry";
            string intersection = "select  st_intersection(" + regiongeom + "," + linegeoms + ")";
            string calsql = "select ST_Length(("+ intersection + ")::geography,false)";
            using (var con = Connection)
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(calsql, con))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        object t = reader.GetValue(0);
                        decimal.TryParse(t.ToString(), out total);
                        break;
                    }
            }
            return total;
        }
        //中途站
        public int ST_BusStopCount_Region(int gid)
        {
            int count = 0;
            //   select count(*)  from
            //(
            // SELECT busstation.gid AS gid
            // FROM(select * from t_pointinfo where type = 0) busstation, (select * from t_division where gid = 116)   region
            // WHERE  st_intersects(busstation.geom, region.geom) = 't'
            //) as t
            string bustation = "(select * from t_pointinfo where type = 0) busstation";
            string regiongeom = " (select * from t_division where gid = "+gid+")   region";
            string intersects = " SELECT busstation.pid AS pid FROM " + bustation + "," + regiongeom
            +" WHERE  st_intersects(busstation.geom, region.geom) = 't'";
            string calsql = " select count(*)  from("+ intersects + ") as t";
            using (var con = Connection)
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(calsql, con))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        object t = reader.GetValue(0);
                        int.TryParse(t.ToString(), out count);
                        break;
                    }
            }
            return count;
        }
        public int ST_BusStopTransfer_Count(int gid)
        {
            int count = 0;
            //   select count(*)  from
            //(
            // SELECT busstation.gid AS gid
            // FROM(select * from t_pointinfo where type = 0) busstation, (select * from t_division where gid = 116)   region
            // WHERE  st_intersects(busstation.geom, region.geom) = 't'
            //) as t
            string bustation = "(select * from t_pointinfo where type = 0) busstation";
            string regiongeom = " (select * from t_division where gid ="+gid+")   region";
            string intersects = " SELECT busstation.pid AS pid FROM " + bustation + "," + regiongeom
            + " WHERE  st_intersects(busstation.geom, region.geom) = 't'";
            string calsql = " select pid  from(" + intersects + ") as t";
            using (var con = Connection)
            {
                 IEnumerable<int> pids = con.Query<int>(calsql);
                if (pids == null || pids.Count() == 0)
                {
                    count= 0;
                }
                else {
                    string pidlist = String.Join(',', pids.ToList<int>());
                    string countsql = "select count(*)  FROM  t_linepoint where pid in (" + pidlist + ") group by pid  having  count(distinct lineguid)>1";
                    count = con.Query<int>(countsql).FirstOrDefault(); 
                }
            }
            return count;
        }
        public decimal ST_BusStopCover_Region(int gid,string bufferradius)
        {
            decimal result = 0.0m;
            string regiongeom = "(select geom from t_division where gid = " + gid + ")::geometry";
            string buffer = "(select geom from "+bufferradius+") ::geometry";
            if (String.IsNullOrEmpty(bufferradius))
            {
                return result;
            }
            string intersection = "select  st_intersection(" + regiongeom + "," + buffer + ")";
            string calsql = "select ST_Area((" + intersection + ")::geography,false)";
            using (var con = Connection)
            {
                con.Open();
                var cover= con.Query<float>(calsql).FirstOrDefault();
                result = decimal.Parse(cover.ToString());
            }
            return result;
        }
        //场站Leave  To Cain
        public int ST_BusStationCount_Region(int gid)
        {
            //select st_intersects(select st_union(geom) from t_busline_shape where lineguid='EE172CA1-4C44-4AD2-A53F-966F62AD3F03' and direction=0,select distinct geom from t_division where gid=116)
            int total = 0;
            string regiongeom = "(select * from t_division where gid =" + gid + ") region";
            string intersects = "SELECT * FROM t_bus_station_polygon station, " + regiongeom +
                    " WHERE  st_intersects(region.geom, station.geom) = 't'";
            string calsql = "select count(*)  from (" + intersects + ") as t";
            using (var con = Connection)
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(calsql, con))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        object t = reader.GetValue(0);
                        int.TryParse(t.ToString(), out total);
                        break;
                    }
            }
            return total;
        }
        public decimal ST_BusStationArea_Region(int gid)
        {
            //
            //            select count(*) from(SELECT * FROM t_bus_station_polygon station, (select * from t_division where gid = 116) region WHERE  st_intersects(region.geom, station.geom) = 't') as t
            decimal area = 0.0m;
            string regiongeom = "(select * from t_division where gid =" + gid + ") region";
            string intersects = "SELECT * FROM t_bus_station_polygon station, " + regiongeom +
                    " WHERE  st_intersects(region.geom, station.geom) = 't'";
            string calsql = "select sum(t.jzmj) from (" + intersects + ") as t";
            using (var con = Connection)
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(calsql, con))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        object t = reader.GetValue(0);
                        decimal.TryParse(t.ToString(), out area);
                        break;
                    }
            }
            return area;
        }
        public int ST_BusStationRepairCount_Region(int gid)
        {
            //select round(sum(t.jzmj),2) from(SELECT * FROM t_bus_station_polygon station, (select * from t_division where gid = 116) region WHERE  st_intersects(region.geom, station.geom) = 't') as t
            //select count(*) from(SELECT * FROM t_bus_station_polygon station, (select * from t_division where gid = 116) region WHERE  st_intersects(region.geom, station.geom) = 't') as t where gn like '%修车%'
            int total = 0;
            string regiongeom = "(select * from t_division where gid =" + gid + ") region";
            string intersects = "SELECT * FROM t_bus_station_polygon station, " + regiongeom +
                    " WHERE  st_intersects(region.geom, station.geom) = 't'";
            string calsql = "select count(*)  from (" + intersects + ") as t where gn like '%修车%'";
            using (var con = Connection)
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(calsql, con))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        object t = reader.GetValue(0);
                        int.TryParse(t.ToString(), out total);
                        break;
                    }
            }
            return total;
        }
        #endregion

        #region 规划总体指标计算
        
        /*
         some functions ready for calculate Area Length Intersection Buffer
             */
        //判断排除线路是否在区域内
        public bool BusLineIntersectsRegion(int gid, string lineguid)
        {
            //            select st_intersects(
            //(select geom from t_division where gid= 116157),
            //(select st_union(geom) from t_busline_shape where
            //         lineguid = 'EE172CA1-4C44-4AD2-A53F-966F62AD3F03' and direction = 0)
            //	)
            bool isInner = false;
            string lines = "(select st_union(geom) from t_busline_shape where lineguid='"+lineguid+"' and direction=0)::geometry";
            string region = "(select geom from t_division where gid=" + gid+")::geometry";
            string st_intersects = " select st_intersects(" + lines + "," + region + ")";
            using (var con = Connection)
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(st_intersects, con))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        object t = reader.GetValue(0);
                        isInner = bool.TryParse(t.ToString(), out isInner);
                        break;
                    }
            }
            return isInner;
        }
        //判断规划线路是否在区域内
        public bool PlanLineIntersectsRegion(int gid, string planid)
        {
            bool isInner = false;
            string lines = "(select st_union(geom) from t_plan_lineshape where planid='" + planid + "')::geometry";
            string region = "(select geom from t_division where gid=" + gid+")::geometry";
            string st_intersects = " select st_intersects(" + lines + "," + region + ")";
            using (var con = Connection)
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(st_intersects, con))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        object t = reader.GetValue(0);
                        isInner = bool.TryParse(t.ToString(), out isInner);
                        break;
                    }
            }
            return isInner;
           
        }
        //区域内裁剪的排除线路长度
        public decimal BuelineRegionClipLength(string lineguid,int gid)
        {
            decimal len = 0.0m;
            string lines = "(select st_linemerge(st_union(geom)) from t_busline_shape where lineguid='" + lineguid + "' and direction=0)::geometry";
            string region = "(select geom from t_division where gid=" + gid+")::geometry";
            string innergeom = "select st_intersection(" + lines + "," + region + ")";
            string st_length= "select ST_Length((" + innergeom + ")::geography,false)";
            using (var con = Connection)
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(st_length, con))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        object t = reader.GetValue(0);
                        decimal.TryParse(t.ToString(), out len);
                    }
            }
            return len;
        }
        //区域内裁剪的规划线路长度
        public decimal PlanlineRegionClipLength(string planid,int gid)
        {
            decimal len = 0.0m;
            string lines = "(select st_linemerge(st_union(geom)) from t_plan_lineshape where planid='" + planid + "' )::geometry";
            string region = "(select geom from t_division where gid=" + gid+")::geometry";
            string innergeom = "select st_intersection(" + lines + "," + region + ")";
            string st_length = "select ST_Length((" + innergeom + ")::geography,false)";
            using (var con = Connection)
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(st_length, con))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        object t = reader.GetValue(0);
                        decimal.TryParse(t.ToString(), out len);
                    }
            }
            return len;
        }
        //区域内  路网线路和规划线路重叠的部分线段的长度
        public decimal PlanLineClipAfterOverlapLength(string planid,int gid)
        {
            decimal len = 0.0m;
            //区域截取的规划线段集合
            string planline = "(select st_linemerge(st_union(geom)) from t_plan_lineshape where planid='" + planid + "')::geometry";
            string region = "(select geom from t_division where gid=" + gid+ ")::geometry";
            string planlineInner = "select st_intersection(" + planline + "," + region + ")";
            string planline_bufferInner = "( SELECT ST_Buffer( ("+ planlineInner +")::geography, 30, 'endcap=flat join=round') )";
            //区域内线网集合
            string regiongeom = "(select geom from t_division where gid =" + gid + ")::geometry";
            string roadlinenet = "(select st_linemerge(st_union(geom)) from t_roadcollection)::geometry";
            string roadlineInner = "(select  st_intersection(" + roadlinenet + "," + regiongeom + ") )";
            //规划线段集合和线网线段集合的buffer重叠部分的长度
            string repeateIntersects= "select  st_intersection(" + roadlineInner + "," + planline_bufferInner + ")";
            string st_length = "select ST_Length((" + repeateIntersects + ")::geography,false)";
            using (var con = Connection)
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(st_length, con))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        object t = reader.GetValue(0);
                        decimal.TryParse(t.ToString(), out len);
                    }
            }
            return len;
            
        }
        
        //线网
        public decimal ST_Plan_RoadNetLength_Region(string lineguid,string planid,int gid)
        {
            decimal netlength = 0.0m;
            var currentnetlen = ST_RoadNetLength_Region(gid);
            var buslineremovelen = BuelineRegionClipLength(lineguid,gid);
            var plancliplen = PlanlineRegionClipLength(planid,gid);
            var overlapplanlen = PlanLineClipAfterOverlapLength(planid,gid);
            netlength = currentnetlen - buslineremovelen + (plancliplen - overlapplanlen);
            return netlength;
        }
        //线路
        //Leave to Cain
        public int ST_PlanBusLineCount_Region(int gid, String planid, String lineguid)
        {
            int planBusLineNum = 0;
            int removeBusLineNum = 0;
            int total = 0;
            var busLineCount = ST_BusLineCount_Region(gid);
            var removeFlag = BusLineIntersectsRegion(gid, lineguid);
            if (removeFlag)
            {
                removeBusLineNum = 1;
            }
            else
            {
                removeBusLineNum = 0;
            }
            var planFlag = PlanLineIntersectsRegion(gid, planid);
            if (planFlag)
            {
                planBusLineNum = 1;
            }
            else
            {
                planBusLineNum = 0;
            }
            total = busLineCount + planBusLineNum - removeBusLineNum;
            return total;
        }
        public decimal BusLineIntersectionRegion(int gid, string lineguid)
        {
            //select st_length ((select st_intersection((select st_union(geom) from t_busline_shape where lineguid='EE172CA1-4C44-4AD2-A53F-966F62AD3F03' and direction=0),(select geom from t_division where gid=116))))
            decimal isInnerLength = 0.0m;
            string lines = "(select st_union(geom) from t_busline_shape where lineguid='" + lineguid + "' and direction=0)";
            string region = "(select geom from t_division where gid=" + gid + ")";
            string st_intersection = "(select st_intersection(" + lines + "," + region + "))";
            string st_length = "select st_length(" + st_intersection + ")";
            using (var con = Connection)
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(st_length, con))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        object t = reader.GetValue(0);
                        decimal.TryParse(t.ToString(), out isInnerLength);
                        break;
                    }
            }
            return isInnerLength;
        }
        public decimal PlanLineIntersectionRegion(int gid, string planid)
        {
            //select st_length ((select st_intersection((select st_union(geom) from t_plan_lineshape where planid='42e5510d-f669-4bec-a78a-697ac66b6561'),(select geom from t_division where gid=116))))
            decimal isInnerLength = 0.0m;
            string lines = "(select st_union(geom) from t_plan_lineshape where planid='" + planid + "')";
            string region = "(select geom from t_division where gid=" + gid + ")";
            string st_intersection = "(select st_intersection(" + lines + "," + region + "))";
            string st_length = "select st_length(" + st_intersection + ")";
            using (var con = Connection)
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(st_length, con))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        object t = reader.GetValue(0);
                        decimal.TryParse(t.ToString(), out isInnerLength);
                        break;
                    }
            }
            return isInnerLength;
        }
        public decimal ST_PlanBusLineLength_Region(int gid, String planid, String lineguid)
        {
            decimal countLength = 0.0m;
            decimal nowLength = 0.0m;
            decimal removeLength = 0.0m;
            decimal planLength = 0.0m;
            nowLength = ST_BusLineLength_Region(gid);
            removeLength = BusLineIntersectionRegion(gid, lineguid);
            planLength = PlanLineIntersectionRegion(gid, planid);
            countLength = nowLength + planLength - removeLength;
            return countLength;
        }
        //中途站
        public int ST_Plan_BusStopCount_Region(string lineguid, string planid, int gid)
        {
            //select pid  from t_line_plan_pointlist 
            //where lineid = '7fbe92e6-5ad0-4353-ba99-3bd4213fe0ba' and ltype = '1'  and ptype = '3'
            //select* from t_pointinfo
            //where(type = 0 and(pid not in (4, 5))) or pid in(205)
            int count = 0;
            string buslinePids = "select distinct pid  from t_line_plan_pointlist" 
                  +" where lineid = '"+lineguid+"' and ltype = '0' and ptype = '0' ";
            string planlinePids = "select pid  from t_line_plan_pointlist"
                  +" where lineid = '"+planid+"' and ltype = '1'  and ptype = '3' ";
            string inpids = "";
            string notinpids = "";
            using (var con = Connection)
            {
                con.Open();
                IEnumerable<int> pidsNotIn = con.Query<int>(buslinePids);
                if (pidsNotIn == null || pidsNotIn.Count() == 0)
                {
                    notinpids = " 1=1 ";
                }
                else
                {
                    notinpids = " (pid not in (" + String.Join(",", pidsNotIn.ToList()) + ")) " ;
                }
                IEnumerable<int> pidsIn = con.Query<int>(planlinePids);
                if (pidsIn == null || pidsIn.Count() == 0)
                {
                    inpids = " 1=2 ";
                }
                else
                {
                    inpids = " pid  in (" + String.Join(",", pidsIn.ToList()) + ") ";
                }
                //
                string collectPids = "select pid from t_pointinfo " +" where (type = 0 and "+ notinpids + " ) or "+ inpids;
                IEnumerable<int> interpids = con.Query<int>(collectPids);
                if (interpids == null || interpids.Count() == 0)
                {
                    /*不直接是0是应为 规划的pid集合 中可能包括了 新增自定义点pid（type=3） 和原始站点（type=0）,d
                     *现在函数查询的SQL中并没有把规划线路的所有pid取出，而只是获取了增量的pid即 type=3的那些点
                     */
                    count = ST_BusStationCount_Region(gid);
                    return count;
                }
                else
                {
                    string bustation = "(select * from t_pointinfo where pid in("+String.Join(',',interpids)+")) busstation ";
                    string regiongeom = " (select * from t_division where gid = " + gid + ")   region";
                    string intersects = " SELECT busstation.pid AS pid FROM " + bustation + "," + regiongeom
                    + " WHERE  st_intersects(busstation.geom, region.geom) = 't' ";
                    string calsql = " select count(*)  from(" + intersects + ") as t";
                    count = con.Query<int>(calsql).FirstOrDefault<int>();
                    return count;
                }
            }
        }
        public int ST_Plan_BusStopTransfer_Count(string lineguid, string planid, int gid)
        {
            int count = 0;
         
            string buslinePids = "select distinct pid  from t_line_plan_pointlist "
                  + "where lineid = '" + lineguid + "' and ltype = '0' and ptype = '0' ";
            string planlinePids = "select pid  from t_line_plan_pointlist "
                  + "where lineid = '" + planid + "' and ltype = '1'  and ptype = '3' ";
            string inpids = "";
            string notinpids = "";
            using (var con = Connection)
            {
                con.Open();
                IEnumerable<int> pidsNotIn = con.Query<int>(buslinePids);
                if (pidsNotIn == null || pidsNotIn.Count() == 0)
                {
                    notinpids = " 1=1 ";
                }
                else
                {
                    notinpids = " (pid not in (" + String.Join(",", pidsNotIn) + ")) ";
                }
                IEnumerable<int> pidsIn = con.Query<int>(planlinePids);
                if (pidsIn == null || pidsIn.Count() == 0)
                {
                    inpids = " 1=2 ";
                }
                else
                {
                    inpids = " pid  in (" + String.Join(",", pidsIn) + ") ";
                }
                //
                string collectPids = "select pid from t_pointinfo " + " where(type = 0 and " + notinpids + " ) or " + inpids;
                IEnumerable<int> interpids = con.Query<int>(collectPids);
                if (interpids == null || interpids.Count() == 0)
                {
                    /*不直接是0是应为 规划的pid集合 中可能包括了 新增自定义点pid（type=3） 和原始站点（type=0）,d
                   *现在函数查询的SQL中并没有把规划线路的所有pid取出，而只是获取了增量的pid即 type=3的那些点
                   */
                    count =  ST_BusStopTransfer_Count(gid);
                    return count;
                }
                else
                {
                    string bustation = "(select * from t_pointinfo where pid in(" + String.Join(',', interpids) + ")) busstation ";
                    string regiongeom = " (select * from t_division where gid = " + gid + ")   region ";
                    string intersects = " SELECT busstation.pid AS pid FROM " + bustation + "," + regiongeom
                    + " WHERE  st_intersects(busstation.geom, region.geom) = 't' ";
                    string calsql = " select pid  from(" + intersects + ") as t";
                    IEnumerable<int> temppids= con.Query<int>(calsql);
                    //
                    if (temppids == null || temppids.Count() == 0)
                    {
                        return count;
                    }
                    else
                    {
                        string pidlist = String.Join(',', temppids.ToList<int>());
                        string countsql = "select count(*)  FROM t_line_plan_pointlist where pid in (" +pidlist + ") group by pid  having  count(distinct lineid)>1";
                        count = con.Query<int>(countsql).FirstOrDefault();
                         
                    }
                    return count;
                }
            }
          
        }
        public decimal ST_BusStopCover_Region(string planid,int gid,string bufferradius,int radius)
        {
            //此处并没有去减去 排除线路站点的缓冲区面积（PM认为不用减），而是增加了规划线路站点缓冲区的面积
            decimal result = 0.0m;
            string planpids="select pid  from t_line_plan_pointlist "
                   + "where lineid = '" + planid + "' and ltype = '1' and ptype = '3' ";
            if (String.IsNullOrEmpty(bufferradius))
            {
                return result;
            }
            using (var con = Connection)
            {
                con.Open();
                IEnumerable<int> planpidlist = con.Query<int>(planpids);
                if (planpidlist == null || planpidlist.Count() == 0)
                {
                    result = ST_BusStopCover_Region(gid, bufferradius);
                    return result;
                }
                else
                {
                    //规划线路上新增的那些自定义点的缓冲区面geom
                    string pointbuffer = "(select ST_Buffer( (ST_Union(geom))::geography," + radius+ ",'quad_segs=8') "
                                    +" from t_pointinfo where pid in (" + String.Join(',',planpidlist)+") )::geometry";
                    //现状站点缓冲区融合面300 500 600的表
                    string bufferlayer = "(select geom from " + bufferradius + ") ::geometry";
                    //把现状和规划的缓冲区合并
                    string unionarea= " (select st_union( "+pointbuffer+","+bufferlayer+") )::geometry";
                    //区域图形geom
                    string regiongeom = "(select geom from t_division where gid = " + gid + ")::geometry";
                    //区域面和新的缓冲区面Intersection
                    string intersection = "select  st_intersection(" + regiongeom + "," + unionarea + ")";
                    string calsql = " select ST_Area((" + intersection + ")::geography,false)";
                    
                    using (var cmd = new NpgsqlCommand(calsql, con))
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read())
                        {
                            object t = reader.GetValue(0);
                            decimal.TryParse(t.ToString(), out result);
                        }
                }
            }
            return result;
             
        }

        #endregion

    }
}