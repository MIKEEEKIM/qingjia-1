using qingjia_MVC.Common;
using qingjia_MVC.Models;
using qingjia_MVC.Models.API;
using qingjia_MVC.Models.API.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        //[HttpOptions,Route("post")]
        //public ApiResult Options()
        //{
        //    return null; // HTTP 200 response with empty body
        //}

        /// <summary>
        /// Get 令牌、请假类型ID
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="leaveTypeID"></param>
        /// <returns></returns>
        [HttpGet, Route("get")]
        public ApiResult Get(string access_token, string leaveTypeID, string state)
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
                IQueryable<vw_New_LeaveList> leaveList = db.vw_New_LeaveList.Where(c => c.IsDelete == 0).OrderByDescending(c => c.SubmitTime);
                SelectCondition conditionsModel = new SelectCondition();
                conditionsModel.conditions.Add(CreatCondition("ST_Grade", accountInfo.Grade));
                conditionsModel.conditions.Add(CreatCondition("ST_TeacherID", accountInfo.userID));

                if (state == "0")//全部请假
                {

                }
                else if (state == "1")//待审核 
                {
                    conditionsModel.conditions.Add(CreatCondition("StateLeave", "0"));
                    conditionsModel.conditions.Add(CreatCondition("StateBack", "0"));
                }
                else if (state == "2")//待销假 
                {
                    conditionsModel.conditions.Add(CreatCondition("StateLeave", "1"));
                    conditionsModel.conditions.Add(CreatCondition("StateBack", "0"));
                }
                else if (state == "3")//已销假 
                {
                    conditionsModel.conditions.Add(CreatCondition("StateLeave", "1"));
                    conditionsModel.conditions.Add(CreatCondition("StateBack", "1"));
                }
                else if (state == "4")//已驳回
                {
                    conditionsModel.conditions.Add(CreatCondition("StateLeave", "2"));
                    conditionsModel.conditions.Add(CreatCondition("StateBack", "1"));
                }
                else
                {
                    return Error("state参数值范围为[0,1,2,3,4]");
                }

                if (leaveTypeID != "0")
                {
                    conditionsModel.conditions.Add(CreatCondition("LeaveType", leaveTypeID));
                }
                else
                {
                    var leaveTypeList = from vw_TeacherLeaveType in db.vw_TeacherLeaveType where (vw_TeacherLeaveType.TeacherID == accountInfo.userID && vw_TeacherLeaveType.IsDelete == 0) select vw_TeacherLeaveType.LeaveTypeID.ToString();
                    if (leaveTypeList.Any())
                    {
                        conditionsModel.conditions.Add(CreatCondition("LeaveType", leaveTypeList.ToList()));
                    }
                    else
                    {
                        return Error("尚未设置可用请假类型");
                    }
                }

                DataList dtSource = GetList(conditionsModel, leaveList);
                dtSource.list = TransformLL((List<vw_New_LeaveList>)dtSource.list);
                return Success("获取成功", dtSource);
            }
            catch (ArgumentException ex)
            {
                ex.ToString();
                OutputLog.WriteLog(DateTime.Now.ToString() + ex.ToString());
                return SystemError();
            }
            #endregion
        }

        /// <summary>
        /// Get 获取请假记录 参数为授权令牌、请假类型ID、页面显示数据条数、页数
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="leaveTypeID"></param>
        /// <param name="pageSize"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet, Route("get")]
        public ApiResult Get(string access_token, string leaveTypeID, string state, int pageSize, int page)
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
                IQueryable<vw_New_LeaveList> leaveList = db.vw_New_LeaveList.Where(c => c.IsDelete == 0).OrderByDescending(c => c.SubmitTime);
                SelectCondition conditionsModel = new SelectCondition();
                conditionsModel.conditions.Add(CreatCondition("ST_Grade", accountInfo.Grade));
                conditionsModel.conditions.Add(CreatCondition("ST_TeacherID", accountInfo.userID));
                
                if (state == "0")//全部请假
                {

                }
                else if (state == "1")//待审核 
                {
                    conditionsModel.conditions.Add(CreatCondition("StateLeave", "0"));
                    conditionsModel.conditions.Add(CreatCondition("StateBack", "0"));
                }
                else if (state == "2")//待销假 
                {
                    conditionsModel.conditions.Add(CreatCondition("StateLeave", "1"));
                    conditionsModel.conditions.Add(CreatCondition("StateBack", "0"));
                }
                else if (state == "3")//已销假 
                {
                    conditionsModel.conditions.Add(CreatCondition("StateLeave", "1"));
                    conditionsModel.conditions.Add(CreatCondition("StateBack", "1"));
                }
                else if (state == "4")//已驳回
                {
                    conditionsModel.conditions.Add(CreatCondition("StateLeave", "2"));
                    conditionsModel.conditions.Add(CreatCondition("StateBack", "1"));
                }
                else
                {
                    return Error("state参数值范围为[0,1,2,3,4]");
                }

                if (leaveTypeID != "0")
                {
                    conditionsModel.conditions.Add(CreatCondition("LeaveType", leaveTypeID));
                }
                else
                {
                    var leaveTypeList = from vw_TeacherLeaveType in db.vw_TeacherLeaveType where (vw_TeacherLeaveType.TeacherID == accountInfo.userID && vw_TeacherLeaveType.IsDelete == 0) select vw_TeacherLeaveType.LeaveTypeID.ToString();
                    if (leaveTypeList.Any())
                    {
                        conditionsModel.conditions.Add(CreatCondition("LeaveType", leaveTypeList.ToList()));
                    }
                    else
                    {
                        return Error("尚未设置可用请假类型");
                    }
                }

                conditionsModel.pageSize = pageSize;
                conditionsModel.page = page;

                DataList dtSource = GetList(conditionsModel, leaveList);
                dtSource.list = TransformLL((List<vw_New_LeaveList>)dtSource.list);
                return Success("获取成功", dtSource);
            }
            catch (ArgumentException ex)
            {
                ex.ToString();
                OutputLog.WriteLog(ex.ToString());
                return SystemError();
            }
            #endregion
        }

        /// <summary>
        /// Get 获取请假记录 参数为授权令牌、请假类型ID、页面显示数据条数、页数、排序字段、排序方向
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="leaveTypeID"></param>
        /// <param name="pageSize"></param>
        /// <param name="page"></param>
        /// <param name="sortDirection"></param>
        /// <param name="sortField"></param>
        /// <returns></returns>
        [HttpGet, Route("get")]
        public ApiResult Get(string access_token, string leaveTypeID, string state, int pageSize, int page, string sortDirection, string sortField)
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
                if (sortDirection == null || sortDirection == "" || (sortDirection != "DESC" && sortDirection != "ASC"))
                {
                    return Error("sortDirection参数错误");
                }
                if (sortField == null || sortField == "")
                {
                    return Error("sortField参数错误");
                }

                AccountInfo accountInfo = GetAccountInfo(access_token);
                IQueryable<vw_New_LeaveList> leaveList = db.vw_New_LeaveList.Where(c => c.IsDelete == 0).OrderByDescending(c => c.SubmitTime);
                SelectCondition conditionsModel = new SelectCondition();
                conditionsModel.conditions.Add(CreatCondition("ST_Grade", accountInfo.Grade));
                conditionsModel.conditions.Add(CreatCondition("ST_TeacherID", accountInfo.userID));

                if (state == "0")//全部请假
                {

                }
                else if (state == "1")//待审核 
                {
                    conditionsModel.conditions.Add(CreatCondition("StateLeave", "0"));
                    conditionsModel.conditions.Add(CreatCondition("StateBack", "0"));
                }
                else if (state == "2")//待销假 
                {
                    conditionsModel.conditions.Add(CreatCondition("StateLeave", "1"));
                    conditionsModel.conditions.Add(CreatCondition("StateBack", "0"));
                }
                else if (state == "3")//已销假 
                {
                    conditionsModel.conditions.Add(CreatCondition("StateLeave", "1"));
                    conditionsModel.conditions.Add(CreatCondition("StateBack", "1"));
                }
                else if (state == "4")//已驳回
                {
                    conditionsModel.conditions.Add(CreatCondition("StateLeave", "2"));
                    conditionsModel.conditions.Add(CreatCondition("StateBack", "1"));
                }
                else
                {
                    return Error("state参数值范围为[0,1,2,3,4]");
                }

                if (leaveTypeID != "0")
                {
                    conditionsModel.conditions.Add(CreatCondition("LeaveType", leaveTypeID));
                }
                else
                {
                    var leaveTypeList = from vw_TeacherLeaveType in db.vw_TeacherLeaveType where (vw_TeacherLeaveType.TeacherID == accountInfo.userID && vw_TeacherLeaveType.IsDelete == 0) select vw_TeacherLeaveType.LeaveTypeID.ToString();
                    if (leaveTypeList.Any())
                    {
                        conditionsModel.conditions.Add(CreatCondition("LeaveType", leaveTypeList.ToList()));
                    }
                    else
                    {
                        return Error("尚未设置可用请假类型");
                    }
                }

                conditionsModel.sortDirection = sortDirection;
                conditionsModel.sortField = sortField;
                conditionsModel.pageSize = pageSize;
                conditionsModel.page = page;

                DataList dtSource = GetList(conditionsModel, leaveList);
                dtSource.list = TransformLL((List<vw_New_LeaveList>)dtSource.list);
                return Success("获取成功", dtSource);
            }
            catch (ArgumentException ex)
            {
                ex.ToString();
                OutputLog.WriteLog(ex.ToString());
                return SystemError();
            }
            #endregion
        }

        /// <summary>
        /// Post 多条件查询
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
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
                IQueryable<vw_New_LeaveList> leaveList = db.vw_New_LeaveList.Where(c => c.IsDelete == 0).OrderByDescending(c => c.SubmitTime);
                SelectCondition conditionsModel = new SelectCondition();

                if (model.conditions != null && model.conditions.Count() != 0)
                {
                    foreach (var item in model.conditions)
                    {
                        if (item.fieldName == "ST_Grade" || item.fieldName == "ST_TeacherID" || item.fieldName == "IsDelete" || item.fieldName == "leaveTypeID")
                        {
                            continue;
                        }
                        conditionsModel.conditions.Add(CreatCondition(item.fieldName, item.fieldValues));
                    }
                }
                conditionsModel.conditions.Add(CreatCondition("ST_Grade", accountInfo.Grade));
                conditionsModel.conditions.Add(CreatCondition("ST_TeacherID", accountInfo.userID));

                if (model.state == "0")//全部请假
                {

                }
                else if (model.state == "1")//待审核 
                {
                    conditionsModel.conditions.Add(CreatCondition("StateLeave", "0"));
                    conditionsModel.conditions.Add(CreatCondition("StateBack", "0"));
                }
                else if (model.state == "2")//待销假 
                {
                    conditionsModel.conditions.Add(CreatCondition("StateLeave", "1"));
                    conditionsModel.conditions.Add(CreatCondition("StateBack", "0"));
                }
                else if (model.state == "3")//已销假 
                {
                    conditionsModel.conditions.Add(CreatCondition("StateLeave", "1"));
                    conditionsModel.conditions.Add(CreatCondition("StateBack", "1"));
                }
                else if (model.state == "4")//已驳回
                {
                    conditionsModel.conditions.Add(CreatCondition("StateLeave", "2"));
                    conditionsModel.conditions.Add(CreatCondition("StateBack", "1"));
                }
                else
                {
                    return Error("state参数值范围为[0,1,2,3,4]");
                }

                //请假类型搜索
                if (model.leaveTypeID != "0")
                {
                    conditionsModel.conditions.Add(CreatCondition("LeaveType", model.leaveTypeID));
                }
                else
                {
                    var leaveTypeList = from vw_TeacherLeaveType in db.vw_TeacherLeaveType where (vw_TeacherLeaveType.TeacherID == accountInfo.userID && vw_TeacherLeaveType.IsDelete == 0) select vw_TeacherLeaveType.LeaveTypeID.ToString();
                    if (leaveTypeList.Any())
                    {
                        conditionsModel.conditions.Add(CreatCondition("LeaveType", leaveTypeList.ToList()));
                    }
                    else
                    {
                        return Error("尚未设置可用请假类型");
                    }
                }

                conditionsModel.sortDirection = model.sortDirection;
                conditionsModel.sortField = model.sortField;
                conditionsModel.pageSize = model.pageSize;
                conditionsModel.page = model.page;

                DataList dtSource = GetList(conditionsModel, leaveList);
                dtSource.list = TransformLL((List<vw_New_LeaveList>)dtSource.list);
                return Success("获取成功", dtSource);
            }
            catch
            {
                return SystemError();
            }
            #endregion
        }

        /// <summary>
        /// Get 同意请假操作 授权令牌、请假记录ID
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="LL_ID"></param>
        /// <returns></returns>
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
                            if (_LL.LeaveType == "6" || _LL.LeaveType == "7")
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
                            SendMsg(LL, "go");

                            Thread MsgThread = new Thread(new ParameterizedThreadStart(SendAsync));

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

        /// <summary>
        /// Get 同意销假操作 授权令牌、请假记录ID
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="LL_ID"></param>
        /// <returns></returns>
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
                    if (LL.ST_Grade.Trim() == accountInfo.Grade.Trim() && LL.ST_TeacherID.Trim() == accountInfo.userID.Trim())
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
                            SendMsg(LL, "back");
                            //Thread sendMsg = new Thread(new ThreadStart(SendMsg));

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

        /// <summary>
        /// Post 驳回请假操作
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="LL_ID"></param>
        /// <returns></returns>
        [HttpPost, Route("auditreject")]
        public ApiResult AuditReject([FromBody]AuditReject model)
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
                vw_New_LeaveList LL = GetLeaveListModel(model.LL_ID);
                if (LL != null)
                {
                    if (LL.ST_Grade == accountInfo.Grade && LL.ST_TeacherID == accountInfo.userID)
                    {
                        if (LL.StateLeave == "0" && LL.StateBack == "0")
                        {
                            #region 同意请假操作
                            T_New_LeaveList _LL = db.T_New_LeaveList.Find(model.LL_ID);
                            _LL.StateLeave = "2";
                            _LL.StateBack = "1";
                            _LL.AuditTeacher = accountInfo.userName;
                            _LL.RejectReason = model.rejectReason;

                            //保存至数据库
                            db.SaveChanges();

                            //发送短信 go 代表同意请假
                            SendMsg(LL, "failed");

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="state">0代表全部请假 1 代表待审核请假 2 代表待销假请假 3 代表已销假 4 代表已驳回请假</param>
        /// <returns></returns>
        [HttpGet, Route("leaveTypeListNum")]
        public ApiResult ListInfo(string access_token, string state)
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
                if (state != "1" && state != "2")
                {
                    return Error("此接口state参数只能为1或2。");
                }
                AccountInfo accountInfo = GetAccountInfo(access_token);
                var _leaveTypeList = db.vw_TeacherLeaveType.Where(c => c.TeacherID == accountInfo.userID && c.IsDelete == 0);
                if (_leaveTypeList.Any())
                {
                    List<LeaveTypes> leaveTypeList = new List<LeaveTypes>();
                    foreach (var item in _leaveTypeList)
                    {
                        int _count;
                        if (state == "1")
                        {
                            _count = db.vw_New_LeaveList.Where(c => c.ST_TeacherID == accountInfo.userID && c.ST_Grade == accountInfo.Grade && c.IsDelete == 0 && c.LeaveType == item.LeaveTypeID.ToString().Trim() && c.StateLeave == "0" && c.StateBack == "0").Count();
                        }
                        else
                        {
                            _count = db.vw_New_LeaveList.Where(c => c.ST_TeacherID == accountInfo.userID && c.ST_Grade == accountInfo.Grade && c.IsDelete == 0 && c.LeaveType == item.LeaveTypeID.ToString().Trim() && c.StateLeave == "1" && c.StateBack == "0").Count();
                        }
                        leaveTypeList.Add(new LeaveTypes() { leaveTypeID = item.LeaveTypeID.ToString(), leaveTypeName = item.LeaveTypeName, count = _count, leaveTypeDescription = item.LeaveTypeDescription, enableMessage = item.EnableMessage.ToString() });
                    }
                    return Success("获取请假类型成功", leaveTypeList);
                }
                else
                {
                    return Error("尚未设置可用请假类型。");
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
        /// 发送通知短信
        /// </summary>
        /// <param name="_LL"></param>
        /// <param name="messageType"></param>
        private void SendMsg(vw_New_LeaveList _LL, string messageType)
        {
            var _enableMessage = from vw_TeacherLeaveType in db.vw_TeacherLeaveType where (vw_TeacherLeaveType.LeaveTypeID.ToString() == _LL.LeaveType && vw_TeacherLeaveType.TeacherID == _LL.ST_TeacherID) select vw_TeacherLeaveType.EnableMessage;
            if (_enableMessage.Any() && _enableMessage.ToList().First() != 0)
            {
                SendSms.sendSms(new MessageModel(_LL, messageType));
            }
        }

        private void SendAsync(object data_1, object data_2)
        {

        }
        #endregion
    }
}