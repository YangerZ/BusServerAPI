using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using WebApplication1.Models;
namespace WebApplication1.Repos
{
    public class DbReo
    {

       //配置文件连接
        public string connectionString { get; set; }
        //程序真实表
        public List<DivisionNumber> GetAll_T_DivisionNumber()
        {
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                string sql = "Select * from T_DivisionNumber";
                var query = connection.Query<DivisionNumber>(sql);
                return query.ToList();
            }
        }
        public DivisionNumber GetSingle_T_DivisionNumber(int id)
        {
            StringBuilder Sqlstr = new StringBuilder();
            Sqlstr.Append(" Select * from T_DivisionNumber where GID="+id);
            
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                var query = connection.Query<DivisionNumber>(Sqlstr.ToString()).SingleOrDefault();
                return query;
            }
        }
        public List<BusLine> GetAll_T_BusLine()
        {
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                Console.WriteLine(connection.State);
                string sql = "SELECT  [Lineguid],[Name],[IsDefault],[Type],[Direction],[First],[Last],[Interval],[Mileage],[Time],[parentguid],[company] FROM [WebBus].[dbo].[T_BusLine]";
                var query = connection.Query<BusLine>(sql);
                return query.ToList();
            }
        }
        public List<OrganizeInfo> GetAll_T_OrganizeInfo()
        {
            using (IDbConnection connection = new SqlConnection(connectionString))
            {

                string rootsql = "SELECT  [id],[Name] as Label,[parentguid],[Blevel],[OrderNumber] FROM [WebBus].[dbo].[T_OrganizeInfo] Where Blevel=1 order by [OrderNumber]";
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
            IEnumerable <OrganizeInfo> childs= null;
            var p_id = p_organ.id.ToString();
            string sql = "SELECT  [id],[Name] as Label,[parentguid],[Blevel],[OrderNumber] FROM [WebBus].[dbo].[T_OrganizeInfo] Where parentguid='" + p_id + "' order by [OrderNumber]";
            
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                childs = connection.Query<OrganizeInfo>(sql);
                p_organ.children = childs;
            }
            if(childs==null||childs.Count()==0)
            {
                return;
            }
            foreach (var item in childs)
            {
                if(item!=null)
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


    }
}
