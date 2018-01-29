using qingjia_MVC.Models;
using qingjia_MVC.Models.API;
using System.Linq;
using System.Web.Http;

namespace qingjia_MVC.Controllers.API.Audit
{
    [RoutePrefix("api/audit")]
    public class AuditLeaveController : BaseApiController
    {
        #region 令牌验证
        //result = Check(access_token);
        //if (result != null)
        //{
        //    return result;
        //}
        #endregion

        #region 逻辑操作
        //try
        //{
        //
        //}
        //catch
        //{
        //    return SystemError();
        //}
        #endregion

        //此模块包含 获取请假记录数据、审批通过、审批驳回、多条件查询接口
        [HttpGet, Route("get")]
        public ApiResult Get(string access_token, string leaveType)
        {
            #region 令牌验证
            result = Check(access_token);
            if (result != null)
            {
                return result;
            }
            #endregion

            #region 逻辑操作
            try
            {
                IQueryable<vw_LeaveList> leaveList = db.vw_LeaveList;
                SelectCondition conditionsModel = new SelectCondition();
                conditionsModel.conditions = null;
                conditionsModel.sortDirection = "DESC";
                conditionsModel.sortField = "ST_Tel";
                return Success("获取成功", GetList(conditionsModel, leaveList));
            }
            catch
            {
                return SystemError();
            }
            #endregion
        }

        [HttpGet, Route("get")]
        public ApiResult Get(string access_token, string leaveType, string limit, string page)
        {
            #region 令牌验证
            result = Check(access_token);
            if (result != null)
            {
                return result;
            }
            #endregion

            #region 逻辑操作
            try
            {
                return null;
            }
            catch
            {
                return SystemError();
            }
            #endregion
        }

        [HttpGet, Route("get")]
        public ApiResult Get(string access_token, string leaveType, string limit, string page, string sortDirection, string sortField)
        {
            #region 令牌验证
            result = Check(access_token);
            if (result != null)
            {
                return result;
            }
            #endregion

            #region 逻辑操作
            try
            {
                return null;
            }
            catch
            {
                return SystemError();
            }
            #endregion
        }
    }
}