using qingjia_MVC.Models;
using qingjia_MVC.Models.API;
using qingjia_MVC.Models.API.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace qingjia_MVC.Controllers.API.User
{
    /// <summary>
    /// 辅导员访问接口  获取相关数据 包括请销假情况、年级请假情况、登录情况、系统使用情况
    /// </summary>
    [RoutePrefix("api/teacher")]
    public class TeacherController : BaseApiController
    {
        /// <summary>
        /// 获取系统待处理请假情况
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpGet, Route("leavelistpending")]
        public ApiResult LeaveListPending(string access_token)
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
                List<vw_New_LeaveList> list = db.vw_New_LeaveList.Where(q => q.ST_TeacherID.ToString().Trim() == accountInfo.userID && q.IsDelete == 0).ToList();

                int a = list.Where(q => q.ST_TeacherID.ToString().Trim() == accountInfo.userID && q.StateLeave == "0" && q.StateBack == "0" && q.IsDelete == 0).Count();
                int b = list.Where(q => q.ST_TeacherID.ToString().Trim() == accountInfo.userID && q.StateLeave == "1" && q.StateBack == "0" && q.IsDelete == 0).Count();

                TeacherLeaveListPending data = new TeacherLeaveListPending
                {
                    go = a,
                    back = b
                };

                return Success("获取成功", data);
            }
            catch (Exception ex)
            {
                return SystemError(ex);
            }
            #endregion
        }

        /// <summary>
        /// 获取学生在校、离校情况
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpGet, Route("studentleave")]
        public ApiResult StudengLeave(string access_token)
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
                IQueryable<vw_New_LeaveList> studentLeaveList = db.vw_New_LeaveList.Where(q => q.ST_TeacherID.ToString().Trim() == accountInfo.userID && q.ST_Grade.ToString().Trim() == accountInfo.Grade && q.StateLeave.Trim() == "1" && q.StateBack.Trim() == "0").OrderByDescending(q => q.SubmitTime);
                List<vw_New_LeaveList> list = studentLeaveList.ToList();
                TeacherStudentLeave data = new TeacherStudentLeave();
                if (list.Any())
                {
                    HashSet<string> studentIDList = new HashSet<string>();
                    Dictionary<string, vw_Student> studentList = new Dictionary<string, vw_Student>();
                    foreach (var item in list)
                    {
                        if (item.LeaveType.Trim() == "1" || item.LeaveType.Trim() == "2" || item.LeaveType.Trim() == "3" || item.LeaveType.Trim() == "4")
                        {
                            //短期请假、长期请假、实习请假、节假日请假
                            vw_Student studentModel = db.vw_Student.Where(q => q.ST_Num.Trim() == item.StudentID.Trim()).ToList().First();
                            if (studentModel != null)
                            {
                                if (studentIDList.Add(item.StudentID.ToString().Trim()))
                                {
                                    studentList.Add(item.StudentID.Trim(), studentModel);
                                }
                            }
                        }
                    }
                    data.studentIDList = studentIDList.ToList();
                    data.studentList = studentList;
                    return Success("获取成功", data);
                }
                else
                {
                    return Success("获取成功", data);
                }
            }
            catch (Exception ex)
            {
                return SystemError(ex);
            }
            #endregion
        }

        /// <summary>
        /// 获取系统登录情况
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpGet, Route("systemlogin")]
        public ApiResult SystemLogin(string access_token)
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
                IQueryable<T_LoginInfo> loginInfo = db.T_LoginInfo.Where(q => q.grade.Trim() == accountInfo.Grade).OrderByDescending(q => q.loginTime);
                List<T_LoginInfo> list = loginInfo.ToList();
                if (list.Any())
                {
                    return Success("获取成功", list);
                }
                else
                {
                    return Success("暂无登录数据", null);
                }
            }
            catch (Exception ex)
            {
                return SystemError(ex);
            }
            #endregion
        }

        /// <summary>
        /// 获取年级请假状况
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpGet, Route("leaveliststatistic")]
        public ApiResult SystemLeaveListStatistic(string access_token)
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
                AccountInfo accountinfo = GetAccountInfo(access_token);
                //各类型请假数据、各班级请假数据
                IQueryable<vw_New_LeaveList> leavelist = db.vw_New_LeaveList.Where(q => q.ST_Grade == accountinfo.Grade).OrderByDescending(q => q.SubmitTime);
                //IQueryable<vw_New_LeaveList> leavelist = db.vw_New_LeaveList.Where(q => q.ST_Grade == "2017").OrderByDescending(q => q.SubmitTime);
                List<vw_New_LeaveList> list = leavelist.ToList();
                if (list.Any())
                {
                    TeacherLeaveListStatistic data = new TeacherLeaveListStatistic();
                    Dictionary<string, int> _statisticByLeavetype = new Dictionary<string, int>();
                    Dictionary<string, int> _statisticByClass = new Dictionary<string, int>();
                    Dictionary<string, Dictionary<string, int>> _statisticByClassLeaveType = new Dictionary<string, Dictionary<string, int>>();

                    int a = 0;  //短期请假
                    int b = 0;  //长期请假
                    int c = 0;  //实习请假
                    int d = 0;  //节假日请假
                    int e = 0;  //晚点名请假
                    int f = 0;  //早自习请假
                    int g = 0;  //晚自习请假
                    int h = 0;  //上课请假备案

                    a = list.Where(q => q.LeaveType.Trim() == "1").Count();
                    b = list.Where(q => q.LeaveType.Trim() == "2").Count();
                    c = list.Where(q => q.LeaveType.Trim() == "3").Count();
                    d = list.Where(q => q.LeaveType.Trim() == "4").Count();
                    e = list.Where(q => q.LeaveType.Trim() == "5").Count();
                    f = list.Where(q => q.LeaveType.Trim() == "6").Count();
                    g = list.Where(q => q.LeaveType.Trim() == "7").Count();
                    h = list.Where(q => q.LeaveType.Trim() == "8").Count();

                    _statisticByLeavetype.Add("短期请假", a);
                    _statisticByLeavetype.Add("长期请假", b);
                    _statisticByLeavetype.Add("实习请假", c);
                    _statisticByLeavetype.Add("节假日请假", d);
                    _statisticByLeavetype.Add("晚点名请假", e);
                    _statisticByLeavetype.Add("早自习请假", f);
                    _statisticByLeavetype.Add("晚自习请假", g);
                    _statisticByLeavetype.Add("上课请假备案", h);

                    //按班级统计数据
                    IQueryable<vw_Class> classList = db.vw_Class.Where(q => q.TeacherID.ToString().Trim() == accountinfo.userID && q.Grade.Trim() == accountinfo.Grade).OrderBy(q => q.ClassName);
                    List<vw_Class> _classlist = classList.ToList();
                    IQueryable<T_LeaveType> leaveTypeList = db.T_LeaveType.OrderBy(q => q.ID);
                    List<T_LeaveType> _leaveTypeList = leaveTypeList.ToList();
                    if (_classlist.Any() && _leaveTypeList.Any())
                    {
                        foreach (var _item in _leaveTypeList)
                        {
                            Dictionary<string, int> leaveTypeData = new Dictionary<string, int>();
                            foreach (var item in _classlist)
                            {
                                int _num = list.Where(q => q.ST_Class.ToString().Trim() == item.ClassName.ToString().Trim() && q.LeaveType.Trim() == _item.ID.ToString().Trim()).Count();

                                leaveTypeData.Add(item.ClassName, _num);
                            }
                            _statisticByClassLeaveType.Add(_item.Name.ToString().Trim(), leaveTypeData);
                        }
                        foreach (var item in _classlist)
                        {
                            int _num = list.Where(q => q.ST_Class.ToString().Trim() == item.ClassName.ToString().Trim()).Count();

                            _statisticByClass.Add(item.ClassName, _num);
                        }
                        data.statisticByLeavetype = _statisticByLeavetype;
                        data.statisticByClass = _statisticByClass;
                        data.statisticByClassLeaveType = _statisticByClassLeaveType;
                        return Success("获取成功", data);
                    }
                    else
                    {
                        data.statisticByLeavetype = _statisticByLeavetype;
                        return Success("获取成功", data);
                    }
                }
                else
                {
                    return Success("获取成功");
                }
            }
            catch (Exception ex)
            {
                return SystemError(ex);
            }
            #endregion
        }

        /// <summary>
        /// 获取年级周会相关设置信息
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpGet, Route("getmeetinginfo")]
        public ApiResult GetMeetingInfo(string access_token)
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
                if (accountInfo.userRoleID == "3")
                {
                    return Success("获取成功", Get_MeetingInfoTeacherID(accountInfo.userID));
                }
                if (accountInfo.userRoleID == "1")
                {
                    return Success("获取成功", Get_MeetingInfoStudentID(accountInfo.userID));
                }
                return Error("此数据接口不支持班级账号！");
            }
            catch (Exception ex)
            {
                return SystemError(ex);
            }
            #endregion
        }

        /// <summary>
        /// 获取节假日相关设置信息
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpGet, Route("getholidayinfo")]
        public ApiResult GetHolidayInfo(string access_token)
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
                if (accountInfo.userRoleID == "3")
                {
                    return Success("获取成功", Get_HolidayInfo(accountInfo.userID));
                }
                if (accountInfo.userRoleID == "1")
                {
                    vw_Student studentModel = db.vw_Student.Where(q => q.ST_Num.Trim() == accountInfo.userID).ToList().First();
                    return Success("获取成功", Get_HolidayInfo(studentModel.ST_TeacherID.Trim()));
                }
                return Error("此数据接口不支持班级账号！");
            }
            catch (Exception ex)
            {
                return SystemError(ex);
            }
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpGet, Route("getteacherindexdata")]
        public ApiResult GetTeacherIndexData(string access_token)
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
                List<ApiResult> data = new List<ApiResult>
                {
                    LeaveListPending(access_token),
                    StudengLeave(access_token),
                    SystemLogin(access_token),
                    SystemLeaveListStatistic(access_token),
                    GetMeetingInfo(access_token),
                    GetHolidayInfo(access_token)
                };
                return Success("获取成功", data);
            }
            catch (Exception ex)
            {
                return SystemError(ex);
            }
            #endregion
        }

        private TeacherMeetingHolidayInfo Get_MeetingInfoTeacherID(string teacherID)
        {
            try
            {
                TeacherMeetingHolidayInfo data = new TeacherMeetingHolidayInfo();
                List<vw_ClassBatch> list = db.vw_ClassBatch.Where(q => q.TeacherID.Trim() == teacherID).OrderBy(q => q.Batch).ToList();
                if (list.Any())
                {
                    vw_ClassBatch model = list.First();
                    data.meetingSetted = true;
                    data.meetingDate = ((DateTime)model.Datetime).ToString("yyyy-MM-dd");
                    data.meetingDeadline = ((DateTime)model.DeadLine).ToString("yyyy-MM-dd HH:mm:ss");
                    if (model.DeadLine > DateTime.Now)
                    {
                        data.meetingDeadline = ((DateTime)model.DeadLine).ToString("yyyy-MM-dd HH:mm:ss");
                        TimeSpan ts = (DateTime)model.DeadLine - DateTime.Now;
                        data.meetingPercentage = 100 - (int)Math.Round(100 * ts.TotalSeconds / (double)(model.AutoUpdateTimeSpan * 3600 * 24));
                    }
                    else
                    {
                        data.meetingDeadline = "请假已截止";
                        data.meetingPercentage = 100;
                    }
                    data.meetingLocation = model.Location;
                    data.meetingTimeSpan = model.AutoUpdateTimeSpan + "天";
                }
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private TeacherMeetingHolidayInfo Get_MeetingInfoStudentID(string studentID)
        {
            try
            {
                TeacherMeetingHolidayInfo data = new TeacherMeetingHolidayInfo();
                List<vw_StudentClassBatch> list = db.vw_StudentClassBatch.Where(q => q.ST_Num.Trim() == studentID).OrderBy(q => q.Batch).ToList();

                if (list.Any())
                {
                    vw_StudentClassBatch model = list.First();
                    data.meetingSetted = true;
                    data.meetingDate = ((DateTime)model.Datetime).ToString("yyyy-MM-dd HH:mm:ss");
                    data.meetingDeadline = ((DateTime)model.DeadLine).ToString("yyyy-MM-dd HH:mm:ss");
                    if (model.DeadLine > DateTime.Now)
                    {
                        data.meetingDeadline = ((DateTime)model.DeadLine).ToString("yyyy-MM-dd HH:mm:ss");
                        TimeSpan ts = (DateTime)model.DeadLine - DateTime.Now;
                        data.meetingPercentage = 100 - (int)Math.Round(100 * ts.TotalSeconds / (double)(model.AutoUpdateTimeSpan * 3600 * 24));
                    }
                    else
                    {
                        data.meetingDeadline = "请假已截止";
                        data.meetingPercentage = 100;
                    }
                    data.meetingLocation = model.Location;
                    data.meetingTimeSpan = model.AutoUpdateTimeSpan + "天";
                }
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private TeacherMeetingHolidayInfo Get_HolidayInfo(string teacherID)
        {
            try
            {
                TeacherMeetingHolidayInfo data = new TeacherMeetingHolidayInfo();
                List<vw_Holiday> holidayInfoList = db.vw_Holiday.Where(q => q.TeacherID.Trim() == teacherID && q.IsDelete == 0).ToList();
                if (holidayInfoList.Any())
                {
                    vw_Holiday model = holidayInfoList.First();
                    data.holidaySetted = true;
                    data.holidayName = model.Name.Trim();
                    data.holidayDeadLine = ((DateTime)model.DeadLine).ToString("yyyy-MM-dd HH:mm:ss");
                    if (model.DeadLine > DateTime.Now)
                    {
                        data.holidayDeadLine = ((DateTime)model.DeadLine).ToString("yyyy-MM-dd HH:mm:ss");
                        TimeSpan ts = (DateTime)model.DeadLine - DateTime.Now;
                        TimeSpan _ts = (DateTime)model.DeadLine - (DateTime)model.SubmitTime;
                        data.holidatPercentage = 100 - (int)Math.Round(100 * ts.TotalSeconds / _ts.TotalSeconds);
                    }
                    else
                    {
                        data.holidayDeadLine = "请假已截止";
                        data.holidatPercentage = 100;
                    }
                    data.holidayStartDate = ((DateTime)model.StartTime).ToString("yyyy-MM-dd");
                    data.holidayEndDate = ((DateTime)model.EndTime).ToString("yyyy-MM-dd");
                }
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}