using FineUIMvc;
using Newtonsoft.Json.Linq;
using qingjia_MVC.Common;
using qingjia_MVC.Models;
using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;

namespace qingjia_MVC.Areas.Message.Controllers
{
    public class NightMessageController : Controller
    {
        private Entities db = new Entities();

        // GET: Message/NightMessage
        public ActionResult Index()
        {
            string teacherid = Session["UserID"].ToString();
            ViewBag.cblClasses = PublicMethod.getClassList(teacherid);
            LoadData();
            
            return View();
        }
        /// <summary>
        /// 初始化晚点名设置
        /// </summary>
        public void LoadData()
        {
            string teacherid = Session["UserID"].ToString();
            int batch_num = 0;
            int i = 0;
            string[][] selected = new string[3][];
            ViewBag.selected_first = "";
            ViewBag.selected_second = "";
            ViewBag.selected_thrid = "";
            ViewBag.tpFirst = "";
            ViewBag.tpSecond = "";
            ViewBag.tpThird = "";
            ViewBag.dpDate = "";

            T_Teacher teacher = db.T_Teacher.Find(teacherid);
            string teacher_name = teacher.Name.ToString();
            string teacher_grade = teacher.Grade.ToString() + "级";
            ViewBag.lblNameAGrade = "辅导员：" + teacher_name + "      " + "年级：" + teacher_grade;
            
            var batch = from b in db.T_Batch
                    where b.TeacherID == teacherid
                    orderby b.Batch
                    select new
                    {
                        b.ID,
                        b.Datetime
                    };
            if (batch.Count() > 0)
            {
                Guid batch_id;
                string time;
                ViewBag.dpDate = batch.First().Datetime;
                foreach (var item in batch)
                {
                    i = 0;
                    batch_id = item.ID;
                    time = item.Datetime.ToString("yyyy-MM-dd HH:mm").Substring(11, 5);
                    var list = db.T_Class.Where(c => c.Batch == batch_id).Where(b => b.TeacherID == teacherid).Select(b => b.ID);
                    selected[batch_num] = new string[list.Count()];
                    foreach (var classes in list)
                    {
                        selected[batch_num][i] = classes.ToString();
                        i++;
                    }
                    switch (batch_num)
                    {
                        case 0:
                            {
                                ViewBag.selected_first = selected[batch_num];
                                ViewBag.tpFirst = time;
                                break;
                            }
                        case 1:
                            {
                                ViewBag.selected_second = selected[batch_num];
                                ViewBag.tpSecond = time;
                                break;
                            }
                        case 2:
                            {
                                ViewBag.selected_thrid = selected[batch_num];
                                ViewBag.tpThird = time;
                                break;
                            }
                        default:
                            break;
                    }
                    batch_num++;
                    

                }

            }
        }

