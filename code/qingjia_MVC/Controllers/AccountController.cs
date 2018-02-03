using FineUIMvc;
using qingjia_MVC.Models;
using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace qingjia_MVC.Controllers
{
    public class AccountController : Controller
    {
        //实例化数据库
        private Entities db = new Entities();

        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult login(string txbAccount, string txbAccountPass)
        {
            var user = from T_Account in db.T_Account where (T_Account.ID == txbAccount.ToString().Trim()) && (T_Account.Psd == txbAccountPass.ToString().Trim()) select T_Account;

            if (user.Any() && user.Count() == 1)
            {

                #region 登录成功、将用户信息写入Session
                //从数据集中提取
                Session["UserID"] = user.First().ID;
                Session["UserPsd"] = user.First().Psd;
                Session["RoleID"] = user.First().RoleID;

                //从表单中提取
                //Session["UserID"] = tbxUserName.ToString().Trim();
                //Session["UserPsd"] = tbxPassword.ToString().Trim();
                #endregion

                #region 检查学生是否完善个人信息、辅导员的信息是否更新
                if (Session["RoleID"].ToString() == "1")//学生
                {
                    T_Student student = db.T_Student.Find(Session["UserID"].ToString());
                    if (student.ContactOne == "" || student.ContactOne == null || student.OneTel == "" || student.OneTel == null)//信息不完善
                    {
                        //FineUI登陆成功提示框、完善个人信息
                        ShowNotify("成功登陆！请完善个人信息", MessageBoxIcon.Success);
                        return RedirectToAction("Index", "UserInfo", new { area = "UserInfo" });
                        //此处需要以Areas的ID作为参数才能实现从Controller到Areas中的Controller的跳转
                    }
                    else//信息已完善
                    {
                        //FineUI登陆成功提示框
                        ShowNotify("成功登录！", MessageBoxIcon.Success);
                        return RedirectToAction("Index", "Home");
                    }
                }
                else if (Session["RoleID"].ToString() == "3")//辅导员
                {
                    string UserID = Session["UserID"].ToString();
                    var TeacherInfo = from T_Teacher in db.T_Teacher where (T_Teacher.ID == UserID) select T_Teacher;
                    T_Teacher teacher = TeacherInfo.ToList().First();
                    Session["Grade"] = teacher.Grade;

                    //辅导员登录-更新晚点名时间
                    if (UpdateTime(Session["UserID"].ToString()))
                    {
                        //更新了时间
                        return RedirectToAction("Index", "Home", new { updatetime = "true" });
                    }
                    else
                    {
                        //未更新时间
                        return RedirectToAction("Index", "Home");
                    }
                }
                else if (Session["RoleID"].ToString() == "2")
                {
                    //FineUI登陆成功提示框
                    ShowNotify("成功登录！", MessageBoxIcon.Success);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    //未知错误
                    return null;
                }
                #endregion
            }
            else if (!user.Any())
            {
                //FineUI登录失败提示框
                //ShowNotify("用户名或密码错误！", MessageBoxIcon.Error);
                alertInfo("登录提示", "用户名或密码错误！", "Information");
                return RedirectToAction("Index", "Account");
            }
            else
            {
                alertInfo("登录提示", "用户名或密码错误！", "Information");
                return RedirectToAction("Index", "Account");
            }
        }

        protected bool UpdateTime(string TeacherID)
        {
            bool flag = false;

            var timelist = from T_Batch in db.T_Batch where (T_Batch.TeacherID == TeacherID) select T_Batch;
            if (timelist.Any())
            {
                foreach (T_Batch batch in timelist.ToList())
                {
                    DateTime _lastTime = batch.Datetime;
                    if (DateTime.Now > _lastTime)
                    {
                        //晚点名已过期
                        T_Batch newBatch = db.T_Batch.Find(batch.ID);
                        string time = _lastTime.ToString("yyyy-MM-dd HH:mm:ss").Substring(10, 9);

                        int weeknow = Convert.ToInt32(DateTime.Now.DayOfWeek);
                        weeknow = (weeknow == 0 ? 7 : weeknow);
                        int daydiff = (7 - weeknow);
                        string LastDay = "";
                        if (daydiff == 0)
                        {
                            LastDay = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            LastDay = DateTime.Now.AddDays(daydiff).ToString("yyyy-MM-dd");
                        }
                        string newTime = LastDay + time;

                        newBatch.Datetime = Convert.ToDateTime(newTime);
                        db.SaveChanges();
                        flag = true;
                    }
                }
                //db.SaveChanges();
                ShowNotify("已完成晚点名时间更新！");
            }
            else
            {
                //无数据
                ShowNotify("无晚点名数据！");
            }
            return flag;
        }

        /// <summary>
        /// Alert.MessageBoxIcon可设置提示框图标样式,可选样式：None无 Information消息 Warning警告 Question问题 Error错误 Success成功,Alert.Target可设置显示提示框的位置,可选样式：Self当前页面 Parent父页面 Top顶层页面
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="message">信息</param>
        /// <param name="icon">Icon类型</param>
        public void alertInfo(string title, string message, string icon)
        {
            Alert alert = new Alert();
            alert.Title = title;
            alert.Message = message;
            alert.MessageBoxIcon = (MessageBoxIcon)Enum.Parse(typeof(MessageBoxIcon), icon, true);
            alert.Target = (Target)Enum.Parse(typeof(Target), "Self", true);
            alert.Show();
        }

        #region 实用函数

        /// <summary>
        /// 显示通知对话框
        /// </summary>
        /// <param name="fields"></param>
        public virtual void ShowNotify(FormCollection values)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("表单字段值：");
            sb.Append("<ul class=\"result\">");
            foreach (string key in values.Keys)
            {
                sb.AppendFormat("<li>{0}: {1}</li>", key, values[key]);
            }
            sb.Append("</ul>");

            ShowNotify(sb.ToString());
        }

        /// <summary>
        /// 显示通知对话框
        /// </summary>
        /// <param name="message"></param>
        public virtual void ShowNotify(string message)
        {
            ShowNotify(message, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 显示通知对话框
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageIcon"></param>
        public virtual void ShowNotify(string message, MessageBoxIcon messageIcon)
        {
            ShowNotify(message, messageIcon, Target.Top);
        }

        /// <summary>
        /// 显示通知对话框
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageIcon"></param>
        /// <param name="target"></param>
        public virtual void ShowNotify(string message, MessageBoxIcon messageIcon, Target target)
        {
            Notify n = new Notify();
            n.Target = target;
            n.Message = message;
            n.MessageBoxIcon = messageIcon;
            n.PositionX = Position.Center;
            n.PositionY = Position.Top;
            n.DisplayMilliseconds = 3000;
            n.ShowHeader = false;

            n.Show();
        }

        //暂时未使用到
        /// <summary>
        /// 获取网址的完整路径
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        public string GetAbsoluteUrl(string virtualPath)
        {
            // http://benjii.me/2015/05/get-the-absolute-uri-from-asp-net-mvc-content-or-action/
            var urlBuilder = new System.UriBuilder(Request.Url.AbsoluteUri)
            {
                Path = Url.Content(virtualPath),
                Query = null,
            };

            return urlBuilder.ToString();
        }


        #endregion
    }
}