using qingjia_MVC.Common;
using qingjia_MVC.Models;
using qingjia_MVC.Models.API;
using qingjia_MVC.Models.API.User;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;

/// <summary>
/// 登录 注销 忘记密码 （登录时应有系统自检功能）
/// </summary>
namespace qingjia_MVC.Controllers.API.User
{
    [RoutePrefix("api/user")]
    public class LoginController : BaseApiController
    {
        /// <summary>
        /// 登录账号
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("login")]
        public ApiResult Login([FromBody]LoginModel model)
        {
            #region 登录验证
            try
            {
                int times = 1;//代表登录次数

                //验证密码、生成令牌、将个人信息写入缓存中
                if (model == null || model.id == null || model.id.ToString().Trim() == "" || model.password == null || model.password.ToString().Trim() == "")
                {
                    return Error("参数错误，请重新输入！");
                }

                var accountList = db.T_Account.Where(c => c.ID == model.id.ToString().Trim());
                if (accountList.Any())
                {
                    T_Account accountModel = accountList.ToList().First();
                    LoginStateModel loginState = (LoginStateModel)HttpRuntime.Cache.Get(model.id.ToString().Trim());
                    if (loginState != null)
                    {
                        if (loginState.times == -1)
                        {
                            //已登录账号、不需要重新登录，请退出后再登录
                            HttpRuntime.Cache.Remove(loginState.access_token);
                            HttpRuntime.Cache.Remove(model.id.ToString().Trim());
                            //return Error("账号已在其他地方登录，请重新尝试！");
                        }
                        else if (loginState.times < 3)
                        {
                            times = loginState.times + 1;
                        }
                        else if (loginState.times == 3)
                        {
                            return Error("已输入密码错误3次，请1分钟之后再次尝试。");
                        }
                    }

                    if (PsdEncryption.Encryption(model.password.ToString().Trim()) == accountModel.Psd)
                    {
                        ReturnUserModel data = new ReturnUserModel();
                        AccountInfo accountInfo = new AccountInfo();

                        //string _access_token = Guid.NewGuid().ToString();
                        string _access_token = "11";
                        data.access_token = _access_token;
                        data.UserID = accountModel.ID.ToString().Trim();
                        data.RoleID = accountModel.RoleID.ToString().Trim();
                        if (accountModel.RoleID.ToString().Trim() == "1")
                        {
                            vw_Student studentModel = db.vw_Student.Where(c => c.ST_Num == accountModel.ID).ToList().First();
                            data.UserName = studentModel.ST_Name.Trim();
                            accountInfo.userRoleName = "学生";
                            accountInfo.Grade = studentModel.ST_Grade.ToString().Trim();
                            List<Dictionary<string, string>> leaveTypeSetting = GetLeaveTypeSetting(studentModel.ST_TeacherID.ToString().Trim());
                            accountInfo.leaveTypeList = leaveTypeSetting[0];
                            accountInfo.enableMessageLeaveList = leaveTypeSetting[1];
                            if (db.vw_Class.Where(q => q.MonitorID.Trim() == accountModel.ID).Any())
                            {
                                //此学生为班级负责人
                                data.IsMonitor = "1";
                            }
                        }
                        if (accountModel.RoleID.ToString().Trim() == "2")
                        {
                            T_Class classModel = db.T_Class.Where(c => c.ID == accountModel.ID).ToList().First();
                            data.UserName = classModel.ClassName.Trim();
                            accountInfo.userRoleName = "班级账号";
                            accountInfo.Grade = classModel.Grade.ToString().Trim();
                        }
                        if (accountModel.RoleID.ToString().Trim() == "3")
                        {
                            T_Teacher teacherModel = db.T_Teacher.Where(c => c.ID == accountModel.ID).ToList().First();
                            data.UserName = teacherModel.Name.Trim();
                            accountInfo.userRoleName = "辅导员";
                            accountInfo.Grade = teacherModel.Grade.ToString().Trim();

                            List<Dictionary<string, string>> leaveTypeSetting = GetLeaveTypeSetting(teacherModel.ID.ToString().Trim());
                            accountInfo.leaveTypeList = leaveTypeSetting[0];
                            accountInfo.enableMessageLeaveList = leaveTypeSetting[1];
                        }

                        accountInfo.access_token = data.access_token;
                        accountInfo.userID = data.UserID;
                        accountInfo.userName = data.UserName;
                        accountInfo.userRoleID = data.RoleID;
                        accountInfo.permissionList = null;

                        LoginRecord(accountInfo.userID, accountInfo.userName, Convert.ToInt32(accountInfo.userRoleID), accountInfo.userRoleName, accountInfo.Grade, 0);

                        HttpRuntime.Cache.Insert(accountInfo.access_token, accountInfo, null, DateTime.MaxValue, TimeSpan.FromMinutes(Convert.ToInt32(ConfigurationManager.AppSettings["CacheSpanTime"].ToString())));
                        HttpRuntime.Cache.Insert(model.id.ToString().Trim(), new LoginStateModel { access_token = _access_token, times = -1 }, null, DateTime.MaxValue, TimeSpan.FromMinutes(Convert.ToInt32(ConfigurationManager.AppSettings["CacheSpanTime"].ToString())));

                        return Success("登陆成功！", data);
                    }
                    else
                    {
                        HttpRuntime.Cache.Insert(model.id.ToString().Trim(), new LoginStateModel { access_token = "", times = times }, null, DateTime.MaxValue, TimeSpan.FromMinutes(1));
                        return Error("密码错误！已输入错误" + times + "次，请重新输入！");
                    }
                }
                else
                {
                    return Error("此用户ID不存在！");
                }
            }
            catch (Exception ex)
            {
                return SystemError(ex);
            }
            #endregion
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpGet, Route("logout")]
        public ApiResult Logout(string access_token)
        {
            try
            {
                result = Check(access_token);
                if (result != null)
                {
                    return result;
                }

                AccountInfo accountInfo = GetAccountInfo(access_token);
                HttpRuntime.Cache.Remove(accountInfo.userID);
                HttpRuntime.Cache.Remove(access_token);

                return Success("已注销！");
            }
            catch
            {
                return SystemError();
            }
        }

