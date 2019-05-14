using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using System.Data;
using Npgsql;
using System.Text;
using static System.Data.CommandType;
using WebApplication1.Models;
using System.Transactions;
using GeoAPI.Geometries;
using WebApplication1.ParamsObj;
using NetTopologySuite.Geometries;

namespace WebApplication1.Repos
{
    public class PostDBRepo:IDisposable
    {
        public  string connectionString { get; set; }
        public PostDBRepo()
        {
            //State is useful or not will be error
            //NpgsqlConnection.GlobalTypeMapper.UseLegacyPostgis();
            NpgsqlConnection.GlobalTypeMapper.UseNetTopologySuite();
             
            //Connection.TypeMapper.UseLegacyPostgis();

        }
        //come on postgis i 
 
        #region demo model
        //Area
        public IEnumerable<Area> GetAreaAllTable()
        {
            StringBuilder tsb = new StringBuilder();
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    IEnumerable<Area> query = connection.Query<Area>("select * from Area");
                    return query;
                }
            }
            catch(Exception ex)
            {
                return null;
            }
           
        }
        public Area GetSingle_T_Area(int id)
        {
            string querysql = " Select * from Area where gid=" + id;

            using (NpgsqlConnection connection =new NpgsqlConnection(connectionString))
            {
                 
                var query = connection.Query<Area>(querysql).SingleOrDefault();
                return query;
            }
        }
        public bool AddSingle_T_Area(Area newArea)
        {
            string insertsql = "INSERT INTO area(gid,objectid,sqmi,sqkm,geom) VALUES(@gid,@objectid,@sqmi,@sqkm,@geom)";
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                 
               DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@gid", newArea.gid);
                parameters.Add("@objectid", newArea.objectid);
                parameters.Add("@continent", newArea.continent);
                parameters.Add("@sqmi", newArea.sqmi);
                parameters.Add("@sqkm", newArea.sqkm);
                parameters.Add("@geom", newArea.geom);
                SqlMapper.Execute(connection, insertsql, parameters, null, null, Text);
                return true;
            }
        }
        public bool Delete_T_Area(int id)
        {
            string querysql = " Delete  from Area where gid=" + id;

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                connection.Execute(querysql);
                return true;
            }
        }
        public bool Update_T_Area(Area newArea)
        {
            string updatesql = "UPDATE area SET  objectid=@objectid,continent=@continent,sqmi=@sqmi,sqkm=@sqkm,geom=@geom" +
                " WHERE gid =@gid";
            bool state = false;
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@gid", newArea.gid);
                parameters.Add("@objectid", newArea.objectid);
                parameters.Add("@continent", newArea.continent);
                parameters.Add("@sqmi", newArea.sqmi);
                parameters.Add("@sqkm", newArea.sqkm);
                parameters.Add("@geom", newArea.geom);
                SqlMapper.Execute(connection, updatesql, parameters, null, null, Text);
                state= true;
            }
            return state;
        }
        //T_LinePoint 站点和公交线路的关系表
        public object  GetLinePointAllTable1()
        {
            //string sql = "SELECT * FROM  t_linepoint";
            //Npgsql.LegacyPostgis.PostgisPoint re =null;
            //using (var con=Connection)
            //{
            //    con.Open();
            //    using (var cmd = new NpgsqlCommand(sql, con))
            //    using (var reader = cmd.ExecuteReader())
            //        while (reader.Read())
            //        {
            //            re= reader.GetValue(6) as Npgsql.LegacyPostgis.PostgisPoint;
            //            break;
            //        }
            //}

            //return re;
            return null;
        }
        public IEnumerable<t_linepoint>  GetLinePointAllTable()
        {
            string sql= "SELECT * FROM  t_linepoint";
            
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var value = connection.Query<t_linepoint>(sql);
                return value;
            }
        }
        public t_linepoint GetSingle_T_LinePoint(string id)
        {
            string querysql = "Select * from t_linepoint where lineguid='" + id+"' ";
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                var query = connection.Query<t_linepoint>(querysql).SingleOrDefault();
                return query;
            }
        }
        public bool AddSingle_T_LinePoint(t_linepoint newLinePoint)
        {
            string insertsql = "INSERT INTO T_LinePoint(Lineguid,OrderNumber,Direction,PID,Name,LineName,geom) VALUES(@Lineguid,@OrderNumber,@Direction,@PID,@Name,@LineName,@geom)";
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                DynamicParameters parameters = new DynamicParameters();
                //parameters.Add("@Lineguid", newLinePoint.Lineguid);
                //parameters.Add("@OrderNumber", newLinePoint.OrderNumber);
                //parameters.Add("@Direction", newLinePoint.Direction);
                //parameters.Add("@PID", newLinePoint.PID);
                //parameters.Add("@Name", newLinePoint.Name);
                //parameters.Add("@LineName", newLinePoint.LineName);
                //parameters.Add("@geom", newLinePoint.geom);
                SqlMapper.Execute(connection, insertsql, parameters, null, null, Text);
                return true;
            }
        }
        public bool Delete_T_LinePoint(string id)
        {
            string querysql = " Delete  from T_LinePoint where Lineguid='" + id+"'";
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Execute(querysql);
                return true;
            }
        }
        public bool Update_T_LinePoint(t_linepoint newLinePoint)
        {
            string updatesql = "UPDATE T_LinePoint SET  OrderNumber=@OrderNumber,Direction=@Direction,PID=@PID,Name=@Name,LineName=@LineName,geom=@geom" +
                " WHERE Lineguid =@Lineguid";
            bool state = false;
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                DynamicParameters parameters = new DynamicParameters();
                SqlMapper.Execute(connection, updatesql, parameters, null, null, Text);
                state = true;
            }
            return state;
        }
        #endregion

        #region  用户信息CRUD 
        //userinfo
        public IEnumerable<t_userinfo> GetUserAllTable()
        {
            StringBuilder tsb = new StringBuilder();
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    IEnumerable<t_userinfo> query = connection.Query<t_userinfo>("select * from userinfo");
                    return query;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public t_userinfo GetSingle_T_UserInfo(string  userid)
        {
            string querysql = " Select * from userinfo where userid='" + userid+"'";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {

                var query = connection.Query<t_userinfo>(querysql).SingleOrDefault();
                return query;
            }
        }
        public t_userinfo GetSingle_T_UserLogin(string username,string password)
        {
            string querysql = " Select * from userinfo where username='" + username + "' and password='"+password+"'";
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                var query = connection.Query<t_userinfo>(querysql).SingleOrDefault();
                return query;
            }
        }
        public bool AddSingle_T_UserInfo(t_userinfo user)
        {
            string insertsql = "INSERT INTO userinfo(username,password,duty,userid,role,realname,other) VALUES(@username,@password,@duty,@userid,@role,@realname,@other,@guestid,@func)";
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@username", user.username);
                parameters.Add("@password", user.password);
                parameters.Add("@duty", user.duty);
                parameters.Add("@userid", user.userid);
                parameters.Add("@role", user.role);
                parameters.Add("@realname", user.realname);
                parameters.Add("@other", user.other);
                parameters.Add("@guestid", user.guestid);
                parameters.Add("@func", user.func);
                SqlMapper.Execute(connection, insertsql, parameters, null, null, Text);
                return true;
            }
        }
        public bool Delete_T_UserInfo(string userid)
        {
            string querysql = " Delete  from userinfo where userid='" + userid+"'";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                connection.Execute(querysql);
                return true;
            }
        }
        public bool Update_T_UserInfo(t_userinfo userinfo)
        {
            string updatesql = "UPDATE userinfo SET  username=@username,password=@password,duty=@duty,role=@role,realname=@realname,other=@other,guestid=@guestid,func=@func" +
                " WHERE userid =@userid";
            bool state = false;
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@userid", userinfo.userid);
                parameters.Add("@username", userinfo.username);
                parameters.Add("@password", userinfo.password);
                parameters.Add("@duty", userinfo.duty);
                parameters.Add("@role", userinfo.role);
                parameters.Add("@realname", userinfo.realname);
                parameters.Add("@other", userinfo.other);
                parameters.Add("@guestid", userinfo.guestid);
                parameters.Add("@func", userinfo.func);
                SqlMapper.Execute(connection, updatesql, parameters, null, null, Text);
                state = true;
            }
            return state;
        }
        public bool CheckUserNameEnable(string username)
        {
            string querysql = "select * from userinfo where username='" + username + "'";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                var query = connection.Query<t_userinfo>(querysql).SingleOrDefault();
                if (query == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion

        #region 角色功能关系
        public List<func> GetAll_T_FuncInfo(string rolename)
        {
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {

                string rootsql = "select * from func where role like '%"+rolename+"%' and pid='-1'";
                var rootnode = connection.Query<func>(rootsql);
                foreach (var item in rootnode)
                {
                    GetFuncInfosChildrens(item,rolename);
                }
                return rootnode.ToList();
            }
        }
        //查找所有子节点的任务
        private void GetFuncInfosChildrens(func p_func,string rolename)
        {
            IEnumerable<func> childs = null;
            var p_id = p_func.num.ToString();

            string sql = "SELECT * FROM func  Where pid='" + p_id + "' and role like '%"+ rolename + "%' order by num ";
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                childs = connection.Query<func>(sql);
                p_func.children = childs;
            }
            if (childs == null || childs.Count() == 0)
            {
                return;
            }
            return;

        }
        #endregion

        //t_busline
        public List<BusLine> GetAll_T_BusLine()
        {
            using (IDbConnection connection =  new NpgsqlConnection(connectionString))
            {
                string sql = "SELECT  lineguid,name,isdefault,type,direction,first,last,interval,mileage,time,parentguid,company FROM t_busLine"+
                    " where t_busline.lineguid in (select lineguid from t_routelinemap)";
                var query = connection.Query<BusLine>(sql);
                return query.ToList();
            }
        }
        public List<BusLine> Query_T_BusLine(string express)
        {
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                string sql = "SELECT  lineguid,name,isdefault,type,direction,first,last,interval,mileage,time,parentguid,company FROM t_busLine "+express;
                var query = connection.Query<BusLine>(sql);
                return query.ToList();
            }
        }
        //t_organizationinfo
        public List<OrganizeInfo> GetAll_T_OrganizeInfo()
        {
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {

                string rootsql = "SELECT  id,name as Label,parentguid,blevel,ordernumber FROM t_organizeInfo Where blevel=1 order by ordernumber";
                var rootnode = connection.Query<OrganizeInfo>(rootsql);
                foreach (var item in rootnode)
                {
                    GetOrganizeInfosChildrens(item);
                }
                return rootnode.ToList();
            }
        }
        //查找所有子节点的任务
        private void GetOrganizeInfosChildrens(OrganizeInfo p_organ)
        {
            IEnumerable<OrganizeInfo> childs = null;
            var p_id = p_organ.id.ToString();
            string sql = "SELECT id,name as Label,parentguid,blevel,ordernumber FROM t_organizeInfo  Where parentguid='" + p_id + "' order by ordernumber";
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                childs = connection.Query<OrganizeInfo>(sql);
                p_organ.children = childs;
            }
            if (childs == null || childs.Count() == 0)
            {
                return;
            }
            foreach (var item in childs)
            {
                if (item != null)
                {
                    GetOrganizeInfosChildrens(item);
                }
                else
                {
                    break;
                }

            }
            return;

        }
        
        //Results t_linenumber 线指标结果 DB操作
        public IEnumerable<t_linenumber> GetAll_T_LineNumber()
        {
            string sql = "SELECT * FROM  t_linenumber";

            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var value = connection.Query<t_linenumber>(sql);
                return value;
            }
        }
        public t_linenumber GetSingle_T_LineNumber(string guid,string c_guid)
        {
            string querysql = "Select * from t_linenumber where lineguid='" + guid + "' and c_lineguid='"+ c_guid + "'";
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                var query = connection.Query<t_linenumber>(querysql).SingleOrDefault();
                return query;
            }
        }
        public bool AddSingle_T_LineNumber(t_linenumber newLineResult)
        {
            string insertsql = "INSERT INTO t_linenumber(lineguid,averagelength,buslinecount,bendrate,c_lineguid,coincidence,createtime) " +
                "VALUES(@lineguid,@averagelength,@buslinecount,@bendrate,@c_lineguid,@coincidence,@createtime)";
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@lineguid", newLineResult.lineguid);
                parameters.Add("@averagelength", newLineResult.averagelength);
                parameters.Add("@buslinecount", newLineResult.buslinecount);
                parameters.Add("@bendrate", newLineResult.bendrate);
                parameters.Add("@c_lineguid", newLineResult.c_lineguid);
                parameters.Add("@coincidence", newLineResult.coincidence);
                parameters.Add("@createtime", newLineResult.createtime);
                SqlMapper.Execute(connection, insertsql, parameters, null, null, Text);
                return true;
            }
        }
        public bool AddMulti_T_LineNumber(IEnumerable<t_linenumber> newLineResult,string targetTable)
        {
            string insertsql = "INSERT INTO "+ targetTable + "(lineguid,averagelength,buslinecount,bendrate,c_lineguid,coincidence,createtime) " +
                "VALUES(@lineguid,@averagelength,@buslinecount,@bendrate,@c_lineguid,@coincidence,@createtime)";
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var transactionScope = new TransactionScope())
                {
                    //批量注入
                    int r = SqlMapper.Execute(connection, insertsql, newLineResult, null, null, Text);
                    //roll back automatically! awesome!
                    transactionScope.Complete();
                    return true;
                }

            }
        }

        //Results t_divisionnumber 区域指标结果 DB操作
        public IEnumerable<t_divisionnumber> GetT_DivisionNumberAll()
        {
            string sql = "SELECT * FROM  t_divisionnumber";

            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var value = connection.Query<t_divisionnumber>(sql);
                return value;
            }
        }
        public t_divisionnumber GetSingle_T_DivisionNumber(string id)
        {
            string querysql = "Select * from t_divisionnumber where gid='" + id + "'";
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                var query = connection.Query<t_divisionnumber>(querysql).SingleOrDefault();
                return query;
            }
        }
        public bool AddSingle_T_DivisionNumber(t_divisionnumber newAreaResult, string targetTable)
        {
            string insertsql = "INSERT INTO " + targetTable + "(gid,linelength,linedensity,roadcover,buslinecount,buslinelength,buslinedensity,stopcount,changecount,cover300,cover500,cover600,stationcount,stationarea,repaircount,createtime) " +
                "VALUES(@gid,@linelength,@linedensity,@roadcover,@buslinecount,@buslinelength,@buslinedensity,@stopcount,@changecount,@cover300,@cover500,@cover600,@stationcount,@stationarea,@repaircount,@createtime)";
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@gid", newAreaResult.gid);
                parameters.Add("@linelength", newAreaResult.linelength);
                parameters.Add("@linedensity", newAreaResult.linedensity);
                parameters.Add("@roadcover", newAreaResult.roadcover);
                parameters.Add("@buslinecount", newAreaResult.buslinecount);
                parameters.Add("@buslinelength", newAreaResult.buslinelength);
                parameters.Add("@buslinedensity", newAreaResult.buslinedensity);
                parameters.Add("@stopcount", newAreaResult.stopcount);
                parameters.Add("@changecount", newAreaResult.changecount);
                parameters.Add("@cover300", newAreaResult.cover300);
                parameters.Add("@cover500", newAreaResult.cover500);
                parameters.Add("@cover600", newAreaResult.cover600);
                parameters.Add("@stationcount", newAreaResult.stationcount);
                parameters.Add("@stationarea", newAreaResult.stationarea);
                parameters.Add("@repaircount", newAreaResult.repaircount);
                parameters.Add("@createtime", newAreaResult.createtime);
                SqlMapper.Execute(connection, insertsql, parameters, null, null, Text);
                return true;
            }
        }
        public bool AddMulti_T_DivisionNumber(IEnumerable<t_divisionnumber> newAreaResult, string targetTable)
        {
            string insertsql = "INSERT INTO " + targetTable + "(gid,linelength,linedensity,roadcover,buslinecount,buslinelength,buslinedensity,stopcount,changecount,cover300,cover500,cover600,stationcount,stationarea,repaircount,createtime) " +
                "VALUES(@gid,@linelength,@linedensity,@roadcover,@buslinecount,@buslinelength,@buslinedensity,@stopcount,@changecount,@cover300,@cover500,@cover600,@stationcount,@stationarea,@repaircount,@createtime)";
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var transactionScope = new TransactionScope())
                {
                    //批量注入
                    int r = SqlMapper.Execute(connection, insertsql, newAreaResult, null, null, Text);
                    //roll back automatically! awesome!
                    transactionScope.Complete();
                    return true;
                }

            }
        }
        //站点信息表
        //Results t_pointinfo
        public IEnumerable<t_pointinfo> Get_T_PointInfoTable()
        {
            try
            {
                IEnumerable<t_pointinfo> templist =null;
                string sql = "select * from t_pointinfo";
                using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    templist = conn.Query<t_pointinfo>(sql);
                    foreach (var item in templist)
                    {
                        var coordinate =item.geom.Coordinate;
                        item.coordinate = new double[2];
                        item.coordinate[0] = coordinate.X;
                        item.coordinate[1] = coordinate.Y;
                        item.geom =null;
                    }
                    return templist;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public t_pointinfo Get_T_PointInfoByID(int pid)
        {
            StringBuilder tsb = new StringBuilder();
            try
            {
                t_pointinfo temp = null;
                string sql = "select * from t_pointinfo where pid="+pid;
                using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    temp = conn.Query<t_pointinfo>(sql).FirstOrDefault();
                     var coordinate = temp.geom.Coordinate;
                    temp.coordinate = new double[2];
                    temp.coordinate[0] = coordinate.X;
                    temp.coordinate[1] = coordinate.Y;
                    temp.geom = null;
                    return temp;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public bool AddSingle_T_PointInfo(t_pointinfo newpointinfo)
        {
            string insertsql = "INSERT INTO t_pointinfo(gid,pid,name,type) VALUES(@gid,@pid,@name,@type)";
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                IEnumerable<int> max1= SqlMapper.Query<int>(conn, "select max(gid) from t_pointinfo");
                int gid = max1.First()+1;
                IEnumerable<int> max2 = SqlMapper.Query<int>(conn, "select max(pid) from t_pointinfo");
                int pid = max2.First() + 1;
                newpointinfo.pid = pid;
                using (var transactionScope = new TransactionScope())
                {
                    using (var cmd = new NpgsqlCommand(insertsql, conn))
                    {
                        //ST_Transform(ST_PointFromText('POINT(13076394.48160 4697320.0884)', 3857),4326);
                        cmd.Parameters.AddWithValue("@gid", gid);
                        //cmd.Parameters.AddWithValue("@objectid", newpointinfo.objectid);
                        cmd.Parameters.AddWithValue("@pid", pid);
                        cmd.Parameters.AddWithValue("@name", newpointinfo.name);
                        cmd.Parameters.AddWithValue("@type", newpointinfo.type);
                        //cmd.Parameters.AddWithValue("@__gid", newpointinfo.__gid);
                        //cmd.Parameters.AddWithValue("@id1", newpointinfo.id1);
                        //cmd.Parameters.AddWithValue("@id2", newpointinfo.id2);
                        cmd.ExecuteNonQuery();

                    }
                    using (var cmd = new NpgsqlCommand("",conn))
                    {
                        cmd.CommandText= "update t_pointinfo set geom=ST_Transform(ST_PointFromText('POINT("+ newpointinfo.coordinate[0].ToString() + " "+ newpointinfo.coordinate[1].ToString() + ")', 3857),4326) where pid="+pid;
                        cmd.ExecuteNonQuery();
                    }
                    transactionScope.Complete();
                    return true;
                }
            }
        }
        public bool Delete_T_PointInfoByID(string pid)
        {
            string querysql = " Delete  from t_pointinfo where pid='" + pid + "'";
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                conn.Execute(querysql);
                return true;
            }
        }
        public bool Update_T_PointInfo(t_pointinfo newpointinfo)
        {
            //string updatesql = "Update t_pointinfo set " +
            //    "gid=@gid,objectid=@objectid,pid=@pid,name=@name,type=@type,__gid=@__gid,id1=@id1,id2=@id2"
            //    + "  where pid=@pid,";
            string updatesql = "Update t_pointinfo set " +
                "gid=@gid,pid=@pid,name=@name,type=@type"
                + "  where pid=@pid,";
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var transactionScope = new TransactionScope())
                {
                    using (var cmd = new NpgsqlCommand(updatesql, conn))
                    {
                        cmd.Parameters.AddWithValue("@gid", newpointinfo.gid);
                        //cmd.Parameters.AddWithValue("@objectid", newpointinfo.objectid);
                        cmd.Parameters.AddWithValue("@pid", newpointinfo.pid);
                        cmd.Parameters.AddWithValue("@name", newpointinfo.name);
                        cmd.Parameters.AddWithValue("@type", newpointinfo.type);
                        //cmd.Parameters.AddWithValue("@__gid", newpointinfo.__gid);
                        //cmd.Parameters.AddWithValue("@id1", newpointinfo.id1);
                        //cmd.Parameters.AddWithValue("@id2", newpointinfo.id2);
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new NpgsqlCommand("", conn))
                    {
                        cmd.CommandText = "update t_pointinfo set geom=ST_Transform(ST_PointFromText('POINT(" + newpointinfo.coordinate[0].ToString() + " " + newpointinfo.coordinate[1].ToString() + ")', 3857),4326) where pid=" + newpointinfo.pid;
                        cmd.ExecuteNonQuery();
                    }
                    transactionScope.Complete();
                    return true;
                }

            }

        }

        //Results By View To Export Data
        //线指标导出数据 导出
        public IEnumerable<t_linenumber_exportview> Get_V_ALL_LineTarget()
        {
            string sql = "SELECT * FROM  t_linenumber_exportview";

            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var value = connection.Query<t_linenumber_exportview>(sql);
                return value;
            }
            
        }
        //区域指标导出数据 导出
        public IEnumerable<t_divisionnumber_exportview> Get_V_ALL_AreaTarget()
        {
            string sql = "SELECT * FROM  t_divisionnumber_exportview";

            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var value = connection.Query<t_divisionnumber_exportview>(sql);
                return value;
            }
            
        }

        #region 规划数据操作
        //Results T_Plan_Infos
        public IEnumerable<t_plan_line> GetPlanInfoAllTable()
        {
             
            try
            {
                string sql = "select * from t_plan_line order by createtime desc";
                using (IDbConnection connection = new NpgsqlConnection(connectionString))
                {
                    IEnumerable<t_plan_line> query = connection.Query<t_plan_line>(sql);
                    return query;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public IEnumerable<t_plan_line> Query_T_PlanInfo(string expression)
        {
            string querysql = "Select * from t_plan_line "+expression;
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                var query = connection.Query<t_plan_line>(querysql);
                return query;
            }
        }
        public t_plan_line GetSingle_T_PlanInfo(string planid)
        {
            string querysql = "Select * from t_plan_line where planid='" + planid + "'";
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                var query = connection.Query<t_plan_line>(querysql).SingleOrDefault();
                return query;
            }
        }
        public bool AddSingle_T_PlanInfo(t_plan_line newplaninfo)
        {
            string insertsql = "INSERT INTO t_plan_line(planid,planname,plantype,linename,lineguid,createtime,creator,userid) VALUES(@planid,@planname,@plantype,@linename,@lineguid,@createtime,@creator,@userid)";
            
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                SqlMapper.Execute(connection, insertsql, newplaninfo, null, null, Text);
                return true;
            }
        }
        public bool Delete_T_PlanInfo(string planid)
        {
            string querysql = " Delete  from t_plan_line where planid='" + planid+"'";

            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                connection.Execute(querysql);
                return true;
            }
        }
        public bool Update_T_PlanInfo(t_plan_line newplaninfo)
        {
            string updatesql = "UPDATE t_plan_line SET  planname=@planname,plantype=@plantype,lineName=@lineName,createtime=@createtime,creator=@creator,userid=@userid" +
                " WHERE planid =@planid";
            bool state = false;
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                SqlMapper.Execute(connection, updatesql, newplaninfo, null, null, Text);
                state = true;
            }
            return state;
        }
        //Results T_Plan_pointlist
        public IEnumerable<t_planpoint_info> GetPlanPointListAllTable()
        {
            StringBuilder tsb = new StringBuilder();
            try
            {
                string sql = "select * from t_planpointlist_info";
                using (IDbConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    IEnumerable<t_planpoint_info> query = connection.Query<t_planpoint_info>(sql);
                    return query;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public IEnumerable<t_planpoint_info> GetPlanPointListByID(string planid)
        {
            StringBuilder tsb = new StringBuilder();
            try
            {
                IEnumerable<t_planpoint_info> query = null;
                string sql = "select * from t_planpointlist_info where planid='" + planid + "'";
                using (IDbConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                     query = connection.Query<t_planpoint_info>(sql);
                    return query;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public bool AddSingle_T_PointList(List<t_planpoint_info> newplanpointlist)
        {
            string insertsql = "INSERT INTO  t_plan_pointlist (planid,pointnumber,pid) VALUES(@planid,@pointnumber,@pid)";
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
               
                using (var transactionScope = new TransactionScope())
                {
                    //批量注入
                    int r=SqlMapper.Execute(connection, insertsql, newplanpointlist, null, null, Text);
                    //roll back automatically! awesome!
                    transactionScope.Complete();
                    return true;
                }
                  
            }
        }
        public bool Delete_T_PlanPointListByID(string planid)
        {
            string querysql = " Delete  from t_plan_pointlist where planid='" + planid + "'";

            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                connection.Execute(querysql);
                return true;
            }
        }
        public bool Update_T_PlanPointListByID(string planid, List<t_planpoint_info> newplanpointlist)
        {
            string insertsql = "INSERT INTO t_plan_pointlist(planid,pointnumber,pid) VALUES(@planid,@pointnumber,@pid)";
            string querysql = " Delete  from t_plan_pointlist where planid='" + planid + "'";
            bool state = false;
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var transactionScope = new TransactionScope())
                {
                    //删除对应的全部Pointlist
                    SqlMapper.Execute(connection,querysql);
                    //重新加入新的list
                    //批量添加
                    int r = SqlMapper.Execute(connection, insertsql, newplanpointlist, null, null, Text);
                    //roll back automatically! awesome!
                    transactionScope.Complete();
                    state = true;
                }
            }
            return state;
        }
        //Results T_Plan_Lineshape
        public List<t_plan_lineshape> GetPlanLineShapeAllTable()
        {
            try
            {
                List<t_plan_lineshape> templist = new List<t_plan_lineshape>();
                string sql = "select * from t_plan_lineshape";
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    templist  = connection.Query<t_plan_lineshape>(sql).ToList();
                    List<double[]> points = new List<double[]>();
                    foreach (var item in templist)
                    {
                        var coords = item.geom.Coordinates;
                        foreach (Coordinate coord in coords)
                        {
                             
                            double[] co = new double[2];
                            co[0] = coord.X;
                            co[1] = coord.Y;
                            points.Add(co);
                        }
                        item.points = points;
                        item.geom = null;
                    }
                    return templist;
                     
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public IEnumerable<t_plan_lineshape> Get_T_Plan_LineshapeByID(string planid)
        {
            StringBuilder tsb = new StringBuilder();
            try
            {
                IEnumerable<t_plan_lineshape> templist = new List<t_plan_lineshape>();
                string sql = "select * from t_plan_lineshape where planid='" + planid + "'";
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    templist = connection.Query<t_plan_lineshape>(sql).ToList();
                    List<double[]> points = new List<double[]>();
                    foreach (var item in templist)
                    {
                        var coords = item.geom.Coordinates;
                        foreach (Coordinate coord in coords)
                        {

                            double[] co = new double[2];
                            co[0] = coord.X;
                            co[1] = coord.Y;
                            points.Add(co);
                        }
                        item.points = points;
                        item.geom = null;
                    }
                    return templist;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public bool AddSingle_T_Plan_Lineshape(List<t_plan_lineshape>   newplanlines,int srid)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                //Connection.TypeMapper.UseLegacyPostgis();
                using (var transactionScope = new TransactionScope())
                {
                    List<DynamicParameters> dynamicParameters = new List<DynamicParameters>();
                    string planid = "";
                    foreach (var item in newplanlines)
                    {
                        using (var cmd = new NpgsqlCommand("", connection))
                        {
                            t_plan_lineshape temp = item;
                            planid = item.planid;
                            StringBuilder coorstr = new StringBuilder();
                            Coordinate[] cc = item.geom.Coordinates;
                            cc= cc.Distinct().ToArray<Coordinate>();
                            string[] ts = new string[cc.Count()];
                            int i = 0;
                            foreach (Coordinate tempcoor in cc)
                            {
                                ts[i] = tempcoor.X.ToString() + " " + tempcoor.Y.ToString();
                                i++;
                            }
                            string sqlparam = String.Join(',', ts);
                            string geoms = "ST_GeomFromText('LineString(" + sqlparam + ")'," + srid.ToString() + ") ";
                            if (srid != 4326)
                            {
                                geoms = "ST_Transform(ST_GeomFromText('LineString(" + sqlparam + ")'," + srid.ToString() + "),4326)";
                            }
                           
                            string insertsql = "INSERT INTO t_plan_lineshape(planid,ordernumber,startpid,endpid,length,geom) " +
                                "VALUES('"+ item.planid + "',"+item.ordernumber+","+item.startpid+","+item.endpid+","+item.length+","+ geoms + ")";
                            cmd.CommandText = insertsql;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    string update = "update t_plan_lineshape set length=ST_Length(geom::geography,false)    where planid='" + planid + "'";
                    using (var cmd = new NpgsqlCommand(update, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    transactionScope.Complete();
                    return true;
                }
                  
            }
        }
        public bool Delete_T_Plan_LineshapeByID(string planid)
        {
            string querysql = " Delete  from t_plan_lineshape where planid='" + planid + "'";
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            
            {
                connection.Open();
                connection.Execute(querysql);
                return true;
            }
        }
        public bool Update_T_Plan_Lineshape(string planid, List<t_plan_lineshape> newplanlines,int srid)
        {
            string delsql = " Delete  from t_plan_lineshape where planid='" + planid + "'";
            string insertsql = "INSERT INTO t_plan_lineshape(planid,ordernumber,startpid,endpid,length,geom) VALUES(@planid,@ordernumber,@startpid,@endpid,@length,@geom)";
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var transactionScope = new TransactionScope())
                {
                    //Delete
                    SqlMapper.Execute(connection, delsql);
                    //Add
                    List<DynamicParameters> dynamicParameters = new List<DynamicParameters>();
                    foreach (var item in newplanlines)
                    {
                        using (var cmd = new NpgsqlCommand(insertsql, connection))
                        {
                            t_plan_lineshape temp = item;
                            planid = item.planid;
                            StringBuilder coorstr = new StringBuilder();
                            Coordinate[] cc = item.geom.Coordinates;
                            string[] ts = new string[cc.Count()];
                            int i = 0;
                            foreach (Coordinate tempcoor in cc)
                            {
                                ts[i] = tempcoor.X.ToString() + " " + tempcoor.Y.ToString();
                                i++;
                            }
                            string sqlparam = String.Join(',', ts);
                            string geoms = "ST_GeomFromText('LineString(" + sqlparam + ")'," + srid.ToString() + ") ";
                            if (srid != 4326)
                            {
                                geoms = "ST_Transform(ST_GeomFromText('LineString(" + sqlparam + ")'," + srid.ToString() + "),4326)";
                            }

                            string updatesql = "INSERT INTO t_plan_lineshape(planid,ordernumber,startpid,endpid,length,geom) " +
                                "VALUES('" + item.planid + "'," + item.ordernumber + "," + item.startpid + "," + item.endpid + "," + item.length + "," + geoms + ")";
                            cmd.CommandText = updatesql;
                            cmd.ExecuteNonQuery();
                        }
                        
                    }
                    string update = "update t_plan_lineshape set length=ST_Length(geom::geography,false)    where planid='" + planid + "'";
                    using (var cmd = new NpgsqlCommand(update, connection))
                    {
                        cmd.CommandText = update;
                        cmd.ExecuteNonQuery();
                    }
                    transactionScope.Complete();
                    return true;
                }
            }
        }
        //Add 调整的时候
        //获现状数据取线路对应的站点信息
        public IEnumerable<t_linepoint> Get_T_LinePoint(string id, int direct)
        {
            string querysql = "Select distinct pid,ordernumber,id,lineguid,direction,name,linename from t_linepoint where lineguid='" + id + "' and direction="+ direct + " order by ordernumber asc";
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                IEnumerable <t_linepoint> query= connection.Query<t_linepoint>(querysql);
                return query;
            }
        }
        //从现有站点列表查询然后插入到规划站点列表
        public bool AddPlanPointListFromCurrentPoint(string planid, string lineguid, int direct)
        {
            //planid = "wewewe";
            //lineguid = "EE172CA1-4C44-4AD2-A53F-966F62AD3F03";
            //direct = 0;
            IEnumerable<t_linepoint> linepoints = Get_T_LinePoint(lineguid, direct);
            if (linepoints == null || linepoints.Count() == 0)
            {
                return true;
            }
            List<t_planpoint_info> newplanpointlist = new List<t_planpoint_info>();
            var i = 0;
            foreach (var item in linepoints)
            {
                t_planpoint_info t = new t_planpoint_info();
                t.pid = item.pid;
                t.planid = planid;
                t.name = item.name;
                
                i++;
                t.pointnumber = i;
                newplanpointlist.Add(t);
            }
            bool add = AddSingle_T_PointList(newplanpointlist);
            return add;
        }
        //从现有线路列表查询操作并插入到规划线路列表
        public bool AddPlanLineShapeFromCurrentLineShape(string planid, string lineguid, int direct)
        {
            bool add = false;
            //planid = "";
            //lineguid = "EE172CA1-4C44-4AD2-A53F-966F62AD3F03";
            //direct = 0;
            int start = 0;
            int end = 0;
            string querysql = "SELECT  distinct pid,  * from  t_linepoint"
                             + " where lineguid = '" + lineguid + "' and direction = " + direct + "  order by t_linepoint.ordernumber asc";
            IEnumerable<t_linepoint> linepoints = null;
            List<int[]> listquery = new List<int[]>();
            //查询t_linepoint表  把相邻两行站点间的顺序号和站点号作为一组参数
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                //查询到所有站点和线路的顺序表
                linepoints = connection.Query<t_linepoint>(querysql);
                if(linepoints==null||linepoints.Count()==0)
                {
                    return true;
                }
                //ordernumber pid
                //组成查询数据
                IEnumerator<t_linepoint> myenumerate = linepoints.GetEnumerator();
                myenumerate.MoveNext();
               t_linepoint temp=myenumerate.Current;
               int i = 0;
                while (myenumerate.MoveNext())
                {
                    i++;
                    t_linepoint current = myenumerate.Current;
                    if(current==null)
                    {
                        break;
                    }
                    int order_start = current.ordernumber;
                    int startpid = current.pid;
                    //开始记录参数
                        int[] obj = new int[4];
                        obj[0] = temp.ordernumber; //Order number
                        obj[1] = current.ordernumber; 
                        obj[2] = temp.pid;//Pid
                        obj[3] = current.pid ;
                        listquery.Add(obj);
                    temp = current;
                   
                }
                 
                
            }
            //使用上步的站点号和顺序号参数组来查询,计算长度,UnionLine线
            //然后调用添加计划线路方法添加到计划的表中
            List<t_plan_lineshape> templist = new List<t_plan_lineshape>();
            List<Coordinate[]> temppoits = new List<Coordinate[]>();
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                if (listquery == null || listquery.Count() == 0)
                {
                    return false;
                }
                int k = 0;
                foreach (int[] item in listquery)
                {
                    start = item[0];
                    end = item[1];
                    string unionsql = "Select sum(t.length)as length ,ST_LineMerge(ST_UNION(t.geom))  as geom " +
               "from  (SELECT  distinct startpid ,* FROM t_busline_shape" +
               " where lineguid = '" + lineguid + "' and direction = " + direct + "  and(ordernumber >= " + start + " and ordernumber < " + end + ") ) as t";
                    unionline uline = connection.Query<unionline>(unionsql).FirstOrDefault();
                    t_plan_lineshape templine = new t_plan_lineshape();
                    k++;
                    templine.ordernumber =k;
                    templine.planid = planid;
                    templine.startpid = item[2];
                    templine.endpid = item[3];
                    templine.length = uline.length;
                    Coordinate[] temp = uline.geom.Coordinates;
                    temppoits.Add(temp);
                    if (uline.geom.GeometryType == "LineString")
                    {
                        templine.geom = uline.geom as LineString;
                    }
                    if (uline.geom.GeometryType == "MultiLineString")
                    {
                        templine.geom =new  LineString(uline.geom.Coordinates);
                    }
                    
                    templine.geom.SRID = uline.geom.SRID;
                    templist.Add(templine);

                }
                add = AddSingle_T_Plan_Lineshape(templist,4326);
                return add;
            }
        }

        #endregion
        #region t_division_busline
        public bool AddBusLinesWithAreaID(IEnumerable<t_division_busline> divisionbuslines)
        {
            string insertsql = "INSERT INTO t_division_busline(gid,lineguid,rid) VALUES(@gid,@lineguid,@rid)";
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var transactionScope = new TransactionScope())
                {
                    //批量注入
                    int r = SqlMapper.Execute(connection, insertsql, divisionbuslines, null, null, Text);
                    //roll back automatically! awesome!
                    transactionScope.Complete();
                    return true;
                }

            }
           
        }
        public bool Delete_T_Division_BusLine()
        {
            string querysql = " Delete  from  t_division_busline";
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                conn.Execute(querysql);
                return true;
            }
        }
        public IEnumerable<t_division_busline> GetBusLineFromDB(int gid)
        {
            IEnumerable<t_division_busline> buslines = null;
            //string sql = "select "+gid+ " as gid,lineguid,ST_Force_2D(ST_Multi(geom)) as geom from t_busline_shape where lineguid in"
            //    + "(select distinct lineguid   from t_routelinemap where rid  in"
            //    +"(select  rid from t_roadcollection  where ST_Intersects(geom,"
            //         +"(select geom from t_division where gid= "+gid+")::geometry)) )and direction = 0";

            string sql = "  select " + gid + " as gid,lineguid,rid from t_routelinemap where lineguid in" +
                           "(select distinct lineguid   from t_routelinemap where rid  in" +
                           "(select  rid from t_roadcollection  where ST_Intersects(geom," +
                           "(select geom from t_division where gid = " + gid + ")::geometry))) and direction = 0";
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                buslines = connection.Query<t_division_busline>(sql);
                
            }
            return buslines;
        }
        public IEnumerable<t_division> Get_T_Division()
        {
            IEnumerable<t_division> division = null;
            string querysql = "select * from t_division";
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                division = connection.Query<t_division>(querysql);
            }
            return division;
        }
        public IEnumerable<tempdivision> Get_T_Temp()
        {
            IEnumerable<tempdivision> division = null;
            string querysql = "select * from temp";
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                division = connection.Query<tempdivision>(querysql);
            }
            return division;
        }
        public IEnumerable<t_division_busline> GetBusLineFromDBTemp(int gid)
        {
            IEnumerable<t_division_busline> buslines = null;
            //string sql = "select "+gid+ " as gid,lineguid,ST_Force_2D(ST_Multi(geom)) as geom from t_busline_shape where lineguid in"
            //    + "(select distinct lineguid   from t_routelinemap where rid  in"
            //    +"(select  rid from t_roadcollection  where ST_Intersects(geom,"
            //         +"(select geom from t_division where gid= "+gid+")::geometry)) )and direction = 0";

            string sql = "  select " + gid + " as gid,lineguid,rid from t_routelinemap where lineguid in" +
                           "(select distinct lineguid   from t_routelinemap where rid  in" +
                           "(select  rid from t_roadcollection  where ST_Intersects(geom," +
                           "(select geom from temp where gid = " + gid + ")::geometry))) and direction = 0";
           
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                buslines = connection.Query<t_division_busline>(sql);

            }
            return buslines;
        }
        #endregion
        public void Dispose()
        {
            
        }
    }
}
