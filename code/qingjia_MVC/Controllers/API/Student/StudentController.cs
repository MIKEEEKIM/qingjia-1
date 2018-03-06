using qingjia_MVC.Common;
using qingjia_MVC.Models;
using qingjia_MVC.Models.API;
using qingjia_MVC.Models.API.Student;
using System;
using System.Linq;
using System.Web.Http;
using Z.EntityFramework.Plus;

namespace qingjia_MVC.Controllers.API
{
    [RoutePrefix("api/student")]
    public class StudentController : BaseApiController
    {
        /// <summary>
        /// GET
        /// 
        /// 获取学生基本信息
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpGet, Route("me")]
        public ApiResult Me(string access_token)
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
                vw_Student studentModel = db.vw_Student.Where(c => c.ST_Num == accountInfo.userID).ToList().First();
                if (studentModel == null)
                {
                    return SystemError();
                }

                //返回 数据模型
                StudentInfo returnStudentModel = new StudentInfo();

                #region 个人信息部分

                returnStudentModel.StudentID = (studentModel.ST_Num == null) ? "" : studentModel.ST_Num.ToString().Trim();
                returnStudentModel.ST_Name = (studentModel.ST_Name == null) ? "" : studentModel.ST_Name.ToString().Trim();
                returnStudentModel.ST_Class = (studentModel.ST_Class == null) ? "" : studentModel.ST_Class.ToString().Trim();
                returnStudentModel.ST_Grade = (studentModel.ST_Grade == null) ? "" : studentModel.ST_Grade.ToString().Trim();
                returnStudentModel.ST_TeacherID = (studentModel.ST_TeacherID == null) ? "" : studentModel.ST_TeacherID.ToString().Trim();
                returnStudentModel.ST_TeacherName = (studentModel.ST_Teacher == null) ? "" : studentModel.ST_Teacher.ToString().Trim();
                returnStudentModel.ST_Tel = (studentModel.ST_Tel == null) ? "" : studentModel.ST_Tel.ToString().Trim();
                returnStudentModel.ST_QQ = (studentModel.ST_QQ == null) ? "" : studentModel.ST_QQ.ToString().Trim();
                returnStudentModel.ST_Email = (studentModel.ST_Email == null) ? "" : studentModel.ST_Email.ToString().Trim();
                returnStudentModel.ST_Sex = (studentModel.ST_Sex == null) ? "" : studentModel.ST_Sex.ToString().Trim();
                returnStudentModel.ST_ContactName = (studentModel.ContactOne == null) ? "" : studentModel.ContactOne.ToString().Trim();
                returnStudentModel.ST_ContactTel = (studentModel.ST_Tel == null) ? "" : studentModel.ST_Tel.ToString().Trim();
                returnStudentModel.ST_Dor = (studentModel.ST_Dor == null) ? "" : studentModel.ST_Dor.ToString().Trim();

                var _leaveTypes = db.vw_TeacherLeaveType.Where(c => c.TeacherID == returnStudentModel.ST_TeacherID && c.TeacherGrade == returnStudentModel.ST_Grade).OrderBy(c => c.LeaveTypeID).ToList();
                if (_leaveTypes.Any())
                {
                    foreach (var item in _leaveTypes)
                    {
                        returnStudentModel.leaveTypes.Add(item.LeaveTypeID.ToString().Trim());
                    }
                }
                #endregion

                #region 年级周会信息及节假日信息
                vw_ClassBatch batchModel = db.vw_ClassBatch.Where(c => c.ClassName.Trim() == studentModel.ST_Class.Trim()).ToList().First();

                //TypeID = 1 代表节假日请假截止时间  TypeID = 2 代表年级周会请假截止时间
                T_Deadline weeklyMeetingDeadLine = db.T_Deadline.Where(c => c.TeacherID.Trim() == studentModel.ST_TeacherID.Trim() && c.TypeID == 2).ToList().First();
                T_Deadline holidayDeadLine = db.T_Deadline.Where(c => c.TeacherID.Trim() == studentModel.ST_TeacherID.Trim() && c.TypeID == 1).ToList().First();
                T_Holiday holidayModel = db.T_Holiday.Where(c => c.TeacherID.Trim() == studentModel.ST_TeacherID.Trim() && c.IsDelete == 0).ToList().First();

