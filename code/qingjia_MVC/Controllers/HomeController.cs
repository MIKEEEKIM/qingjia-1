using FineUIMvc;
using qingjia_MVC.Models;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml;

namespace qingjia_MVC.Controllers
{
    public class HomeController : BaseController
    {
        //实例化数据库
        private Entities db = new Entities();

        public ActionResult Index()
        {
            if (Request.QueryString["updatetime"] != null)
            {
                string updatetime = Request.QueryString["updatetime"].ToString();
                if (updatetime != "" && updatetime != null && updatetime == "true")
                {
                    ShowNotify("更新晚点名时间成功！");
                }
            }
            LoadData();
            return View();
        }

        protected void LoadData()
        {
            string UserID = Session["UserID"].ToString();
            string RoleID = Session["RoleID"].ToString();
            if (RoleID == "1")
            {
                //学生首页
                ViewBag.IFrameURL = "~/Home/Default_ST";
                var UserName = from T_Student in db.T_Student where (T_Student.ID == UserID) select T_Student.Name;
                ViewBag.UserName = "学生-" + UserName.ToList().First().ToString();
            }
            if (RoleID == "2")
            {
                //班级账号首页、尚未完成
                ViewBag.IFrameURL = "~/Vacation/Vacation/Index";

                var ClassName = from T_Class in db.T_Class where (T_Class.ID == UserID) select T_Class.ClassName;
                ViewBag.UserName = "班级-" + ClassName.ToList().First().ToString() + "班";
            }
            if (RoleID == "3")
            {
                //辅导员首页
                ViewBag.IFrameURL = "~/Home/Default_T";

                var UserName = from T_Teacher in db.T_Teacher where (T_Teacher.ID == UserID) select T_Teacher.Name;
                ViewBag.UserName = "辅导员-" + UserName.ToList().First().ToString();
            }

            //根据角色类型加载菜单栏
            LoadTreeMenuData(RoleID);
        }

        private void LoadTreeMenuData(string RoleID)
        {
            string xmlPath = "";
            if (RoleID == "1")
            {
                //学生端
                xmlPath = Server.MapPath("~/Content/XML/Student.xml");
            }
            if (RoleID == "2")
            {
                //暂未完成
                xmlPath = Server.MapPath("~/Content/XML/Class.xml");
            }
            if (RoleID == "3")
            {
                //辅导员端
                xmlPath = Server.MapPath("~/Content/XML/Teacher.xml");
            }

            //处理XML数据
            string xmlContent = String.Empty;
            using (StreamReader sr = new StreamReader(xmlPath))
            {
                xmlContent = sr.ReadToEnd();
            }

            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(xmlContent);

            ViewBag.TreeDataSource = xdoc;
        }

        public ActionResult Hello()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnHello_Click()
        {
            Alert.Show("你好 FineUI！", MessageBoxIcon.Warning);

            return UIHelper.Result();
        }

        public ActionResult Login()
        {
            return View();
        }

        // GET: Themes
        public ActionResult Themes()
        {
            return View();
        }

        public void setBatch()
        {
            string teacherid = "";

            try
            {
                if (Session["RoleID"].ToString() == "3")//辅导员
                {
                    teacherid = Session["UserID"].ToString();
                    T_Teacher teacher = db.T_Teacher.Find(teacherid);
                    string teacher_name = teacher.Name.ToString();
                    string teacher_grade = teacher.Grade.ToString() + "级";
                    ViewBag.lblTName = teacher_name.ToString();
                    ViewBag.lblTGrade = teacher_grade.ToString();
                }
                else if (Session["RoleID"].ToString() == "1")//学生
                {
                    string ST_Num = Session["UserID"].ToString();
                    var student = from vw_Student in db.vw_Student where vw_Student.ST_Num == ST_Num select vw_Student;
                    teacherid = student.ToList().First().ST_TeacherID.ToString();
                    T_Teacher teacher = db.T_Teacher.Find(teacherid);
                    string teacher_name = teacher.Name.ToString();
                    string teacher_grade = teacher.Grade.ToString() + "级";
                    ViewBag.lblTName = teacher_name.ToString();
                    ViewBag.lblTGrade = teacher_grade.ToString();
                }
            }
            catch
            {
                ViewBag.lblTName = "未设定";
                ViewBag.lblTGrade = "未设定";
            }

            if (teacherid != "")
            {
                ViewBag.IsTeacherIDExist = "1";
                try
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        var classes = from c in db.vw_ClassBatch
                                      where c.TeacherID == teacherid && c.Batch == i
                                      select c;
                        StringBuilder str_class = new StringBuilder();
                        string time;
                        if (classes.Count() > 0)
                        {

                            foreach (var item in classes)
                            {
                                str_class.Append(item.ClassName.ToString());
                                str_class.Append("；");
                            }
                            time = classes.First().Datetime.ToString();
                        }
                        else
                        {
                            str_class.Append("无");
                            time = "无";
                        }
                        switch (i)
                        {
                            case 1:
                                {
                                    ViewBag.lblTimeFirst = time;
                                    ViewBag.lblClassFirst = str_class.ToString();
                                    break;
                                }
                            case 2:
                                {
                                    ViewBag.lblTimeSecond = time;
                                    ViewBag.lblClassSecond = str_class.ToString();
                                    break;
                                }
                            case 3:
                                {
                                    ViewBag.lblTimeThird = time;
                                    ViewBag.lblClassThird = str_class.ToString();
                                    break;
                                }
                            default:
                                break;
                        }
                    }

                    string deadline_time = "辅导员尚未设定";

                    //此处TypeID = 2 代表晚点名请假截止时间、TypeID = 1 代表节假日请假截止时间
                    var deadline = from d in db.T_Deadline
                                   where d.TeacherID == teacherid && d.TypeID == 2
                                   select d.Time;
                    if (deadline.Count() > 0)
                    {
                        deadline_time = deadline.First().ToString();
                    }

                    ViewBag.lblDeadlineNight = deadline_time;

                }
                catch
                {
                    ViewBag.lblTimeFirst = "未设定晚点名批次";
                    ViewBag.lblClassFirst = "未设定晚点名时间";
                    ViewBag.lblTimeSecond = "未设定晚点名批次";
                    ViewBag.lblClassSecond = "未设定晚点名时间";
                    ViewBag.lblTimeThird = "未设定晚点名批次";
                    ViewBag.lblClassThird = "未设定晚点名时间";
                    ViewBag.lblDeadlineNight = "未设定截止时间";
                }
            }
            else
            {
                //学生端口   尚未绑定辅导员
                ViewBag.IsTeacherIDExist = "0";
            }
        }

        // GET: Default_ST
        public ActionResult Default_ST()
        {
            setBatch();
            return View();
        }

        // GET: Default_T
        public ActionResult Default_T()
        {
            setBatch();
            return View();
        }
    }
}