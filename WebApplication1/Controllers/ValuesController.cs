using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApplication1.Models;
using WebApplication1.Repos;
namespace WebApplication1.Controllers
{
    [Route("bus/[controller]")]

    public class ValuesController : Controller
    {
        public DbConfig dbcofig;
        public DbReo myRepo = new DbReo();
       
        public ValuesController( IOptions<DbConfig> config)
        {
            dbcofig = config.Value;
            myRepo.connectionString = dbcofig.DefaultConnection;
            
        }
        // GET api/values
        [HttpGet]
        public JsonResult Get()
        {
            return Json(new { success = "200", data = "Ok"});
        }

       
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
         
        [HttpGet, Route("GetAllInfos")]
        public JsonResult GetAllData()
        {
            List<DivisionNumber> temp = null;
            try
            {
                temp = myRepo.GetAll_T_DivisionNumber();
                return Json(new { success = "200", data = temp });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }


        }

        [HttpGet("{gid}")]
        public JsonResult GetSigleDataByID(int gid)
        {
            DivisionNumber temp = null;
            try
            {
                temp = myRepo.GetSingle_T_DivisionNumber(gid);
                return Json(new { success = "200", data = temp });
            }
            catch (Exception ex)
            {
                return Json(new { success = "404", error = ex.ToString() });
            }
        }
        
        //Move to PostDB controller 
        //[HttpGet,Route("GetBusLines")]
        //public JsonResult GetAllBusLine()
        //{
        //    List<BusLine> temp = null;
        //    try
        //    {
        //        temp = myRepo.GetAll_T_BusLine();
        //        return Json(new { success = "200", data = temp });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = "404", error = ex.ToString() });
        //    }
        //}
        //[HttpGet, Route("GetOrganizeInfos")]
        //public JsonResult GetOrganizeInfo()
        //{
        //    List<OrganizeInfo> temp = null;
        //    try
        //    {
        //        temp = myRepo.GetAll_T_OrganizeInfo();
        //        return Json(new { success = "200", data = temp });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = "404", error = ex.ToString() });
        //    }
        //}

    }
}
