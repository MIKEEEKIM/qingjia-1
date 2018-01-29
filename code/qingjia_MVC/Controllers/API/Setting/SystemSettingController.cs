using qingjia_MVC.Models;
using qingjia_MVC.Models.API;
using qingjia_MVC.Models.API.Setting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Z.EntityFramework.Plus;


namespace qingjia_MVC.Controllers.API.Setting
{
    [RoutePrefix("api/system")]
    public class SystemSettingController : BaseApiController
    {
        //此模块包含 请假类型设置、

        #region 请假类型设置
        /// <summary>
        /// 获取 请假类型设置情况
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpGet, Route("getleavetype")]
        public ApiResult GetLeaveType(string access_token)
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
                AccountInfo accountInfo = GetAccountInfo(access_token);
                IQueryable<vw_TeacherLeaveType> leaveList = db.vw_TeacherLeaveType;
                SelectCondition conditionsModel = new SelectCondition();
                ConditionModel condition = CreatCondition("TeacherID", accountInfo.userID);
                conditionsModel.conditions.Add(condition);
                return Success("获取成功", GetList(conditionsModel, leaveList));
            }
            catch
            {
                return SystemError();
            }
            #endregion
        }

        /// <summary>
        /// 获取所有请假类型
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpGet, Route("leavetype")]
        public ApiResult LeaveType(string access_token)
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
                AccountInfo accountInfo = GetAccountInfo(access_token);
                IQueryable<T_LeaveType> leaveList = db.T_LeaveType;
                SelectCondition conditionsModel = new SelectCondition();
                conditionsModel.conditions = null;
                return Success("获取成功", GetList(conditionsModel, leaveList));
            }
            catch
            {
                return SystemError();
            }
            #endregion
        }

        /// <summary>
        /// 设置所使用的请假类型
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("setleavetype")]
        public ApiResult SetLeaveType([FromBody] SetLeaveType model)
        {
            #region 令牌验证
            result = Check(model.access_token);
            if (result != null)
            {
                return result;
            }
            #endregion

            #region 逻辑操作
            try
            {
                if (model.leaveTypeIdList == null)
                {
                    return Error("leaveTypeIdList参数错误");
                }
                AccountInfo accountInfo = GetAccountInfo(model.access_token);

                //批量修改 T_TeacherLeaveType.Enable 字段
                db.T_TeacherLeaveType.Where(q => q.TeacherID == accountInfo.access_token).Update(q => new T_TeacherLeaveType() { Enable = 0 });
                List<T_TeacherLeaveType> list = new List<T_TeacherLeaveType>();
                foreach (string _item in model.leaveTypeIdList)
                {
                    int item = Convert.ToInt32(_item);
                    if (db.T_LeaveType.Find(item) != null)
                    {
                        list.Add(new T_TeacherLeaveType() { LeaveTypeID = item, TeacherID = accountInfo.userID, Enable = 1 });
                    }
                }
                db.T_TeacherLeaveType.AddRange(list);
                db.SaveChanges();
                return Success("修改成功！");
            }
            catch
            {
                return SystemError();
            }
            #endregion
        }
        #endregion
    }
}