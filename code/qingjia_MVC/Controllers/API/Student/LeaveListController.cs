using qingjia_MVC.Common;
using qingjia_MVC.Models;
using qingjia_MVC.Models.API;
using qingjia_MVC.Models.API.Student;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace qingjia_MVC.Controllers.API
{
    [RoutePrefix("api/leavelist")]
    public class LeaveListController : BaseApiController
    {
        /// <summary>
        /// Get 获取请假记录 参数为授权令牌、请假类型ID、页面显示数据条数、页数
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="leaveTypeID"></param>
        /// <param name="state"></param>
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
                IQueryable<vw_New_LeaveList> leaveList = db.vw_New_LeaveList.Where(c => c.StudentID == accountInfo.userID && c.IsDelete == 0).OrderByDescending(c => c.SubmitTime);
                SelectCondition conditionsModel = new SelectCondition();

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
        /// GET
        /// 撤销请假记录
        /// 学生删除待审核请假记录
        /// </summary>
        /// <param name="leavelistID"></param>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpGet, Route("revoke")]
        public ApiResult Revoke(string access_token, string leavelistID)
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
                T_New_LeaveList _LL = db.T_New_LeaveList.Where(c => c.IsDelete == 0 && c.StudentID.Trim() == accountInfo.userID.Trim() && c.ID.Trim() == leavelistID.Trim()).ToList().First();
                if (_LL == null)
                {
                    return Error("在您的请假记录中不包含ID为" + leavelistID + "的请假记录！");
                }
                if (_LL.StateLeave.Trim() == "0" && _LL.StateBack.Trim() == "0")
                {
                    _LL.IsDelete = 1;
                    db.SaveChanges();
                    return Success("撤回成功！");
                }
                else
                {
                    return Error("非待审核状态，不能撤回！");
                }
            }
            catch
            {
                return SystemError();
            }
            #endregion
        }

        /// <summary>
        /// POST
        /// 
        /// 学生离校请假
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("leaveschool")]
        public ApiResult LeaveSchool([FromBody]LeaveSchoolModel data)
        {
            #region 令牌验证
            result = Check(data.access_token);
            if (result != null)
            {
                return result;
            }
            #endregion

            #region 逻辑
            //验证参数是否完整
            try
            {
                if (data.Check())
                {
                    return leaveSchool(data);
                }
                else
                {
                    return Error("参数格式错误或缺少参数！");
                }
            }
            catch (Exception ex)
            {
                return SystemError();
            }
            #endregion
        }

        /// <summary>
        /// POST
        /// 
        /// 学生特殊请假
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost, Route("leavespecial")]
        public ApiResult LeaveSpecial([FromBody]LeaveSpecialModel data)
        {
            #region 令牌验证
            result = Check(data.access_token);
            if (result != null)
            {
                return result;
            }
            #endregion

            #region 逻辑
            //验证参数是否完整
            try
            {
                if (data.Check())
                {
                    return leaveSpecial(data);
                }
                else
                {
                    return Error("参数格式错误或缺少参数！");
                }
            }
            catch (Exception ex)
            {
                return SystemError();
            }
            #endregion
        }

        /// <summary>
        /// POST
        /// 
        /// 学生上课请假备案
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost, Route("leavelesson")]
        public ApiResult LeaveLesson([FromBody]LeaveClassModel data)
        {
            #region 令牌验证
            result = Check(data.access_token);
            if (result != null)
            {
                return result;
            }
            #endregion

            #region 逻辑
            //验证参数是否完整
            try
            {
                if (data.Check())
                {
                    return leaveClass(data);
                }
                else
                {
                    return Error("参数格式错误或缺少参数！");
                }
            }
            catch (Exception ex)
            {
                return SystemError();
            }
            #endregion
        }

        /// <summary>
        /// 打印请假条
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="LV_NUM"></param>
        /// <returns></returns>
        [HttpGet, Route("print")]
        public ApiBaseResult PrintLeave(string access_token, string LV_NUM)
        {
            //ApiBaseResult result = Check(access_token);
            //if (result == null)
            //{
            //    result = new ApiBaseResult();

            //    #region 检查此人是否有打印此请假条的权限
            //    string[] sArray = access_token.Split('_');
            //    string UserID = sArray[0];
            //    string GuidString = sArray[1];

            //    var accountList = from T_Account in db.T_Account where (T_Account.YB_AccessToken == GuidString) select T_Account;
            //    if (accountList.Any())
            //    {
            //        T_Account accountModel = accountList.ToList().First();
            //        if (accountModel.RoleID.ToString().Trim() == "1")
            //        {
            //            var leavelist = from vw_LeaveList in db.vw_LeaveList
            //                            where (vw_LeaveList.StudentID == accountModel.ID && vw_LeaveList.ID == LV_NUM)
            //                            select vw_LeaveList;
            //            if (leavelist.Any())
            //            {
            //                string url = UpLoadQiNiu.UpLoadData(Print.Print_Form(LV_NUM), LV_NUM);
            //                if (url != null)
            //                {
            //                    result.result = "success";
            //                    result.data = url;
            //                }
            //                else
            //                {
            //                    result.result = "error";
            //                    result.messages = "出现错误，请联系系统维护人员";
            //                }
            //            }
            //            else
            //            {
            //                result.result = "error";
            //                result.messages = "您没有请假单号为" + LV_NUM + "此条请假记录";
            //            }
            //        }
            //        else
            //        {
            //            result.result = "error";
            //            result.messages = "此接口仅支持学生账号访问";
            //        }
            //    }
            //    else
            //    {
            //        result.result = "error";
            //        result.messages = "出现未知错误，请联系维护人员";
            //    }
            //    #endregion
            //}
            //return result;
            return null;
        }

        #region 其他方法

        /// <summary>
        /// 通过API接口 下载图片
        /// </summary>
        /// <param name="LV_NUM"></param>
        /// <returns></returns>
        [HttpGet, Route("downloadpic")]
        public IHttpActionResult DownLoadPic(string LV_NUM)
        {
            return null;//此接口暂时关闭


            //var browser = string.Empty;
            //if (HttpContext.Current.Request.UserAgent != null)
            //{
            //    browser = HttpContext.Current.Request.UserAgent.ToUpper();
            //}
            //HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            //byte[] fileStream = Print.Print_Form(LV_NUM);
            //httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            //httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = LV_NUM + ".jpg" };
            //return ResponseMessage(httpResponseMessage);
        }

        //是否可以请假 存在bug

        #region 学生请假操作 此处不需要短信通知

        #region 请假类型合法性检查
        private T_TeacherLeaveType IsLeaveTypeExist(string studentID, int leaveTypeID)
        {
            string teacherID = db.vw_Student.Where(c => c.ST_Num.Trim() == studentID.Trim()).Select(c => c.ST_TeacherID.Trim()).ToList().First();
            T_TeacherLeaveType result = db.T_TeacherLeaveType.Where(c => c.TeacherID.Trim() == teacherID.Trim() && c.LeaveTypeID == leaveTypeID && c.IsDelete == 0).ToList().First();
            if (result != null)
            {
                return result;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region 请假条重复性检查
        /// <summary>
        /// 
        /// </summary>
        /// <param name="studentID"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private bool DuplicateCheck(string studentID, DateTime start, DateTime end)
        {
            if (db.T_New_LeaveList.Where(c => c.StudentID.Trim() == studentID.Trim() && c.IsDelete == 0 && c.StateBack.Trim() == "0" && (c.LeaveType.Trim() == "1" || c.LeaveType.Trim() == "2" || c.LeaveType.Trim() == "4") && ((c.LeaveTime >= start && c.BackTime <= end) || (c.LeaveTime > start && c.LeaveTime < end) || (c.BackTime > start && c.BackTime < end))).Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="studentID"></param>
        /// <param name="leaveTypeID"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        private bool DuplicateCheck(string studentID, string leaveTypeID, DateTime date)
        {
            if (db.T_New_LeaveList.Where(c => c.StudentID.Trim() == studentID && (c.LeaveType.Trim() == "5" || c.LeaveType.Trim() == "6" || c.LeaveType.Trim() == "7") && c.IsDelete == 0 && c.StateBack.Trim() == "0" && c.LeaveTime == date && c.LeaveType.Trim() == leaveTypeID.Trim()).Any())
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="studentID"></param>
        /// <param name="date"></param>
        /// <param name="lesson"></param>
        /// <returns></returns>
        private bool DuplicateCheck(string studentID, DateTime date, string lesson)
        {
            if (db.T_New_LeaveList.Where(c => c.StudentID.Trim() == studentID && c.LeaveType.Trim() == "8" && c.IsDelete == 0 && c.StateBack.Trim() == "0" && c.LeaveTime == date && c.Lesson.Trim() == lesson).Any())
            {
                return true;
            }
            return false;
        }
        #endregion

        /// <summary>
        /// 离校请假 短期请假 长期请假 节假日请假
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private ApiResult leaveSchool(LeaveSchoolModel data)
        {
            try
            {
                //验证是否开通此类请假 分类别判断请假  是否符合要求  是否相互之间存在重叠
                #region 请假操作
                AccountInfo accountInfo = GetAccountInfo(data.access_token);
                T_TeacherLeaveType leaveType = IsLeaveTypeExist(accountInfo.userID, Convert.ToInt32(data.leaveTypeID.ToString().Trim()));
                if (leaveType != null)
                {
                    T_New_LeaveList model = new T_New_LeaveList();

                    string leaveTimeString = data.leave_date + " " + data.leave_time;
                    string backTimeString = data.back_date + " " + data.back_time;
                    DateTime leaveTime = Convert.ToDateTime(leaveTimeString);
                    DateTime backTime = Convert.ToDateTime(backTimeString);
                    if (backTime <= leaveTime)
                    {
                        return Error("开始时间不能小于结束时间");
                    }
                    if (DuplicateCheck(accountInfo.userID, leaveTime, backTime))
                    {
                        return Error("请假时间与已有请假记录存在重叠！请重新输入。");
                    }
                    TimeSpan time_days = backTime - leaveTime;
                    int days = time_days.Days;

                    if (data.leaveTypeID.Trim() == "1")
                    {
                        #region
                        if (days < 3)
                        {
                            model.StudentID = accountInfo.userID;
                            model.LeaveType = data.leaveTypeID.Trim();
                            model.LeaveTypeChildrenID = "";
                            model.LeaveTime = leaveTime;
                            model.BackTime = backTime;
                            model.LeaveWay = data.leave_way.Trim();
                            model.BackWay = data.back_way.Trim();
                            model.Address = data.address.Trim();
                            model.Reason = data.leave_reason.Trim();
                        }
                        else
                        {
                            return Error("短期请假不能超过3天");
                        }
                        #endregion
                    }
                    else if (data.leaveTypeID.Trim() == "2")
                    {
                        #region
                        if (days >= 3)
                        {
                            model.StudentID = accountInfo.userID;
                            model.LeaveType = data.leaveTypeID.Trim();
                            model.LeaveTypeChildrenID = "";
                            model.LeaveTime = leaveTime;
                            model.BackTime = backTime;
                            model.LeaveWay = data.leave_way.Trim();
                            model.BackWay = data.back_way.Trim();
                            model.Address = data.address.Trim();
                            model.Reason = data.leave_reason.Trim();
                        }
                        else
                        {
                            return Error("长期请假不能少于3天");
                        }
                        #endregion
                    }
                    else if (data.leaveTypeID.Trim() == "4")
                    {
                        #region
                        model.StudentID = accountInfo.userID;
                        model.LeaveType = data.leaveTypeID.Trim();
                        model.LeaveTypeChildrenID = "";
                        model.LeaveTime = leaveTime;
                        model.BackTime = backTime;
                        model.LeaveWay = data.leave_way.Trim();
                        model.BackWay = data.back_way.Trim();
                        model.Address = data.address.Trim();
                        model.Reason = data.leave_reason.Trim();
                        #endregion
                    }
                    else
                    {
                        return Error("此接口仅支持短期请假、长期请假、节假日请假！");
                    }

                    if (Leave(model) == 1)
                    {
                        return Success("申请请加成功，请及时联系辅导员审批。");
                    }
                    else
                    {
                        return Error("保存至数据库失败！");
                    }
                }
                else
                {
                    return Error("此请假类型尚未开通，请联系辅导员！");
                }
                #endregion
            }
            catch(Exception ex)
            {
                return SystemError(ex);
            }
        }

        /// <summary>
        /// 特殊请假  包括 晚点名请假、早晚自习请假
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private ApiResult leaveSpecial(LeaveSpecialModel data)
        {
            try
            {
                AccountInfo accountInfo = GetAccountInfo(data.access_token);
                T_TeacherLeaveType leaveType = IsLeaveTypeExist(accountInfo.userID, Convert.ToInt32(data.leaveTypeID.Trim()));
                if (leaveType != null)
                {
                    T_New_LeaveList model = new T_New_LeaveList();

                    string leaveTimeString = data.leave_date + " " + "00:00:00";
                    DateTime leaveTime = Convert.ToDateTime(leaveTimeString);
                    if (DuplicateCheck(accountInfo.userID, data.leaveTypeID, leaveTime))
                    {
                        return Error("请假时间与已有请假记录存在重叠！请重新输入。");
                    }

                    if (data.leaveTypeID.Trim() == "5" || data.leaveTypeID.Trim() == "6" || data.leaveTypeID.Trim() == "7")
                    {
                        model.StudentID = accountInfo.userID;
                        model.LeaveType = data.leaveTypeID.Trim();
                        model.LeaveTypeChildrenID = data.leaveTypeChildrenID.Trim();
                        model.LeaveTime = leaveTime;
                        model.BackTime = leaveTime;
                        model.Reason = data.leave_reason.Trim();
                    }
                    else
                    {
                        return Error("此接口仅支持晚点名请假、早晚自习请假！");
                    }

                    if (Leave(model) == 1)
                    {
                        return Success("申请请加成功，请及时联系辅导员审批。");
                    }
                    else
                    {
                        return Error("保存至数据库失败！");
                    }
                }
                else
                {
                    return Error("此请假类型尚未开通，请联系辅导员！");
                }
            }
            catch (Exception ex)
            {
                return SystemError(ex);
            }
        }

        /// <summary>
        /// 上课请假备案
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private ApiResult leaveClass(LeaveClassModel data)
        {
            try
            {
                AccountInfo accountInfo = GetAccountInfo(data.access_token);
                T_TeacherLeaveType leaveType = IsLeaveTypeExist(accountInfo.userID, 8);
                if (leaveType != null)
                {
                    T_New_LeaveList model = new T_New_LeaveList();

                    model.StudentID = accountInfo.userID.Trim();
                    model.LeaveType = "8";
                    model.LeaveTypeChildrenID = data.leaveTypeChildrenID.Trim();
                    model.Lesson = data.lesson.Trim();

                    string leaveTime = "";
                    string backTime = "";
                    string lesson = data.lesson.Trim(); ;

                    if (lesson == "1")
                    {
                        leaveTime = "08:00";
                        backTime = "09:40";
                    }
                    if (lesson == "2")
                    {
                        leaveTime = "10:10";
                        backTime = "11:50";
                    }
                    if (lesson == "3")
                    {
                        leaveTime = "14:00";
                        backTime = "15:40";
                    }
                    if (lesson == "4")
                    {
                        leaveTime = "16:00";
                        backTime = "17:40";
                    }
                    if (lesson == "5")
                    {
                        leaveTime = "18:30";
                        backTime = "21:00";
                    }
                    DateTime _leaveTime = Convert.ToDateTime(data.leave_date.Trim() + " " + leaveTime);
                    DateTime _backTime = Convert.ToDateTime(data.leave_date.Trim() + " " + backTime);
                    model.Teacher = data.teacher_name.Trim();
                    model.LeaveTime = _leaveTime;
                    model.BackTime = _backTime;
                    model.Reason = data.leave_reason.Trim();

                    if (DuplicateCheck(accountInfo.userID, _leaveTime, data.lesson.Trim()))
                    {
                        return Error("请假时间与已有请假记录存在重叠！请重新输入。");
                    }

                    if (Leave(model) == 1)
                    {
                        return Success("申请请加成功，请及时联系辅导员审批。");
                    }
                    else
                    {
                        return Error("保存至数据库失败！");
                    }
                }
                else
                {
                    return Error("此请假类型尚未开通，请联系辅导员！");
                }
            }
            catch
            {
                return Error("此请假类型尚未开通，请联系辅导员！");
            }
        }

        /// <summary>
        /// 保存请假记录到数据库表格，返回值为插入请假记录成功条数
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private int Leave(T_New_LeaveList model)
        {
            string _LL_ID = "";

            #region 生成请假单号
            _LL_ID = DateTime.Now.ToString("yyMMdd");
            string endString = "0001";
            var leavelist = from T_New_LeaveList in db.T_New_LeaveList where (T_New_LeaveList.ID.StartsWith(_LL_ID)) orderby T_New_LeaveList.ID descending select T_New_LeaveList.ID;
            if (leavelist.Any())
            {
                string leaveNumTop = leavelist.First().ToString().Trim();
                int end = Convert.ToInt32(leaveNumTop.Substring(6, 4));
                end++;
                endString = end.ToString("0000");//按照此格式Tostring
            }
            _LL_ID += endString;
            #endregion

            model.ID = _LL_ID;
            model.SubmitTime = DateTime.Now;
            model.StateLeave = "0";
            model.StateBack = "0";
            model.RejectReason = "";
            model.IsDelete = 0;
            model.PrintTimes = "1";

            db.T_New_LeaveList.Add(model);
            if (db.SaveChanges() == 1)
            {
                //return Success("请假成功，请联系辅导员审批");
                return 1;
            }
            else
            {
                //return SystemError();
                return 0;
            }
        }
        #endregion

        #endregion
    }
}