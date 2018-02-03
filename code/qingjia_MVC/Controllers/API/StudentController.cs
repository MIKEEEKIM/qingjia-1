using qingjia_MVC.Common;
using qingjia_MVC.Models;
using qingjia_MVC.Models.API;
using System.Configuration;
using System.Linq;
using System.Web.Http;

namespace qingjia_MVC.Controllers.API
{
    #region 接收数据模型

    public class ChangePsdModel
    {
        public string old_psd { get; set; }
        public string new_psd { get; set; }
        public string access_token { get; set; }
    }

    public class ChangeInfoModel
    {
        public string access_token { get; set; }
        public string ST_Tel { get; set; }
        public string ST_QQ { get; set; }
        public string ST_Email { get; set; }
        public string ST_Guardian { get; set; }
        public string ST_GuardianName { get; set; }
        public string ST_GuardianTel { get; set; }
        public bool CheckInfo()
        {
            if (this.access_token == null || this.ST_Tel == null || this.ST_QQ == null || this.ST_Email == null || this.ST_Guardian == null || this.ST_GuardianName == null || this.ST_GuardianTel == null)
            {
                return false;
            }
            else
            {
                if (ST_Guardian == "父亲" || ST_Guardian == "母亲" || ST_Guardian == "其他")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }

    #endregion

    [RoutePrefix("api/student")]
    public class StudentController : ApiController
    {
        //实例化数据库
        private Entities db = new Entities();

        /// <summary>
        /// GET
        /// 
        /// 获取学生基本信息
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpGet, Route("me")]
        public ApiBaseResult Me(string access_token)
        {
            ApiBaseResult result = Check(access_token);
            if (result == null)
            {
                result = new ApiBaseResult();

                #region 获取用户信息

                string UserID = access_token.Substring(0, access_token.IndexOf("_"));

                var studentInfo = from vw_Student in db.vw_Student where (vw_Student.ST_Num == UserID) select vw_Student;
                if (studentInfo.Any())
                {
                    vw_Student student = studentInfo.ToList().First();

                    //创建数据模型
                    UserInfo userInfo = new UserInfo();
                    userInfo.UserID = student.ST_Num;
                    userInfo.UserName = student.ST_Name;
                    userInfo.UserClass = (student.ST_Class == null) ? "" : student.ST_Class;
                    userInfo.UserYear = (student.ST_Grade == null) ? "" : student.ST_Grade;
                    userInfo.UserTeacherID = (student.ST_TeacherID == null) ? "" : student.ST_TeacherID;
                    userInfo.UserTeacherName = (student.ST_Teacher == null) ? "" : student.ST_Teacher;
                    userInfo.UserTel = (student.ST_Tel == null) ? "" : student.ST_Tel;
                    userInfo.UserQQ = (student.ST_QQ == null) ? "" : student.ST_QQ;
                    userInfo.UserEmail = (student.ST_Email == null) ? "" : student.ST_Email;
                    userInfo.UserSex = (student.ST_Sex == null) ? "" : student.ST_Sex;
                    userInfo.UserDoor = (student.ST_Dor == null) ? "" : student.ST_Dor;
                    userInfo.ContactName = (student.ContactOne == null) ? "" : student.ContactOne;
                    userInfo.ContactTel = (student.OneTel == null) ? "" : student.OneTel;
                    userInfo.IsFreshman = (ConfigurationManager.AppSettings["FreshmanYear"].ToString().Trim() == student.ST_Grade.ToString().Trim()) ? "true" : "false";

                    result.result = "success";
                    result.data = userInfo;
                }
                else
                {
                    result.result = "error";
                    result.messages = "出现未知错误、请联系管理员";
                }

                #endregion

                return result;
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        /// POST
        /// 
        /// 学生修改个人信息
        /// </summary>
        /// <param name="changeInfo"></param>
        /// <returns></returns>
        [HttpPost, Route("changeinfo")]
        public ApiBaseResult ChangeInfo([FromBody]ChangeInfoModel changeInfo)
        {
            ApiBaseResult result;

            if (changeInfo != null)
            {
                if (!changeInfo.CheckInfo())
                {
                    result = new ApiBaseResult();
                    result.result = "error";
                    result.messages = "参数格式错误或缺少参数！";
                    return result;
                }
            }
            else
            {
                result = new ApiBaseResult();
                result.result = "error";
                result.messages = "参数格式错误或缺少参数！";
                return result;
            }

            result = Check(changeInfo.access_token);
            if (result == null)
            {
                result = new ApiBaseResult();

                string StudentID = changeInfo.access_token.Substring(0, changeInfo.access_token.IndexOf("_"));
                T_Student student = db.T_Student.Find(StudentID);
                student.Tel = changeInfo.ST_Tel;
                student.QQ = changeInfo.ST_QQ;
                student.Email = changeInfo.ST_Email;
                student.ContactOne = changeInfo.ST_Guardian + "-" + changeInfo.ST_GuardianName;
                student.OneTel = changeInfo.ST_GuardianTel;
                db.SaveChanges();

                result.result = "success";

                return result;
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        /// GET
        /// 
        /// 获取学生晚点名信息
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpGet, Route("night")]
        public ApiBaseResult Nigth(string access_token)
        {
            ApiBaseResult result = Check(access_token);
            if (result == null)
            {
                result = new ApiBaseResult();

                #region 获取数据

                string UserID = access_token.Substring(0, access_token.IndexOf("_"));

                NightInfo nightInfo = new NightInfo();

                var studentInfo = from vw_Student in db.vw_Student where (vw_Student.ST_Num == UserID) select vw_Student;
                if (studentInfo.Any())
                {
                    string className = studentInfo.ToList().First().ST_Class;
                    string TeacherID = studentInfo.ToList().First().ST_TeacherID;
                    nightInfo.TeacherID = TeacherID;
                    nightInfo.TeacherName = studentInfo.ToList().First().ST_Teacher;

                    var batchList = from vw_ClassBatch in db.vw_ClassBatch where (vw_ClassBatch.ClassName == className) select vw_ClassBatch;
                    if (batchList.Any())
                    {
                        nightInfo.BatchTime = batchList.ToList().First().Datetime.ToString();
                    }
                    else
                    {
                        result.result = "error";
                        result.messages = "未设置晚点名时间！";
                    }

                    T_Deadline deadLine = new T_Deadline();
                    try
                    {
                        deadLine = (from T_Deadline in db.T_Deadline where ((T_Deadline.TeacherID == TeacherID) && (T_Deadline.TypeID == 2)) select T_Deadline).ToList().First();
                        nightInfo.DeadLine = deadLine.Time.ToString();
                        result.result = "success";
                        result.data = nightInfo;
                    }
                    catch
                    {
                        result.result = "error";
                        result.messages = "未设置晚点名请假截止时间";
                    }
                }
                else
                {
                    result.result = "error";
                    result.messages = "出现未知错误、请联系管理员";
                }

                #endregion

                return result;
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        /// GET
        /// 
        /// 获取学生节假日信息
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpGet, Route("holiday")]
        public ApiBaseResult Holiaday(string access_token)
        {
            ApiBaseResult result = Check(access_token);
            if (result == null)
            {
                result = new ApiBaseResult();

                #region 获取数据

                Holiday holiday = new Holiday();

                string UserID = access_token.Substring(0, access_token.IndexOf("_"));
                var studentInfo = from vw_Student in db.vw_Student where (vw_Student.ST_Num == UserID) select vw_Student;
                if (studentInfo.Any())
                {
                    string TeacherID = studentInfo.ToList().First().ST_TeacherID;
                    holiday.TeacherID = TeacherID;

                    var deadLineList = from T_Deadline in db.T_Deadline where ((T_Deadline.TeacherID == TeacherID) && (T_Deadline.TypeID == 1)) select T_Deadline;
                    if (deadLineList.Any())
                    {
                        holiday.DeadLine = deadLineList.ToList().First().Time.ToString();
                        result.result = "success";
                        result.data = holiday;
                    }
                    else
                    {
                        result.result = "error";
                        result.messages = "未设置节假日请假截止时间，请联系辅导员";
                    }
                }
                else
                {
                    result.result = "error";
                    result.messages = "出现未知错误、请联系管理员";
                }

                #endregion

                return result;
            }
            else
            {
                return result;
            }
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
        public ApiBaseResult PassWord([FromBody]ChangePsdModel PsdModel)
        {
            ApiBaseResult result;

            if (PsdModel != null)
            {
                if (PsdModel.old_psd == null || PsdModel.new_psd == null || PsdModel.access_token == null)
                {
                    result = new ApiBaseResult();
                    result.result = "error";
                    result.messages = "参数格式错误或缺少参数！";
                    return result;
                }
            }
            else
            {
                result = new ApiBaseResult();
                result.result = "error";
                result.messages = "参数格式错误或缺少参数！";
                return result;
            }

            result = Check(PsdModel.access_token);
            if (result == null)
            {
                result = new ApiBaseResult();

                string StudentID = PsdModel.access_token.Substring(0, PsdModel.access_token.IndexOf("_"));
                T_Account account = db.T_Account.Find(StudentID);
                if (account.Psd == PsdModel.old_psd)
                {
                    account.Psd = PsdModel.new_psd;
                    db.SaveChanges();

                    result.result = "success";
                }
                else
                {
                    result.result = "error";
                    result.messages = "原密码错误，修改密码失败";
                }
                return result;
            }
            else
            {
                return result;
            }
        }

        #region 检查Access_Token
        /// <summary>
        /// 检查Access_Token
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        public ApiBaseResult Check(string access_token)
        {
            ApiBaseResult result = new ApiBaseResult();

            if (access_token != null)
            {
                string[] sArray = access_token.Split('_');
                string UserID = sArray[0];
                string GuidString = sArray[1];

                T_Account account = db.T_Account.Find(UserID);
                if (account != null)
                {
                    if (account.YB_AccessToken == GuidString)
                    {
                        return null;
                    }
                    else
                    {
                        result.result = "error";
                        result.messages = "Access_Token错误";
                        return result;
                    }
                }
                else
                {
                    result.result = "error";
                    result.messages = "Access_Token错误";
                    return result;
                }
            }
            else
            {
                result.result = "error";
                result.messages = "Access_Token为空，传值错误";
                return result;
            }
        }
        #endregion
    }
}