        /// <summary>
        /// 管理员账号登录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("adminlogin")]
        public ApiResult LoginAdmin([FromBody]LoginModel model)
        {
            #region 参数检验
            if (model.id.Trim() == "" || model.password.Trim() == "")
            {
                return Error("管理员账户和密码不能为空！");
            }
            #endregion

            #region 逻辑操作
            try
            {
                int times = 1;//代表登录次数

                LoginStateModel loginState = (LoginStateModel)HttpRuntime.Cache.Get(model.id.ToString().Trim());
                if (loginState != null)
                {
                    if (loginState.times == -1)
                    {
                        //已登录账号、不需要重新登录，请退出后再登录
                        HttpRuntime.Cache.Remove(loginState.access_token);
                        HttpRuntime.Cache.Remove(model.id.ToString().Trim());
                        //return Error("账号已在其他地方登录，请重新尝试！");
                    }
                    else if (loginState.times < 3)
                    {
                        times = loginState.times + 1;
                    }
                    else if (loginState.times == 3)
                    {
                        return Error("已输入密码错误3次，请1分钟之后再次尝试。");
                    }
                }


                ReturnUserModel data = new ReturnUserModel();

                string adminID = ConfigurationManager.AppSettings["adminID"].ToString();
                string adminPsd = ConfigurationManager.AppSettings["adminPsd"].ToString();

                if (model.id == adminID && model.password == adminPsd)
                {
                    //登陆成功
                    string _access_token = Guid.NewGuid().ToString();
                    AccountInfo accountInfo = new AccountInfo
                    {
                        userID = model.id,
                        userName = "管理员",
                        userRoleID = "0",
                        userRoleName = "管理员",
                        access_token = _access_token
                    };

                    LoginRecord(model.id, "管理员", 0, "管理员", "", 0);

                    HttpRuntime.Cache.Insert(accountInfo.access_token, accountInfo, null, DateTime.MaxValue, TimeSpan.FromMinutes(Convert.ToInt32(ConfigurationManager.AppSettings["CacheSpanTime"].ToString())));
                    HttpRuntime.Cache.Insert(model.id.ToString().Trim(), new LoginStateModel { access_token = _access_token, times = -1 }, null, DateTime.MaxValue, TimeSpan.FromMinutes(Convert.ToInt32(ConfigurationManager.AppSettings["CacheSpanTime"].ToString())));

                    data.access_token = _access_token;
                    data.UserID = model.id.ToString().Trim();
                    data.UserName = "管理员";
                    data.RoleID = "0";

                    return Success("登陆成功！", data);
                }
                else
                {
                    HttpRuntime.Cache.Insert(model.id.ToString().Trim(), new LoginStateModel { access_token = "", times = times }, null, DateTime.MaxValue, TimeSpan.FromMinutes(1));
                    return Error("管理员账户或密码错误！已输入错误" + times + "次，请重新输入！");
                }
            }
            catch (Exception ex)
            {
                return SystemError(ex);
            }
            #endregion
        }

