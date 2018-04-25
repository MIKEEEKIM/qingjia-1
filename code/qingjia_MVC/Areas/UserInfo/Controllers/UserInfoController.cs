using qingjia_MVC.Controllers;
using qingjia_MVC.Models;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;


namespace qingjia_MVC.Areas.UserInfo.Controllers
{
    public class UserInfoController : BaseController
    {
        private imaw_qingjiaEntities db = new imaw_qingjiaEntities();

        // GET: UserInfo/UserInfo
        public ActionResult Index()
        {
            LoadData(Session["UserID"].ToString());
            return View();
        }

        protected void LoadData(string ST_NUM)
        {
            string ST_Num = Session["UserID"].ToString();
            vw_Student modelStudent = new vw_Student();
            try
            {
                var vw_stu = from vw_Student in db.vw_Student where (vw_Student.ST_Num == ST_Num) select vw_Student;
                modelStudent = vw_stu.ToList().First();
            }
            catch (DbEntityValidationException dbEx)
            {

            }

            //var vw_stu = from vw_Student in db.vw_Student where vw_Student.ST_Num == ST_NUM select vw_Student;
            //vw_Student modelStudent = vw_stu.ToList().First();

            //先判断是否存在，再进行复制，防止报错
            ViewBag.ST_NUM = (modelStudent.ST_Num == null) ? "" : modelStudent.ST_Num.ToString();
            ViewBag.ST_Name = (modelStudent.ST_Name == null) ? "" : modelStudent.ST_Name.ToString();
            ViewBag.ST_SEX = (modelStudent.ST_Sex == null) ? "" : modelStudent.ST_Sex.ToString();
            ViewBag.ST_CLASS = (modelStudent.ST_Class == null) ? "" : modelStudent.ST_Class.ToString();
            ViewBag.Teacher = (modelStudent.ST_Teacher == null) ? "" : modelStudent.ST_Teacher.ToString();
            ViewBag.ST_Tel = (modelStudent.ST_Tel == null) ? "" : modelStudent.ST_Tel.ToString();
            ViewBag.ST_QQ = (modelStudent.ST_QQ == null) ? "" : modelStudent.ST_QQ.ToString();
            ViewBag.ST_Email = (modelStudent.ST_Email == null) ? "" : modelStudent.ST_Email.ToString();
            ViewBag.ST_Door = (modelStudent.ST_Dor == null) ? "" : modelStudent.ST_Dor.ToString();

            if (modelStudent.ContactOne.ToString() == "")
            {
                ViewBag.Relation = "";
                ViewBag.RelationName = "";
            }
            else
            {
                try
                {
                    ViewBag.Relation = modelStudent.ContactOne.ToString().Substring(0, 2);
                    ViewBag.RelationName = modelStudent.ContactOne.ToString().Substring(3, modelStudent.ContactOne.ToString().Length - 3);
                }
                catch
                {
                    //ViewBag.Relation = modelStudent.ContactOne.ToString().Substring(0, 2);
                    //ViewBag.RelationName = modelStudent.ContactOne.ToString().Substring(3, modelStudent.ContactOne.ToString().Length - 3);
                }
                
            }
            ViewBag.RelaTIonTel = modelStudent.OneTel.ToString();
            ViewBag.RelaTIonTel = ((modelStudent.OneTel == "") || (modelStudent.OneTel == null)) ? "" : modelStudent.OneTel.ToString();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUserInfo(string ST_Tel, string ST_QQ, string ST_Email, string ST_ContactName, string ST_ContactRelation, string ST_ContactTel)
        {
            string ST_Num = Session["UserID"].ToString();
            T_Student student = db.T_Student.Find(ST_Num);
            if (student.ContactOne == "" || student.ContactOne == null || student.OneTel == "" || student.OneTel == null)
            {
                #region 登陆之后自动跳转，完善个人信息，联系人方式等信息为空
                //缺少验证处理

                if (ST_Tel == "" && ST_QQ == "" && ST_Email == "" && ST_ContactName == "" && ST_ContactRelation == "" && ST_ContactTel == "")
                {
                    LoadData(ST_Num);
                    ShowNotify("未做任何修改！");
                    return View("Index");
                }
                if (ST_Tel != "")
                {
                    student.Tel = ST_Tel.ToString();
                }
                if (ST_QQ != "")
                {
                    student.QQ = ST_QQ.ToString();
                }
                if (ST_Email != "")
                {
                    student.Email = ST_Email.ToString();
                }
                if (ST_ContactName != "")
                {
                    //此处需要判断原数据是否为null
                    if (student.ContactOne != null && student.ContactOne.ToString() != "")
                    {
                        student.ContactOne = student.ContactOne.ToString().Substring(0, 3) + ST_ContactName.ToString();
                    }
                    else
                    {
                        student.ContactOne = ST_ContactName.ToString();
                    }
                }
                if (ST_ContactRelation != "")
                {
                    if (ST_ContactRelation != "父亲" && ST_ContactRelation != "母亲" && ST_ContactRelation != "其他")
                    {
                        LoadData(ST_Num);
                        ShowNotify("联系人关系类型为父亲、母亲或其他，请重新填写！");
                        return View("Index");
                    }
                    else
                    {
                        //检查是否包含“-”字符
                        if (student.ContactOne.Contains("-"))
                        {
                            //联系人信息已存在
                            student.ContactOne = ST_ContactRelation.ToString() + "-" + student.ContactOne.ToString().Substring(3, student.ContactOne.ToString().Length - 3);
                        }
                        else
                        {
                            //联系人信息不存在
                            student.ContactOne = ST_ContactRelation.ToString() + "-" + student.ContactOne.ToString();
                        }
                    }
                }
                if (ST_ContactTel != "")
                {
                    student.OneTel = ST_ContactTel.ToString();
                }
                db.SaveChanges();
                LoadData(ST_Num);
                ViewBag.Changed = true;
                ShowNotify("修改成功");
                //return View("Index");
                return RedirectToAction("Index", "Home", new { area = "" });
                #endregion
            }
            else
            {
                #region 修改个人信息，联系人方式等信息为空
                //缺少验证处理

                if (ST_Tel == "" && ST_QQ == "" && ST_Email == "" && ST_ContactName == "" && ST_ContactRelation == "" && ST_ContactTel == "")
                {
                    LoadData(ST_Num);
                    ShowNotify("未做任何修改！");
                    return View("Index");
                }
                if (ST_Tel != "")
                {
                    student.Tel = ST_Tel.ToString();
                }
                if (ST_QQ != "")
                {
                    student.QQ = ST_QQ.ToString();
                }
                if (ST_Email != "")
                {
                    student.Email = ST_Email.ToString();
                }
                if (ST_ContactName != "")
                {
                    //此处需要判断原数据是否为null
                    if (student.ContactOne != null && student.ContactOne.ToString() != "")
                    {
                        student.ContactOne = student.ContactOne.ToString().Substring(0, 3) + ST_ContactName.ToString();
                    }
                    else
                    {
                        student.ContactOne = ST_ContactName.ToString();
                    }
                }
                if (ST_ContactRelation != "")
                {
                    if (ST_ContactRelation != "父亲" && ST_ContactRelation != "母亲" && ST_ContactRelation != "其他")
                    {
                        LoadData(ST_Num);
                        ShowNotify("联系人关系类型为父亲、母亲或其他，请重新填写！");
                        return View("Index");
                    }
                    else
                    {
                        //检查是否包含“-”字符
                        if (student.ContactOne.Contains("-"))
                        {
                            //联系人信息已存在
                            student.ContactOne = ST_ContactRelation.ToString() + "-" + student.ContactOne.ToString().Substring(3, student.ContactOne.ToString().Length - 3);
                        }
                        else
                        {
                            //联系人信息不存在
                            student.ContactOne = ST_ContactRelation.ToString() + "-" + student.ContactOne.ToString();
                        }
                    }
                }
                if (ST_ContactTel != "")
                {
                    student.OneTel = ST_ContactTel.ToString();
                }
                db.SaveChanges();
                LoadData(ST_Num);
                ViewBag.Changed = true;
                ShowNotify("修改成功");
                return View("Index");
                #endregion
            }
        }
    }
}