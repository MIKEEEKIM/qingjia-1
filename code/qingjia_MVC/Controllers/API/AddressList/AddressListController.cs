using qingjia_MVC.Models;
using qingjia_MVC.Models.API;
using qingjia_MVC.Models.API.AddressList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace qingjia_MVC.Controllers.API.AddressList
{
    [RoutePrefix("api/addresslist")]
    public class AddressListController : BaseApiController
    {
        /// <summary>
        /// Get 获取学生通讯录
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet, Route("get")]
        public ApiResult Get(string access_token, string classID, int page, int pageSize)
        {
            #region 令牌验证
            result = Check(access_token);
            if (result != null)
            {
                return result;
            }
            #endregion

            #region 逻辑操作
            //判断角色类型 返回数据
            try
            {
                AccountInfo accountInfo = GetAccountInfo(access_token);
                IQueryable<vw_Student> studentList = db.vw_Student;
                SelectCondition conditionsModel = new SelectCondition();
                if (accountInfo.userRoleID == "1")
                {
                    //学生
                    vw_Student studentModel = db.vw_Student.Where(c => c.ST_Num == accountInfo.userID).ToList().First();
                    studentList = db.vw_Student.Where(c => c.ST_Grade.ToString().Trim() == accountInfo.Grade.ToString().Trim() && c.ST_Class.ToString().Trim() == studentModel.ST_Class.ToString().Trim());
                }
                else if (accountInfo.userRoleID == "2")
                {
                    //班级账号
                    vw_Class classModel = db.vw_Class.Where(c => c.ID == accountInfo.userID).ToList().First();
                    studentList = db.vw_Student.Where(c => c.ST_Grade.ToString().Trim() == accountInfo.Grade.ToString().Trim() && c.ST_Class.ToString().Trim() == classModel.ClassName.ToString().Trim());
                }
                else if (accountInfo.userRoleID == "3")
                {
                    //辅导员
                    studentList = db.vw_Student.Where(c => c.ST_Grade.ToString().Trim() == accountInfo.Grade.ToString().Trim() && c.ST_TeacherID.ToString().Trim() == accountInfo.userID.ToString().Trim());
                }

                //根据班级检索
                if (accountInfo.userRoleID == "3" && classID.Trim() != "0")
                {
                    studentList = studentList.Where(c => c.ClassID == classID.Trim());
                }

                studentList = studentList.OrderBy(c => c.ST_Class).ThenBy(c => c.ST_Sex).ThenBy(c => c.ST_Num);

                if (page != 0 && pageSize != 0)
                {
                    conditionsModel.page = page;
                    conditionsModel.pageSize = pageSize;
                }

                DataList dtSource = GetList(conditionsModel, studentList);
                dtSource.list = TransformStudentList((List<vw_Student>)dtSource.list, accountInfo.userRoleID);

                return Success("获取成功", dtSource);
            }
            catch (Exception ex)
            {
                return SystemError(ex);
            }
            #endregion
        }

        /// <summary>
        /// Get 查询某一学生信息  需要辅导员权限
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="studentID"></param>
        /// <returns></returns>
        [HttpGet, Route("get")]
        public ApiResult Get(string access_token, string studentID)
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

                IQueryable<vw_Student> studentList = db.vw_Student;
                studentList = studentList.Where(c => c.ST_TeacherID == accountInfo.userID && c.ST_Num == studentID);

                if (studentList.Any())
                {
                    //查询此学生详细信息

                    //学生个人信息
                    DataList dtSource = new DataList();
                    dtSource.total = 1;
                    dtSource.list = studentList.ToList();
                    dtSource.list = TransformStudentList((List<vw_Student>)dtSource.list, accountInfo.userRoleID);

                    //在校状态
                    //请假系统使用记录 未销假、被驳回、、请假类型分布

                    IQueryable<vw_New_LeaveList> studentLeaveList = db.vw_New_LeaveList.Where(q => q.StudentID == studentID).OrderByDescending(q => q.SubmitTime);
                    bool IsLeave = false; //false代表在校状态  true代表离校状态
                    int a = 0;  //短期请假
                    int b = 0;  //长期请假
                    int c = 0;  //实习请假
                    int d = 0;  //节假日请假
                    int e = 0;  //晚点名请假
                    int f = 0;  //早自习请假
                    int g = 0;  //晚自习请假
                    int h = 0;  //上课请假备案
                    int m = 0;  //待销假
                    List<LeaveStatisticModel> _leaveNumInfo = new List<LeaveStatisticModel>();

                    if (studentLeaveList.Any())
                    {
                        foreach (var item in studentLeaveList)
                        {
                            if (item.LeaveType.Trim() == "1") { a++; }
                            if (item.LeaveType.Trim() == "2") { b++; }
                            if (item.LeaveType.Trim() == "3") { c++; }
                            if (item.LeaveType.Trim() == "4") { d++; }
                            if (item.LeaveType.Trim() == "5") { e++; }
                            if (item.LeaveType.Trim() == "6") { f++; }
                            if (item.LeaveType.Trim() == "7") { g++; }
                            if (item.LeaveType.Trim() == "8") { h++; }

                            if (item.StateLeave.Trim() == "1" && item.StateBack.Trim() == "0")
                            {
                                m++;
                                if (item.LeaveType.Trim() == "2" || item.LeaveType.Trim() == "3" || item.LeaveType.Trim() == "4")
                                {
                                    IsLeave = true; //true 代表该学生处于离校状态
                                }
                            }
                        }

                        _leaveNumInfo.Add(new LeaveStatisticModel("1", "短期请假", a));
                        _leaveNumInfo.Add(new LeaveStatisticModel("2", "长期请假", b));
                        _leaveNumInfo.Add(new LeaveStatisticModel("3", "实习请假", c));
                        _leaveNumInfo.Add(new LeaveStatisticModel("4", "节假日请假", d));
                        _leaveNumInfo.Add(new LeaveStatisticModel("5", "晚点名请假", e));
                        _leaveNumInfo.Add(new LeaveStatisticModel("6", "早自习请假", f));
                        _leaveNumInfo.Add(new LeaveStatisticModel("7", "晚自习请假", g));
                        _leaveNumInfo.Add(new LeaveStatisticModel("8", "上课请假备案", h));
                        //res.leaveNumInfo = _leaveNumInfo;
                        //res.total = a + b + c + d + e + f + g + h;
                    }
                    else
                    {
                        //系统中暂无 此学生的请假记录、在校状态->在校
                    }

                    #region
                    StudentAddressInfo res = new StudentAddressInfo(studentList.ToList().First()) { IsLeave = IsLeave };
                    res.leaveNumInfo = _leaveNumInfo;
                    res.total = a + b + c + d + e + f + g + h;

                    #endregion

                    return Success("获取成功", res);
                }
                else
                {
                    return Error("未找到此学生详细信息!");
                }
            }
            catch (Exception ex)
            {
                return SystemError(ex);
            }
            #endregion
        }

        /// <summary>
        /// Post 获取学生通讯录
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
                IQueryable<vw_Student> studentList = db.vw_Student;
                if (accountInfo.userRoleID == "1")
                {
                    //学生
                    vw_Student studentModel = db.vw_Student.Where(c => c.ST_Num == accountInfo.userID).ToList().First();
                    studentList = db.vw_Student.Where(c => c.ST_Grade.ToString().Trim() == accountInfo.Grade.ToString().Trim() && c.ST_Class.ToString().Trim() == studentModel.ST_Class.ToString().Trim());
                }
                else if (accountInfo.userRoleID == "2")
                {
                    //班级账号
                    vw_Class classModel = db.vw_Class.Where(c => c.ID == accountInfo.userID).ToList().First();
                    studentList = db.vw_Student.Where(c => c.ST_Grade.ToString().Trim() == accountInfo.Grade.ToString().Trim() && c.ST_Class.ToString().Trim() == classModel.ClassName.ToString().Trim());
                }
                else if (accountInfo.userRoleID == "3")
                {
                    //辅导员
                    studentList = db.vw_Student.Where(c => c.ST_Grade.ToString().Trim() == accountInfo.Grade.ToString().Trim() && c.ST_TeacherID.ToString().Trim() == accountInfo.userID.ToString().Trim());
                }

                studentList = studentList.OrderBy(c => c.ST_Class).ThenBy(c => c.ST_Sex).ThenBy(c => c.ST_Num);

                DataList dtSource = GetList(model, studentList);
                dtSource.list = TransformStudentList((List<vw_Student>)dtSource.list, accountInfo.userRoleID);

                return Success("获取成功", dtSource);
            }
            catch
            {
                return SystemError();
            }
            #endregion
        }
    }
}
