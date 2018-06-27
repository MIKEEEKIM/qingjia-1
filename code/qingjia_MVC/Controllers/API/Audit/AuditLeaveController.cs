using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using qingjia_MVC.Common;
using qingjia_MVC.Models;
using qingjia_MVC.Models.API;
using qingjia_MVC.Models.API.Audit;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

                //需判断用户角色
                if (accountInfo.userRoleID == "1")
                {
                    leaveList = leaveList.Where(c => c.StudentID == accountInfo.userID);
                }
                else if (accountInfo.userRoleID == "2")
                {
                    leaveList = leaveList.Where(c => c.ST_Class == accountInfo.userName);
                }
                else if (accountInfo.userRoleID == "3")
                {
                    leaveList = leaveList.Where(c => c.ST_TeacherID == accountInfo.userID);
                }

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
                    //需要判断用户角色
                    string teacherID = GetTeacherID(accountInfo);
                    var leaveTypeList = from vw_TeacherLeaveType in db.vw_TeacherLeaveType where (vw_TeacherLeaveType.TeacherID == teacherID && vw_TeacherLeaveType.IsDelete == 0) select vw_TeacherLeaveType.LeaveTypeID.ToString();
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

                //需判断用户角色
                if (accountInfo.userRoleID == "1")
                {
                    leaveList = leaveList.Where(c => c.StudentID == accountInfo.userID);
                }
                else if (accountInfo.userRoleID == "2")
                {
                    leaveList = leaveList.Where(c => c.ST_Class == accountInfo.userName);
                }
                else if (accountInfo.userRoleID == "3")
                {
                    leaveList = leaveList.Where(c => c.ST_TeacherID == accountInfo.userID);
                }

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
                else if (state == "5")
                {
                    conditionsModel.conditions.Add(CreatCondition("StateBack", "0"));
                }
                else
                {
                    return Error("state参数值范围为[0,1,2,3,4,5]");
                }

                if (leaveTypeID != "0")
                {
                    conditionsModel.conditions.Add(CreatCondition("LeaveType", leaveTypeID));
                }
                else
                {
                    //需要判断用户角色
                    string teacherID = GetTeacherID(accountInfo);
                    var leaveTypeList = from vw_TeacherLeaveType in db.vw_TeacherLeaveType where (vw_TeacherLeaveType.TeacherID == teacherID && vw_TeacherLeaveType.IsDelete == 0) select vw_TeacherLeaveType.LeaveTypeID.ToString();
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
                //需判断用户角色
                if (accountInfo.userRoleID == "1")
                {
                    leaveList = leaveList.Where(c => c.StudentID == accountInfo.userID);
                }
                else if (accountInfo.userRoleID == "2")
                {
                    leaveList = leaveList.Where(c => c.ST_Class == accountInfo.userName);
                }
                else if (accountInfo.userRoleID == "3")
                {
                    leaveList = leaveList.Where(c => c.ST_TeacherID == accountInfo.userID);
                }

                SelectCondition conditionsModel = new SelectCondition();
                conditionsModel.conditions.Add(CreatCondition("ST_Grade", accountInfo.Grade));

                //需判断用户角色
                //conditionsModel.conditions.Add(CreatCondition("ST_TeacherID", accountInfo.userID));

                if (state == "0")//全部请假
                {
                    //返回所有类型请假数据
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
                else if (state == "5")
                {
                    conditionsModel.conditions.Add(CreatCondition("StateBack", "0"));
                }
                else
                {
                    return Error("state参数值范围为[0,1,2,3,4,5]");
                }

                if (leaveTypeID != "0")
                {
                    conditionsModel.conditions.Add(CreatCondition("LeaveType", leaveTypeID));
                }
                else
                {
                    //需要判断用户角色
                    string teacherID = GetTeacherID(accountInfo);
                    var leaveTypeList = from vw_TeacherLeaveType in db.vw_TeacherLeaveType where (vw_TeacherLeaveType.TeacherID == teacherID && vw_TeacherLeaveType.IsDelete == 0) select vw_TeacherLeaveType.LeaveTypeID.ToString();
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

                //需判断用户角色
                if (accountInfo.userRoleID == "1")
                {
                    leaveList = leaveList.Where(c => c.StudentID == accountInfo.userID);
                }
                else if (accountInfo.userRoleID == "2")
                {
                    leaveList = leaveList.Where(c => c.ST_Class == accountInfo.userName);
                }
                else if (accountInfo.userRoleID == "3")
                {
                    leaveList = leaveList.Where(c => c.ST_TeacherID == accountInfo.userID);
                }

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
                //conditionsModel.conditions.Add(CreatCondition("ST_TeacherID", accountInfo.userID));

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
                else if (model.state == "5")
                {
                    conditionsModel.conditions.Add(CreatCondition("StateBack", "0"));
                }
                else
                {
                    return Error("state参数值范围为[0,1,2,3,4,5]");
                }

                //请假类型搜索
                if (model.leaveTypeID != "0")
                {
                    conditionsModel.conditions.Add(CreatCondition("LeaveType", model.leaveTypeID));
                }
                else
                {
                    //需要判断用户角色
                    string teacherID = GetTeacherID(accountInfo);
                    var leaveTypeList = from vw_TeacherLeaveType in db.vw_TeacherLeaveType where (vw_TeacherLeaveType.TeacherID == teacherID && vw_TeacherLeaveType.IsDelete == 0) select vw_TeacherLeaveType.LeaveTypeID.ToString();
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
                    if (LL.ST_Grade.Trim() == accountInfo.Grade && LL.ST_TeacherID.Trim() == accountInfo.userID)
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

                            //创建线程执行短信操作
                            //Thread MsgThread = new Thread(new ParameterizedThreadStart(SendAsync));
                            //MsgThread.Start(CreatMessage(LL, "go"));

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

                            //创建线程执行短信操作
                            //Thread MsgThread = new Thread(new ParameterizedThreadStart(SendAsync));
                            //MsgThread.Start(CreatMessage(LL, "back"));

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
                    if (LL.ST_Grade.Trim() == accountInfo.Grade && LL.ST_TeacherID.Trim() == accountInfo.userID)
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

                            //创建线程执行短信操作
                            //Thread MsgThread = new Thread(new ParameterizedThreadStart(SendAsync));
                            //MsgThread.Start(CreatMessage(LL, "failed"));

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

        #region 节假日去向导出模块
        [HttpGet, Route("getholidayleavelist")]
        public ApiResult GetHolidayLeaveList(string access_token, string startTime, string endTime, string classID)
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
                DateTime _startTime = Convert.ToDateTime(startTime);
                DateTime _endTime = Convert.ToDateTime(endTime);
                IQueryable<vw_New_LeaveList> list = db.vw_New_LeaveList.Where(q => q.LeaveType.Trim() == "4" && q.IsDelete == 0 && q.StateBack.Trim() == "0");//4 代表节假日请假
                list = list.Where(q => ((q.LeaveTime <= _startTime && q.BackTime >= _startTime) || (q.LeaveTime <= _endTime && q.BackTime >= _endTime) || (q.LeaveTime <= _startTime && q.BackTime >= _endTime) || (q.LeaveTime >= _startTime && q.BackTime <= _endTime)));
                if (accountInfo.userRoleID == "1")
                {
                    List<vw_Class> classList = db.vw_Class.Where(q => q.MonitorID.Trim() == accountInfo.userID).ToList();
                    if (classList.Any())
                    {
                        string className = classList.First().ClassName.Trim();
                        list = list.Where(q => q.ST_Class.Trim() == className);
                    }
                    else
                    {
                        return Error("仅班级负责人账号可以到处节假日去向表！");
                    }
                }
                else if (accountInfo.userRoleID == "2")
                {
                    list = list.Where(q => q.ST_Class.Trim() == accountInfo.userName);
                }
                else if (accountInfo.userRoleID == "3")
                {
                    list = list.Where(q => q.ST_TeacherID.Trim() == accountInfo.userID);
                }
                else
                {
                    return Error("此角色不支持到处节假日去向！");
                }

                list = list.OrderBy(q => q.StudentID);

                return Success("获取成功", TransportHolidayData(list.ToList(), classID));
            }
            catch (Exception ex)
            {
                return SystemError(ex);
            }
            #endregion
        }

        [HttpGet, Route("downloadholidaylist")]
        public HttpResponseMessage DownLoadExeclData(string access_token, string startTime, string endTime, string classID)
        {
            ApiResult result = GetHolidayLeaveList(access_token, startTime, endTime, classID);
            if (result.status == "success")
            {
                try
                {
                    T_Class classModel = db.T_Class.Find(classID);
                    if (classModel == null)
                    {
                        return null;
                    }
                    string fileName = "节假日去向表--" + classModel.ClassName.Trim() + ".xls";
                    HttpResponseMessage _result = new HttpResponseMessage(HttpStatusCode.OK);
                    var stream = ToExcel((List<HolidayTable>)result.data, classModel.ClassName.Trim(), fileName);
                    _result.Content = new StreamContent(stream);
                    _result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                    _result.Content.Headers.ContentDisposition.FileName = fileName;
                    _result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.ms-excel");
                    _result.Content.Headers.ContentLength = stream.Length;
                    return _result;
                }
                catch
                {
                    throw new Exception("生成表格发生错误！");
                }
            }
            return null;
        }

        /// <summary>
        /// 从数据库中查询数据
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="UserID"></param>
        /// <param name="RoleID"></param>
        /// <param name="className"></param>
        private List<HolidayTable> TransportHolidayData(List<vw_New_LeaveList> list, string classID)
        {
            List<HolidayTable> tableLeaveList = new List<HolidayTable>();
            List<vw_Student> studnetList = db.vw_Student.Where(q => q.ClassID.Trim() == classID).OrderBy(q => q.ST_Num).ToList();
            if (studnetList.Any())
            {
                foreach (var item in studnetList)
                {
                    HolidayTable model = new HolidayTable();
                    model.ST_Num = item.ST_Num.Trim();
                    model.ST_Name = item.ST_Name.Trim();
                    model.ST_Class = item.ST_Class;
                    model.Contact = item.ContactOne == null ? "" : item.ContactOne;
                    model.Tel = item.ST_Tel == null ? "" : item.ST_Tel;

                    model.ST_Go = "留校";
                    model.TimeLeave = "";
                    model.TimeBack = "";
                    model.LeaveWay = "";
                    model.BackWay = "";
                    model.Address = "";
                    foreach (var _item in list)
                    {
                        if (_item.StudentID.Trim() == item.ST_Num.Trim())
                        {
                            model.ST_Go = _item.Reason;
                            model.TimeLeave = ((DateTime)_item.LeaveTime).ToString("yyyy-MM-dd HH:mm:ss");
                            model.TimeBack = ((DateTime)_item.BackTime).ToString("yyyy-MM-dd HH:mm:ss");
                            model.LeaveWay = _item.LeaveWay;
                            model.BackWay = _item.BackWay;
                            model.Address = _item.Address;
                            break;
                        }
                    }

                    tableLeaveList.Add(model);
                }

                return tableLeaveList;
            }
            else
            {
                throw new Exception("数据库数据错误，该班级不包含任何学生");
            }
        }

        private MemoryStream ToExcel(List<HolidayTable> table_LL, string className, string title)
        {
            //文件名称   必须包含 .xls
            string fileName = "节假日去向表--" + className + ".xls";

            //创建工作簿、工作表
            HSSFWorkbook newExcel = new HSSFWorkbook();
            HSSFSheet sheet = (HSSFSheet)newExcel.CreateSheet("离校去向表");
            setSheet(newExcel, sheet, table_LL, title);

            //输出
            MemoryStream ms = new MemoryStream();
            newExcel.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        private void setSheet(HSSFWorkbook newExcel, HSSFSheet sheet, List<HolidayTable> dt, string title)
        {
            #region 设置行宽，列高
            sheet.SetColumnWidth(0, 30 * 256);
            sheet.SetColumnWidth(1, 30 * 256);
            sheet.SetColumnWidth(2, 30 * 256);
            sheet.SetColumnWidth(3, 30 * 256);
            sheet.SetColumnWidth(4, 30 * 256);
            sheet.SetColumnWidth(5, 30 * 256);
            sheet.SetColumnWidth(6, 30 * 256);
            sheet.SetColumnWidth(7, 30 * 256);
            sheet.SetColumnWidth(8, 30 * 256);
            sheet.SetColumnWidth(9, 30 * 256);
            sheet.SetColumnWidth(10, 30 * 256);
            sheet.SetColumnWidth(11, 30 * 256);
            sheet.SetColumnWidth(12, 30 * 256);
            sheet.DefaultRowHeight = 15 * 20;
            #endregion

            #region 设置字体
            HSSFFont font_title = (HSSFFont)newExcel.CreateFont();
            font_title.FontHeightInPoints = 10;

            HSSFFont font_name = (HSSFFont)newExcel.CreateFont();
            font_name.FontHeightInPoints = 7;
            font_name.IsBold = true;

            HSSFFont font_data = (HSSFFont)newExcel.CreateFont();
            font_data.FontHeightInPoints = 7;
            #endregion

            #region 设置样式
            //1、标题的样式
            HSSFCellStyle style_title = (HSSFCellStyle)newExcel.CreateCellStyle();
            style_title.Alignment = HorizontalAlignment.Center;
            style_title.VerticalAlignment = VerticalAlignment.Center;
            style_title.SetFont(font_title);

            //2、字段名的样式
            HSSFCellStyle style_name = (HSSFCellStyle)newExcel.CreateCellStyle();
            style_name.Alignment = HorizontalAlignment.Center;
            style_name.VerticalAlignment = VerticalAlignment.Center;
            style_name.SetFont(font_name);
            style_name.BorderTop = BorderStyle.Thin;
            style_name.BorderBottom = BorderStyle.Thin;
            style_name.BorderLeft = BorderStyle.Thin;
            style_name.BorderRight = BorderStyle.Thin;

            //3、批次的样式
            HSSFCellStyle style_batch = (HSSFCellStyle)newExcel.CreateCellStyle();
            style_batch.Alignment = HorizontalAlignment.Center;
            style_batch.VerticalAlignment = VerticalAlignment.Center;
            style_batch.FillPattern = FillPattern.SolidForeground;
            style_batch.FillForegroundColor = HSSFColor.Grey40Percent.Index;
            style_batch.SetFont(font_data);
            style_batch.BorderTop = BorderStyle.Thin;
            style_batch.BorderBottom = BorderStyle.Thin;
            style_batch.BorderLeft = BorderStyle.Thin;
            style_batch.BorderRight = BorderStyle.Thin;

            //4、数据的样式
            HSSFCellStyle style_data = (HSSFCellStyle)newExcel.CreateCellStyle();
            style_data.Alignment = HorizontalAlignment.Center;
            style_data.VerticalAlignment = VerticalAlignment.Center;
            style_data.SetFont(font_data);
            style_data.BorderTop = BorderStyle.Thin;
            style_data.BorderBottom = BorderStyle.Thin;
            style_data.BorderLeft = BorderStyle.Thin;
            style_data.BorderRight = BorderStyle.Thin;
            #endregion

            #region 设置内容
            //第一行 标题
            HSSFRow row_title = (HSSFRow)sheet.CreateRow(0);
            HSSFCell cell_title = (HSSFCell)row_title.CreateCell(0);
            cell_title.SetCellValue(title);
            cell_title.CellStyle = style_title;
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 12));   //合并单元格(起始行，结束行，起始列，结束列)

            //第二行 字段名 
            HSSFRow row_name = (HSSFRow)sheet.CreateRow(1);

            HSSFCell cell_name_1 = (HSSFCell)row_name.CreateCell(0);
            cell_name_1.SetCellValue("序号");
            cell_name_1.CellStyle = style_name;

            HSSFCell cell_name_2 = (HSSFCell)row_name.CreateCell(1);
            cell_name_2.SetCellValue("学号");
            cell_name_2.CellStyle = style_name;

            HSSFCell cell_name_3 = (HSSFCell)row_name.CreateCell(2);
            cell_name_3.SetCellValue("班级");
            cell_name_3.CellStyle = style_name;

            HSSFCell cell_name_4 = (HSSFCell)row_name.CreateCell(3);
            cell_name_4.SetCellValue("姓名");
            cell_name_4.CellStyle = style_name;

            HSSFCell cell_name_5 = (HSSFCell)row_name.CreateCell(4);
            cell_name_5.SetCellValue("节假日去向");
            cell_name_5.CellStyle = style_name;

            HSSFCell cell_name_6 = (HSSFCell)row_name.CreateCell(5);
            cell_name_6.SetCellValue("离校时间");
            cell_name_6.CellStyle = style_name;

            HSSFCell cell_name_7 = (HSSFCell)row_name.CreateCell(6);
            cell_name_7.SetCellValue("返校时间");
            cell_name_7.CellStyle = style_name;

            HSSFCell cell_name_8 = (HSSFCell)row_name.CreateCell(7);
            cell_name_8.SetCellValue("离校方式");
            cell_name_8.CellStyle = style_name;

            HSSFCell cell_name_9 = (HSSFCell)row_name.CreateCell(8);
            cell_name_9.SetCellValue("返校方式");
            cell_name_9.CellStyle = style_name;

            HSSFCell cell_name_10 = (HSSFCell)row_name.CreateCell(9);
            cell_name_10.SetCellValue("离校去向地址");
            cell_name_10.CellStyle = style_name;

            HSSFCell cell_name_11 = (HSSFCell)row_name.CreateCell(10);
            cell_name_11.SetCellValue("联系人");
            cell_name_11.CellStyle = style_name;

            HSSFCell cell_name_12 = (HSSFCell)row_name.CreateCell(11);
            cell_name_12.SetCellValue("联系方式");
            cell_name_12.CellStyle = style_name;

            HSSFCell cell_name_13 = (HSSFCell)row_name.CreateCell(12);
            cell_name_13.SetCellValue("签名确认");
            cell_name_13.CellStyle = style_name;

            //数据
            int n = 2;
            int i = 1;
            foreach (HolidayTable item in dt)
            {
                HSSFRow row = (HSSFRow)sheet.CreateRow(n++);//写入行  
                row.CreateCell(0).SetCellValue(i++);
                row.CreateCell(1).SetCellValue(item.ST_Num);
                row.CreateCell(2).SetCellValue(item.ST_Class);
                row.CreateCell(3).SetCellValue(item.ST_Name);
                row.CreateCell(4).SetCellValue(item.ST_Go);
                row.CreateCell(5).SetCellValue(item.TimeLeave);
                row.CreateCell(6).SetCellValue(item.TimeBack);
                row.CreateCell(7).SetCellValue(item.LeaveWay);
                row.CreateCell(8).SetCellValue(item.BackWay);
                row.CreateCell(9).SetCellValue(item.Address);
                row.CreateCell(10).SetCellValue(item.Contact);
                row.CreateCell(11).SetCellValue(item.Tel);
                row.CreateCell(12).SetCellValue("");
                foreach (ICell cell in row)
                {
                    if (cell.ColumnIndex == 0)
                    {
                        cell.CellStyle = style_batch;
                    }
                    else
                    {
                        cell.CellStyle = style_data;
                    }
                }
            }
            #endregion
        }
        #endregion

        #region 发送短信
        /// <summary>
        /// 判断是否发送短信 创建短信模板
        /// </summary>
        /// <param name="_LL"></param>
        /// <param name="messageType"></param>
        private MessageModel CreatMessage(vw_New_LeaveList _LL, string messageType)
        {
            var _enableMessage = from vw_TeacherLeaveType in db.vw_TeacherLeaveType where (vw_TeacherLeaveType.LeaveTypeID.ToString() == _LL.LeaveType && vw_TeacherLeaveType.TeacherID == _LL.ST_TeacherID) select vw_TeacherLeaveType.EnableMessage;
            if (_enableMessage.Any() && _enableMessage.ToList().First() != 0)
            {
                return new MessageModel(_LL, messageType);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 执行发送短信操作 输出短信发送日志
        /// </summary>
        /// <param name="_message"></param>
        private void SendAsync(object _message)
        {
            try
            {
                if (_message != null)
                {
                    //发送短信
                    MessageModel message = (MessageModel)_message;
                    SendSms.sendSms(message);
                }
            }
            catch
            {
                //短信发送失败时记录发送失败日志
            }
        }
        #endregion
    }
}