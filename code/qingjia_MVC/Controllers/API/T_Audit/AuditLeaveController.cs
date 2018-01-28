using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using qingjia_MVC.Models.API;
using qingjia_MVC.Models;

namespace qingjia_MVC.Controllers.API.T_Audit
{
    [RoutePrefix("api/audit")]
    public class AuditLeaveController : BaseApiController
    {
        //此模块包含 获取请假记录数据、审批通过、审批驳回、多条件查询接口
        [HttpGet, Route("get")]
        public ApiResult Get(string access_token, string leaveType)
        {
            IQueryable<vw_LeaveList> leaveList = db.vw_LeaveList;
            SelectCondition conditionsModel = new SelectCondition();
            conditionsModel.conditions = null;
            conditionsModel.sortDirection = "DESC";
            conditionsModel.sortField = "ST_Tel";
            return Success("获取成功", GetList(conditionsModel, leaveList));
        }

        [HttpGet, Route("get")]
        public ApiResult Get(string access_token, string leaveType, string limit, string page)
        {

            return result;
        }

        [HttpGet, Route("get")]
        public ApiResult Get(string access_token, string leaveType, string limit, string page, string sortDirection, string sortField)
        {

            return result;
        }
    }
}