                if (batchModel != null && weeklyMeetingDeadLine != null)
                {
                    returnStudentModel.weeklyMeetingInfo.deadLine = weeklyMeetingDeadLine.Time.ToString("yyyy/MM/dd HH:mm:ss");
                    returnStudentModel.weeklyMeetingInfo.time = batchModel.Datetime.ToString("yyyy/MM/dd HH:mm:ss");
                    returnStudentModel.weeklyMeetingInfo.location = batchModel.Location.ToString().Trim();
                }
                if (holidayDeadLine != null && holidayModel != null)
                {
                    returnStudentModel.holidayInfo.startTime = ((DateTime)holidayModel.StartTime).ToString("yyyy/MM/dd HH:mm:ss");
                    returnStudentModel.holidayInfo.endTime = ((DateTime)holidayModel.EndTime).ToString("yyyy/MM/dd HH:mm:ss");
                    returnStudentModel.holidayInfo.deadLine = holidayDeadLine.Time.ToString("yyyy/MM/dd HH:mm:ss");
                }
                #endregion

                return Success("加载数据成功", returnStudentModel);
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
        /// 学生修改个人信息
        /// </summary>
        /// <param name="changeInfo"></param>
        /// <returns></returns>
        [HttpPost, Route("changeinfo")]
        public ApiResult ChangeInfo([FromBody]ChangeInfo model)
        {
            #region 令牌验证
            result = Check(model.access_token);
            if (result != null)
            {
                return result;
            }
            #endregion

            #region 检查参数
            if (model != null)
            {
                if (!model.CheckInfo())
                {
                    return Error("参数格式错误或缺少参数!");
                }
            }
            else
            {
                return Error("参数格式错误或缺少参数!");
            }
            #endregion

            #region 逻辑操作
            try
            {
                AccountInfo accountInfo = GetAccountInfo(model.access_token);
                if (accountInfo == null)
                {
                    return SystemError();
                }
                T_Student studentModel = db.T_Student.Find(accountInfo.userID);
                if (studentModel != null)
                {
                    studentModel.Tel = model.ST_Tel.Trim();

                    studentModel.QQ = model.ST_QQ.Trim();
                    studentModel.Email = model.ST_Email.Trim();
                    studentModel.ContactOne = model.ST_Guardian.Trim() + "-" + model.ST_GuardianName.Trim();
                    studentModel.OneTel = model.ST_GuardianTel.Trim();

                    db.SaveChanges();
                    return Success("修改个人信息成功", Me(model.access_token).data);
                }
                else
                {
                    return SystemError();
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
        /// 学生修改密码
        /// </summary>
        /// <param name="old_psd"></param>
        /// <param name="new_psd"></param>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpPost, Route("password")]
        public ApiResult PassWord([FromBody]ChangePassword model)
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
                if (model == null)
                {
                    return Error("参数格式错误或缺少参数！");
                }
                if (model.old_psd == "" || model.old_psd == null || model.new_psd == "" || model.new_psd == null)
                {
                    return Error("参数格式错误或缺少参数！");
                }
                if (model.new_psd.Length <= 6)
                {
                    return Error("新密码必须大于6位！");
                }

                AccountInfo accountInfo = GetAccountInfo(model.access_token);
                if (db.T_Account.Where(c => c.ID == accountInfo.userID && c.Psd == PsdEncryption.Encryption(model.old_psd)).Any())
                {
                    db.T_Account.Where(c => c.ID == accountInfo.userID).Update(c => new T_Account() { Psd = PsdEncryption.Encryption(model.new_psd) });
                    db.SaveChanges();

                    return Success("修改密码成功！");
                }
                else
                {
                    return Error("原密码错误！");
                }
            }
            catch
            {
                return SystemError();
            }
            #endregion
        }
    }
}
