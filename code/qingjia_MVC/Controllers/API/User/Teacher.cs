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
    public class Teacher : BaseApiController
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
                int a = db.vw_New_LeaveList.Where(q => q.ST_TeacherID.ToString().Trim() == accountInfo.userID && q.StateLeave == "0" && q.StateBack == "0" && q.IsDelete == 0).Count();
                int b = db.vw_New_LeaveList.Where(q => q.ST_TeacherID.ToString().Trim() == accountInfo.userID && q.StateLeave == "1" && q.StateBack == "0" && q.IsDelete == 0).Count();

                TeacherLeaveListPending data = new TeacherLeaveListPending();
                data.go = a;
                data.back = b;

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
                TeacherStudentLeave data = new TeacherStudentLeave();
                if (studentLeaveList.Any())
                {
                    HashSet<string> studentIDList = new HashSet<string>();
                    Dictionary<string, vw_Student> studentList = new Dictionary<string, vw_Student>();
                    foreach (var item in studentLeaveList)
                    {
                        if (item.LeaveType.Trim() == "1" || item.LeaveType.Trim() == "2" || item.LeaveType.Trim() == "3" || item.LeaveType.Trim() == "4")
                        {
                            //短期请假、长期请假、实习请假、节假日请假
                            vw_Student studentModel = db.vw_Student.Where(q => q.ST_Num.Trim() == item.StudentID.Trim()).ToList().First();
                            if (studentModel != null)
                            {
                                studentIDList.Add(item.StudentID.ToString().Trim());
                                studentList.Add(item.StudentID.Trim(), studentModel);
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
                if (loginInfo.Any())
                {
                    return Success("获取成功", loginInfo);
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
                if (leavelist.Any())
                {
                    TeacherLeaveListStatistic data = new TeacherLeaveListStatistic();
                    Dictionary<string, int> _statisticByLeavetype = new Dictionary<string, int>();
                    Dictionary<string, int> _statisticByClass = new Dictionary<string, int>();

                    int a = 0;  //短期请假
                    int b = 0;  //长期请假
                    int c = 0;  //实习请假
                    int d = 0;  //节假日请假
                    int e = 0;  //晚点名请假
                    int f = 0;  //早自习请假
                    int g = 0;  //晚自习请假
                    int h = 0;  //上课请假备案

                    a = leavelist.Where(q => q.LeaveType.Trim() == "1").Count();
                    b = leavelist.Where(q => q.LeaveType.Trim() == "2").Count();
                    c = leavelist.Where(q => q.LeaveType.Trim() == "3").Count();
                    d = leavelist.Where(q => q.LeaveType.Trim() == "4").Count();
                    e = leavelist.Where(q => q.LeaveType.Trim() == "5").Count();
                    f = leavelist.Where(q => q.LeaveType.Trim() == "6").Count();
                    g = leavelist.Where(q => q.LeaveType.Trim() == "7").Count();
                    h = leavelist.Where(q => q.LeaveType.Trim() == "8").Count();

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
                    if (classList.Any())
                    {
                        foreach (var item in classList)
                        {
                            int _num = leavelist.Where(q => q.ST_Class.ToString().Trim() == item.ClassName.ToString().Trim()).Count();
                            _statisticByClass.Add(item.ClassName.ToString().Trim(), _num);
                        }
                        data.statisticByLeavetype = _statisticByLeavetype;
                        data.statisticByClass = _statisticByClass;
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
    }
}