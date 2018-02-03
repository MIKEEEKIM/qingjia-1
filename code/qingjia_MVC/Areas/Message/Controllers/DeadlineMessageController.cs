using FineUIMvc;
using qingjia_MVC.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace qingjia_MVC.Areas.Message.Controllers
{
    public class DeadlineMessageController : Controller
    {
        private Entities db = new Entities();
        
        private int type_night = 2;     //特殊请假
        private int type_vacation = 1;  //离校请教

        // GET: Message/DeadlineMessage
        public ActionResult Index()
        {
            LoadData();
            return View();
        }

        //Deadline 加载数据
        protected void LoadData()
        {
            string teacherid = Session["UserID"].ToString();
            DateTime date;
            string time;
            #region 晚点名时间
            var night = from d in db.T_Deadline
                        where d.TypeID == type_night && d.TeacherID == teacherid
                        select d.Time;
            ViewBag.DateNight = "";
            ViewBag.TimeNight = "";
            if (night.Count() > 0)
            {
                date = night.First();
                time = date.ToString("yyyy-MM-dd HH:mm").Substring(11, 5);
                ViewBag.DateNight = date;
                ViewBag.TimeNight = time;
            }
            #endregion

            #region 节假日时间
            var vacation = from d in db.T_Deadline
                           where d.TypeID == type_vacation && d.TeacherID == teacherid
                           select d.Time;
            ViewBag.DateVacation = "";
            ViewBag.TimeVacation = "";
            if (vacation.Count() > 0)
            {
                date = vacation.First();
                time = date.ToString("yyyy-MM-dd HH:mm").Substring(11, 5);
                ViewBag.DateVacation = date;
                ViewBag.TimeVacation = time;
            }
            #endregion
        }

        #region 晚点名请假截止时间
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnNight_Click(FormCollection form)
        {
            string teacherid = Session["UserID"].ToString();
            T_Deadline t_deadline;
            string date = form["dpNight"];
            string time = form["tpNight"];
            string datetime = date + " " + time + ":00";
            DateTime d_time = Convert.ToDateTime(datetime);

            var night = from d in db.T_Deadline
                        where d.TypeID == type_night && d.TeacherID == teacherid
                        select d;

            if (night.Count() > 0)
            {
                t_deadline = night.First();
                t_deadline.Time = d_time;
                db.Entry(t_deadline).State= EntityState.Modified;
            }
            else
            {
                t_deadline = new T_Deadline();
                t_deadline.Time = d_time;
                t_deadline.TypeID = type_night;
                t_deadline.TeacherID = teacherid;

                db.T_Deadline.Add(t_deadline);
            }
            db.SaveChanges();

            Alert alert = new Alert();
            alert.Message = "截止时间设置成功！";
            alert.EnableClose = false;
            alert.Show();

            return UIHelper.Result();
        }
        #endregion

        #region 设置节假日请假截止时间
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnVacation_Click(FormCollection form)
        {
            string teacherid = Session["UserID"].ToString();
            T_Deadline t_deadline;
            string date = form["dpVacation"];
            string time = form["tpVacation"];
            string datetime = date + " " + time + ":00";
            DateTime d_time = Convert.ToDateTime(datetime);

            var vacation = from d in db.T_Deadline
                        where d.TypeID == type_vacation && d.TeacherID == teacherid
                        select d;

            if (vacation.Count() > 0)
            {
                t_deadline = vacation.First();
                t_deadline.Time = d_time;
                db.Entry(t_deadline).State = EntityState.Modified;
            }
            else
            {
                t_deadline = new T_Deadline();
                t_deadline.Time = d_time;
                t_deadline.TypeID = type_vacation;
                t_deadline.TeacherID = teacherid;

                db.T_Deadline.Add(t_deadline);
            }
            db.SaveChanges();

            Alert alert = new Alert();
            alert.Message = "截止时间设置成功！";
            alert.EnableClose = false;
            alert.Show();

            return UIHelper.Result();
        }
        #endregion
    }
}