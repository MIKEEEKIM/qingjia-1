using qingjia_MVC.Models;
using qingjia_MVC.Models.API;
using System.Linq;
using System.Web.Http;
using qingjia_MVC.Common;
using System.Threading;

namespace qingjia_MVC.Controllers.API.Audit
{
    [RoutePrefix("api/auditleave")]
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
        public ApiResult Get(string access_token, string leaveTypeID)
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
                if (leaveTypeID == null || leaveTypeID == "")
                {
                    return Error("leaveTypeID参数错误");
                }

                AccountInfo accountInfo = GetAccountInfo(access_token);
                IQueryable<vw_LeaveList> leaveList = db.vw_LeaveList;
                SelectCondition conditionsModel = new SelectCondition();
                conditionsModel.conditions.Add(CreatCondition("ST_Grade", accountInfo.Grade));
                conditionsModel.conditions.Add(CreatCondition("ST_TeacherID", accountInfo.userID));
                conditionsModel.conditions.Add(CreatCondition("IsDelete", "0"));
                conditionsModel.conditions.Add(CreatCondition("LeaveType", leaveTypeID));
                conditionsModel.sortDirection = "DSEC";
                conditionsModel.sortField = "SubmitTime";
                return Success("获取成功", GetList(conditionsModel, leaveList));
            }
            catch
            {
                return SystemError();
            }
            #endregion
        }

        [HttpGet, Route("get")]
        public ApiResult Get(string access_token, string leaveTypeID, int pageSize, int page)
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
                if (leaveTypeID == null || leaveTypeID == "")
                {
                    return Error("leaveTypeID参数错误");
                }

                AccountInfo accountInfo = GetAccountInfo(access_token);
                IQueryable<vw_LeaveList> leaveList = db.vw_LeaveList;
                SelectCondition conditionsModel = new SelectCondition();
                conditionsModel.conditions.Add(CreatCondition("ST_Grade", accountInfo.Grade));
                conditionsModel.conditions.Add(CreatCondition("ST_TeacherID", accountInfo.userID));
                conditionsModel.conditions.Add(CreatCondition("IsDelete", "0"));
                conditionsModel.conditions.Add(CreatCondition("LeaveType", leaveTypeID));
                conditionsModel.sortDirection = "DSEC";
                conditionsModel.sortField = "SubmitTime";
                conditionsModel.pageSize = pageSize;
                conditionsModel.page = page;
                return Success("获取成功", GetList(conditionsModel, leaveList));
            }
            catch
            {
                return SystemError();
            }
            #endregion
        }

        [HttpGet, Route("get")]
        public ApiResult Get(string access_token, string leaveTypeID, int pageSize, int page, string sortDirection, string sortField)
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
                if (leaveTypeID == null || leaveTypeID == "")
                {
                    return Error("leaveTypeID参数错误");
                }
                if (sortDirection == null || sortDirection == "")
                {
                    return Error("sortDirection参数错误");
                }
                if (sortField == null || sortField == "")
                {
                    return Error("sortField参数错误");
                }

                AccountInfo accountInfo = GetAccountInfo(access_token);
                IQueryable<vw_LeaveList> leaveList = db.vw_LeaveList;
                SelectCondition conditionsModel = new SelectCondition();
                conditionsModel.conditions.Add(CreatCondition("ST_Grade", accountInfo.Grade));
                conditionsModel.conditions.Add(CreatCondition("ST_TeacherID", accountInfo.userID));
                conditionsModel.conditions.Add(CreatCondition("IsDelete", "0"));
                conditionsModel.conditions.Add(CreatCondition("LeaveType", leaveTypeID));
                conditionsModel.sortDirection = sortDirection;
                conditionsModel.sortField = sortField;
                conditionsModel.pageSize = pageSize;
                conditionsModel.page = page;
                return Success("获取成功", GetList(conditionsModel, leaveList));
            }
            catch
            {
                return SystemError();
            }
            #endregion
        }

        [HttpPost, Route("post")]
        public ApiResult Post([FromBody]SelectCondition model)
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
                AccountInfo accountInfo = GetAccountInfo(model.access_token);
                IQueryable<vw_LeaveList> leaveList = db.vw_LeaveList;
                SelectCondition conditionsModel = new SelectCondition();

                if (model.conditions != null && model.conditions.Count() != 0)
                {
                    foreach (var item in model.conditions)
                    {
                        if (item.fieldName == "ST_Grade" || item.fieldName == "ST_TeacherID")
                        {
                            continue;
                        }
                        conditionsModel.conditions.Add(CreatCondition(item.fieldName, item.fieldValues));
                    }
                }
                conditionsModel.conditions.Add(CreatCondition("ST_Grade", accountInfo.Grade));
                conditionsModel.conditions.Add(CreatCondition("ST_TeacherID", accountInfo.userID));
                conditionsModel.sortDirection = model.sortDirection;
                conditionsModel.sortField = model.sortField;
                conditionsModel.pageSize = model.pageSize;
                conditionsModel.page = model.page;
                return Success("获取成功", GetList(conditionsModel, leaveList));
            }
            catch
            {
                return SystemError();
            }
            #endregion
        }

        [HttpGet, Route("auditleave")]
        public ApiResult AuditLeave(string access_token, string LL_ID)
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
                vw_New_LeaveList LL = GetLeaveListModel(LL_ID);
                if (LL != null)
                {
                    if (LL.ST_Grade == accountInfo.Grade && LL.ST_TeacherID == accountInfo.userID)
                    {
                        if (LL.StateLeave == "0" && LL.StateBack == "0")
                        {
                            #region 同意请假操作
                            T_New_LeaveList _LL = db.T_New_LeaveList.Find(LL_ID);
                            //早晚自习请假和上课请假不需要销假
                            if (_LL.LeaveType == 6 || _LL.LeaveType == 7)
                            {
                                _LL.StateLeave = "1";
                                _LL.StateBack = "1";
                            }
                            else
                            {
                                _LL.StateLeave = "1";
                                _LL.StateBack = "0";
                            }
                            _LL.AuditTeacher = accountInfo.userName;

                            //保存至数据库
                            db.SaveChanges();

                            //发送短信 go 代表同意请假
                            SendSms.sendSms(new MessageModel(LL, "go"));
                            return Success("已同意请假！");
                            #endregion
                        }
                        else
                        {
                            return Error("此请假记录不处于待审核状态，请重新输入。");
                        }
                    }
                    else
                    {
                        return Error("您不具备操作此请假记录权限。");
                    }
                }
                else
                {
                    return Error("此请假记录ID不存在，请重新输入。");
                }
            }
            catch
            {
                return SystemError();
            }
            #endregion
        }

        [HttpGet, Route("auditback")]
        public ApiResult AuditBack(string access_token, string LL_ID)
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
                vw_New_LeaveList LL = GetLeaveListModel(LL_ID);
                if (LL != null)
                {
                    if (LL.ST_Grade == accountInfo.Grade && LL.ST_TeacherID == accountInfo.userID)
                    {
                        if (LL.StateLeave == "1" && LL.StateBack == "0")
                        {
                            #region 同意请假操作
                            T_New_LeaveList _LL = db.T_New_LeaveList.Find(LL_ID);
                            _LL.StateLeave = "1";
                            _LL.StateBack = "1";
                            _LL.AuditTeacher = accountInfo.userName;

                            //保存至数据库
                            db.SaveChanges();

                            //发送短信 go 代表同意请假  利用线程短信
                            //Thread sendMsg = new Thread(new ThreadStart());
                            SendSms.sendSms(new MessageModel(LL, "back"));
                            return Success("已同意销假！");
                            #endregion
                        }
                        else
                        {
                            return Error("此请假记录不处于待销假状态，请重新输入。");
                        }
                    }
                    else
                    {
                        return Error("您不具备操作此请假记录权限。");
                    }
                }
                else
                {
                    return Error("此请假记录ID不存在，请重新输入。");
                }
            }
            catch
            {
                return SystemError();
            }
            #endregion
        }

        [HttpGet, Route("auditreject")]
        public ApiResult AuditReject(string access_token, string LL_ID)
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
                vw_New_LeaveList LL = GetLeaveListModel(LL_ID);
                if (LL != null)
                {
                    if (LL.ST_Grade == accountInfo.Grade && LL.ST_TeacherID == accountInfo.userID)
                    {
                        if (LL.StateLeave == "0" && LL.StateBack == "0")
                        {
                            #region 同意请假操作
                            T_New_LeaveList _LL = db.T_New_LeaveList.Find(LL_ID);
                            _LL.StateLeave = "2";
                            _LL.StateBack = "1";
                            _LL.AuditTeacher = accountInfo.userName;

                            //保存至数据库
                            db.SaveChanges();

                            //发送短信 go 代表同意请假
                            SendSms.sendSms(new MessageModel(LL, "failed"));
                            return Success("已驳回请假！");
                            #endregion
                        }
                        else
                        {
                            return Error("此请假记录不处于待审核状态，请重新输入。");
                        }
                    }
                    else
                    {
                        return Error("您不具备操作此请假记录权限。");
                    }
                }
                else
                {
                    return Error("此请假记录ID不存在，请重新输入。");
                }
            }
            catch
            {
                return SystemError();
            }
            #endregion
        }

        #region 发送短信
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_LL"></param>
        /// <param name="messageType"></param>
        private void SendMsg(vw_New_LeaveList _LL, string messageType)
        {
            var _enableMessage = from vw_TeacherLeaveType in db.vw_TeacherLeaveType where (vw_TeacherLeaveType.LeaveTypeID == _LL.LeaveType && vw_TeacherLeaveType.TeacherID == _LL.ST_TeacherID) select vw_TeacherLeaveType.EnableMessage;
            if (_enableMessage.Any() && _enableMessage.ToList().First() != 0)
            {
                SendSms.sendSms(new MessageModel(_LL, messageType));
            }
        }
        #endregion
    }
}