        /// <summary>
        /// 提交晚点名通知
        /// </summary>
        /// <param name="form">表单</param>
        /// <param name="first">第一批次班级</param>
        /// <param name="second">第二批次班级</param>
        /// <param name="third">第三批次班级列表</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnSubmit_Click(FormCollection form, JArray first, JArray second, JArray third)
        {
            string teacherid = Session["UserID"].ToString();
            string date = form["dpDate"];
            string time_one = form["tpFirst"];
            string time_two = form["tpSecond"];
            string time_three = form["tpThird"];

            DateTime dt_one = Convert.ToDateTime(date + " " + time_one + ":00");
            DateTime dt_two = Convert.ToDateTime(date + " " + time_two + ":00");
            DateTime dt_three = Convert.ToDateTime(date + " " + time_three + ":00");

            T_Batch batch;
            T_Class t_class;
            Guid id;

            #region 清除该辅导员管理的班级的批次信息
            var classes = from c in db.T_Class
                          where c.TeacherID == teacherid
                          select c;
            foreach (var item in classes)
            {
                item.Batch = null;
                db.Entry(item).State = EntityState.Modified;
            }
            db.SaveChanges();
            #endregion

            #region 第一批
            if ((time_one != null) && (first.Count > 0))
            {
                batch = new T_Batch();
                
                var batch_id = from b in db.T_Batch
                               where b.Batch == 1 && b.TeacherID == teacherid
                               select b.ID;

                //辅导员是否设置过晚点名通知
                if (batch_id.Count() == 0)
                {
                    batch.ID = Guid.NewGuid();
                    batch.Batch = 1;
                    batch.TeacherID = teacherid;
                    batch.Datetime = dt_one;
                    db.T_Batch.Add(batch);
                }
                else
                {
                    id = batch_id.ToList().First();
                    batch = db.T_Batch.Find(id);
                    batch.Datetime = dt_one;
                    db.Entry(batch).State = EntityState.Modified;
                }
                db.SaveChanges();

                batch_id = from b in db.T_Batch
                           where b.Batch == 1 && b.TeacherID == teacherid
                           select b.ID;
                id = batch_id.ToList().First();
                foreach (JObject item in first)
                {
                    t_class = db.T_Class.Find(item["ID"].ToString());
                    t_class.Batch = id;
                    db.Entry(t_class).State = EntityState.Modified;
                }
                db.SaveChanges();
            }
            #endregion

            #region 第二批
            if ((time_two != null) && (second.Count > 0))
            {
                batch = new T_Batch();
                var batch_id = from b in db.T_Batch
                               where b.Batch == 2 && b.TeacherID == teacherid
                               select b.ID;

                //辅导员是否设置过晚点名通知
                if (batch_id.Count() == 0)
                {
                    batch.ID = Guid.NewGuid();
                    batch.Batch = 2;
                    batch.TeacherID = teacherid;
                    batch.Datetime = dt_two;
                    db.T_Batch.Add(batch);
                }
                else
                {
                    id = batch_id.ToList().First();
                    batch = db.T_Batch.Find(id);
                    batch.Datetime = dt_two;
                    db.Entry(batch).State = EntityState.Modified;
                }
                try
                {
                    db.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {

                }
                

                batch_id = from b in db.T_Batch
                           where b.Batch == 2 && b.TeacherID == teacherid
                           select b.ID;
                id = batch_id.ToList().First();
                foreach (JObject item in second)
                {
                    t_class = db.T_Class.Find(item["ID"].ToString());
                    t_class.Batch = id;
                    db.Entry(t_class).State = EntityState.Modified;
                }
                db.SaveChanges();
            }
            #endregion

            #region 第三批
            if ((time_three != null) && (third.Count > 0))
            {
                batch = new T_Batch();
                var batch_id = from b in db.T_Batch
                               where b.Batch == 3 && b.TeacherID == teacherid
                               select b.ID;

                //辅导员是否设置过晚点名通知
                if (batch_id.Count() == 0)
                {
                    batch.ID = Guid.NewGuid();
                    batch.Batch = 3;
                    batch.TeacherID = teacherid;
                    batch.Datetime = dt_three;
                    db.T_Batch.Add(batch);
                }
                else
                {
                    id = batch_id.ToList().First();
                    batch = db.T_Batch.Find(id);
                    batch.Datetime = dt_three;
                    db.Entry(batch).State = EntityState.Modified;
                }
                db.SaveChanges();

                batch_id = from b in db.T_Batch
                           where b.Batch == 3 && b.TeacherID == teacherid
                           select b.ID;
                id = batch_id.ToList().First();
                foreach (JObject item in third)
                {
                    t_class = db.T_Class.Find(item["ID"].ToString());
                    t_class.Batch = id;
                    db.Entry(t_class).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            #endregion

            Alert alert = new Alert();
            alert.Message = "晚点名通知设置成功";
            alert.EnableClose = false;
            alert.Show();
            return UIHelper.Result();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult cbl_SelectedIndexChanged(JArray one, JArray two, JArray three,string index)
        {
            Alert alert = new Alert();
            string[] one_new = new string[one.Count];
            int i = 0;
            foreach(JObject item in one)
            {
                one_new[i] = item["ID"].ToString();
                i++;
            }
            foreach (JObject item in two)
            {
                foreach (JObject selected in one)
                {
                    if (item["ID"].ToString().Equals(selected["ID"].ToString()))
                    {
                        one_new[one.IndexOf(selected)] = "";
                        
                        switch (index)
                        {
                            case "1":
                                {
                                    ViewBag.selected_first = one_new;
                                    break;
                                }
                            case "2":
                                {
                                    ViewBag.selected_second = one_new;
                                    break;
                                }
                            case "3":
                                {
                                    ViewBag.selected_thrid = one_new;
                                    break;
                                }
                            default:
                                break;
                        }

                        alert.Message = "该班级已被选，请选其它班级！";
                        alert.Show();
                        return UIHelper.Result();
                    }
                }
            }
            foreach (JObject item in three)
            {
                foreach (JObject selected in one)
                {
                    if (item["ID"] == selected["ID"])
                    {
                        one_new[one.IndexOf(selected)] = "";
                        switch (index)
                        {
                            case "1":
                                {
                                    ViewBag.selected_first = one_new;
                                    break;
                                }
                            case "2":
                                {
                                    ViewBag.selected_second = one_new;
                                    break;
                                }
                            case "3":
                                {
                                    ViewBag.selected_thrid = one_new;
                                    break;
                                }
                            default:
                                break;
                        }

                        alert.Message = "该班级已被选，请选其它班级！";
                        alert.Show();
                        return UIHelper.Result();
                    }
                }
            }
            return UIHelper.Result();
        }
    }
}