        #region 忘记密码 尚未完成
        [HttpGet, Route("forgetpsd")]
        public ApiResult ForgetPassWord(string id)
        {
            string tel = "15307179930";

            try
            {
                T_Account accountModel = db.T_Account.Where(c => c.ID.ToString().Trim() == id.ToString().Trim()).ToList().First();
                if (accountModel != null)
                {
                    if (accountModel.RoleID.ToString().Trim() == "1")
                    {
                        T_Student studentModel = db.T_Student.Where(c => c.ID.ToString().Trim() == id.ToString().Trim()).ToList().First();

                        //发送验证码短信



                        return Success("验证码短信发送至" + tel);
                    }
                    if (accountModel.RoleID.ToString().Trim() == "2")
                    {
                        return Error("暂不支持班级账号");
                    }
                    if (accountModel.RoleID.ToString().Trim() == "3")
                    {
                        return Error("暂不支持辅导员账号");
                    }
                }
                else
                {
                    return Error("此账号不存在，请重新输入！");
                }
            }
            catch (Exception ex)
            {
                return SystemError(ex);
            }

            return null;
        }

        [HttpGet, Route("forgetpsd")]
        public ApiResult ForgetPassWord(string id, string code)
        {

            return null;
        }
        #endregion

        #region 获取开启的请假类型和短息服务信息
        private List<Dictionary<string, string>> GetLeaveTypeSetting(string teacherID)
        {
            List<Dictionary<string, string>> returnData = new List<Dictionary<string, string>>();
            Dictionary<string, string> leaveTypeList = new Dictionary<string, string>();
            Dictionary<string, string> enableMessageList = new Dictionary<string, string>();

            IQueryable<vw_TeacherLeaveType> leaveTypeSettings = db.vw_TeacherLeaveType.Where(q => q.IsDelete == 0 && q.TeacherID.Trim() == teacherID).OrderBy(q => q.LeaveTypeID);
            if (leaveTypeSettings.Any())
            {
                foreach (var item in leaveTypeSettings)
                {
                    leaveTypeList.Add(item.LeaveTypeID.ToString().Trim(), item.LeaveTypeName);
                    if (item.EnableMessage == 1)
                    {
                        enableMessageList.Add(item.LeaveTypeID.ToString().Trim(), item.LeaveTypeName);
                    }
                }
            }
            returnData.Add(leaveTypeList);
            returnData.Add(enableMessageList);
            return returnData;
        }
        #endregion

        #region 登录记录
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="userName">用户名</param>
        /// <param name="roleID">用户角色ID</param>
        /// <param name="roleName">用户角色名</param>
        /// <param name="grade">用户年级</param>
        /// <param name="loginPort">登陆端口  0-网页端  1-微信端   2-易班端</param>
        private void LoginRecord(string userID, string userName, int roleID, string roleName, string grade, int loginPort)
        {
            try
            {
                T_LoginInfo loginInfo = new T_LoginInfo();
                loginInfo.userID = userID;
                loginInfo.userName = userName;
                loginInfo.roleID = roleID;
                loginInfo.roleName = roleName;
                loginInfo.grade = grade;
                loginInfo.loginPort = loginPort;
                loginInfo.loginTime = DateTime.Now;
                db.T_LoginInfo.Add(loginInfo);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        #endregion
    }
}
