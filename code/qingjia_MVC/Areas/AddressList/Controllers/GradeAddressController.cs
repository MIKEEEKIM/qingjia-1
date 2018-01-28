using FineUIMvc;
using Newtonsoft.Json.Linq;
using qingjia_MVC.Common;
using qingjia_MVC.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace qingjia_MVC.Areas.AddressList.Controllers
{
    public class GradeAddressController : Controller
    {
        private imaw_qingjiaEntities db = new imaw_qingjiaEntities();

        // GET: AddressList/GradeAddress
        public ActionResult Index()
        {
            string teacherid = Session["UserID"].ToString();
            List<PublicMethod.ClassList> classes = PublicMethod.getClassList(teacherid);
            classes.Insert(0, new PublicMethod.ClassList("0", "全部班级"));
            ViewBag.ddlSearchClass_DataSource = classes;
            List<T_Student> stu = (from s in db.T_Student
                                   join c in db.T_Class
                                   on s.ClassName equals c.ClassName
                                   where c.TeacherID == teacherid
                                   orderby s.ClassName
                                   select s)
                                .ToList();
            return View(stu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ttbSearchName_Trigger2Click(string name, JArray fields)
        {
            string teacherid = Session["UserID"].ToString();
            var stuList = from s in db.T_Student
                          join c in db.T_Class
                          on s.ClassName equals c.ClassName
                          where c.TeacherID == teacherid && s.Name.StartsWith(name)
                          select s;
            //var stuList = db.T_Student.Where(stu => stu.Name.Contains(name));
            UIHelper.Grid("gridClassAddress").DataSource(stuList.ToList(), fields);

            UIHelper.TwinTriggerBox("ttbSearchName").Text("");
            UIHelper.TwinTriggerBox("ttbSearchID").Text("");

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ttbSearchID_Trigger2Click(string id, JArray fields)
        {
            string teacherid = Session["UserID"].ToString();
            var stuList = from s in db.T_Student
                          join c in db.T_Class
                          on s.ClassName equals c.ClassName
                          where c.TeacherID == teacherid && s.ID.StartsWith(id)
                          select s;
            //var stuList = db.T_Student.Where(stu => stu.ID.Contains(id));
            UIHelper.Grid("gridClassAddress").DataSource(stuList.ToList(), fields);

            UIHelper.TwinTriggerBox("ttbSearchName").Text("");
            UIHelper.TwinTriggerBox("ttbSearchID").Text("");

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ddlSearchClass_SelectedIndexChanged(string ddlSearchClass, string ddlSearchClass_text, JArray fields)
        {
            string teacherid = Session["UserID"].ToString();
            var stuList = from s in db.T_Student
                          join c in db.T_Class
                          on s.ClassName equals c.ClassName
                          where c.TeacherID == teacherid
                          orderby s.ClassName
                          select s;
            if (ddlSearchClass != "0")
            {
                stuList = stuList.Where(s => s.ClassName == ddlSearchClass_text);
            }
            UIHelper.Grid("gridClassAddress").DataSource(stuList.ToList(), fields);
            
            

            UIHelper.TwinTriggerBox("ttbSearchName").Text("");
            UIHelper.TwinTriggerBox("ttbSearchID").Text("");

            return UIHelper.Result();
        }
    }
}