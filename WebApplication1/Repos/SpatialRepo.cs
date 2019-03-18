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

    }
}