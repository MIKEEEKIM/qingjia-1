using FineUIMvc;
using Newtonsoft.Json.Linq;
using qingjia_MVC.Content;
using qingjia_MVC.Controllers;
using qingjia_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace qingjia_MVC.Areas.Leave.Controllers
{
    public class LeaveListController : BaseController
    {
        //实例化数据库
        private imaw_qingjiaEntities db = new imaw_qingjiaEntities();
        private string staticLeaveType = "total";

        #region Index
        // GET: Leave/LeaveList
        public ActionResult Index()
        {
            LoadData();
            return View();
        }

        //Index 加载数据
        protected void LoadData()
        {
            //防止Session过期
            try
            {
                string ST_Num = Session["UserID"].ToString();
                var studentInfo = from vw_Student in db.vw_Student where (vw_Student.ST_Num == ST_Num) select vw_Student;
                if (studentInfo.Any())
                {
                    vw_Student studentModel = (vw_Student)studentInfo.First();
                    if (studentModel.ST_Grade == null || studentModel.ST_Grade == "" || studentModel.ST_TeacherID == null || studentModel.ST_TeacherID == "")
                    {
                        //信息不完整  该学生尚未绑定班级 及 辅导员
                    }
                    ViewData["studentInfo"] = (vw_Student)studentInfo.First();
                }
                else
                {

                }

                var leaveList = from vw_LeaveList in db.vw_LeaveList where (vw_LeaveList.StudentID == ST_Num) orderby vw_LeaveList.ID descending select vw_LeaveList;
                if (leaveList.Count() == 0)
                {
                    ViewBag.LeaveListExist = false;
                    ViewData["allCount"] = 0;
                    ViewData["waitBackCount"] = 0;
                }
                else
                {
                    //取出前五条记录展示
                    ViewBag.LeaveListExist = true;
                    foreach (var ll in leaveList.Take(5).ToList())
                    {
                        ll.StateLeave = getKey("leavetype", ll.StateLeave + ll.StateBack);
                    }
                    ViewData["leaveList"] = leaveList.Take(5).ToList();
                    //计算销假情况及请假次数
                    int wait_back_count = 0;
                    foreach (vw_LeaveList li in leaveList)
                    {
                        if (li.StateLeave == "1" && li.StateBack == "0")
                        {
                            wait_back_count++;
                        }
                    }
                    ViewData["allCount"] = leaveList.Count();
                    ViewData["waitBackCount"] = wait_back_count;
                }
            }
            catch
            {

            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult changeInfo()
        {
            ActiveWindow.HidePostBack();
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult window_leave_Close()
        {
            //弹出个人信息修改框
            UIHelper.Window("changeInfo").Hidden(false);
            return UIHelper.Result();
        }
        #endregion

        #region 离校请假

        //GET: Leave/LeaveList/leaveschool
        public ActionResult leaveschool()
        {
            leaveschool_LoadData();
            return View();
        }

        //leaveschool 加载数据
        protected void leaveschool_LoadData()
        {
            string ST_Num = Session["UserID"].ToString();
            var ST_Info = from vw_Student in db.vw_Student where (vw_Student.ST_Num == ST_Num) select vw_Student;
            if (ST_Info.Any())
            {
                ViewData["LL_ST_Info"] = ST_Info.ToList().First();
            }

            var type = from T_Type in db.T_Type where (T_Type.FatherID == 1) select T_Type.Name;
            ViewBag.LeaveType = type.ToList();

            //ViewBag.holidayType
            List<string> holidayType = new List<string>();
            holidayType.Add("回家");
            holidayType.Add("旅游");
            holidayType.Add("因公外出");
            holidayType.Add("其他");
            ViewBag.ddl_holidayType = holidayType;
        }

        //重置表单
        protected void FormReset()
        {
            UIHelper.DatePicker("leaveDate").Reset();
            UIHelper.TimePicker("leaveTime").Reset();
            UIHelper.DatePicker("backDate").Reset();
            UIHelper.TimePicker("backTime").Reset();
            UIHelper.TextBox("leaveWay").Reset();
            UIHelper.TextBox("backWay").Reset();
            UIHelper.TextBox("address").Reset();
            UIHelper.DropDownList("holidayType").Reset();
            UIHelper.TextArea("leaveReason").Reset();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LL_TypeOnSelectedIndexChanged_LeaveSchool(string LL_Type)
        {
            FormReset();
            string ST_NUM = Session["UserID"].ToString();

            var studentlist = from vw_Student in db.vw_Student where (vw_Student.ST_Num == ST_NUM) select vw_Student.ST_TeacherID;
            var TeacherID = studentlist.First().ToString();

            var deadLine = from T_Deadline in db.T_Deadline where ((T_Deadline.TypeID == 1) && (T_Deadline.TeacherID == TeacherID)) select T_Deadline;
            string deadLineTime = "";

            if (deadLine.Any())
            {
                deadLineTime = deadLine.ToList().First().Time.ToString();
            }
            else
            {
                deadLineTime = "";
            }




            string text = LL_Type;
            if (LL_Type == "节假日请假")
            {
                if (deadLineTime != "")
                {
                    if (Convert.ToDateTime(deadLineTime) >= DateTime.Now)//截止时间大于当前时间
                    {
                        //节假日请假尚未截止
                        UIHelper.FormRow("holiday_formrow_1").Hidden(false);
                        UIHelper.FormRow("holiday_formrow_2").Hidden(false);
                        UIHelper.FormRow("holiday_formrow_3").Hidden(false);
                        UIHelper.TextArea("leaveReason").Hidden(true);

                    }
                    else
                    {
                        ShowNotify("节假日请假已经截至！");
                        UIHelper.DropDownList("LL_Type").Reset();
                    }
                }
                else
                {
                    ShowNotify("节假日请假截止时间尚未设置，请联系辅导员！");
                    UIHelper.DropDownList("LL_Type").Reset();
                }
            }
            else if (LL_Type == "短期请假" || LL_Type == "长期请假")
            {
                UIHelper.FormRow("holiday_formrow_1").Hidden(true);
                UIHelper.FormRow("holiday_formrow_2").Hidden(true);
                UIHelper.FormRow("holiday_formrow_3").Hidden(true);
                UIHelper.TextArea("leaveReason").Hidden(false);
            }
            else if (LL_Type == "早晚自习请假")
            {
                string ST_Num = Session["UserID"].ToString();
                var student = from vw_Student in db.vw_Student where vw_Student.ST_Num == ST_Num select vw_Student;
                vw_Student modelStudent = student.ToList().First();

                //获取具有早晚自习请假的年级
                string FreshmanYear = System.Configuration.ConfigurationManager.AppSettings["FreshmanYear"].ToString().Trim();

                if (modelStudent.ST_Grade.Trim() != FreshmanYear)
                {
                    ShowNotify("您没有早晚自习！");
                    UIHelper.DropDownList("LL_Type").Reset();
                }
            }
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LL_TypeOnSelectedIndexChanged_LeaveSpecial(string LL_Type)
        {
            //FormReset();
            string ST_NUM = Session["UserID"].ToString();
            string deadLineTime = "";

            var studentlist = from vw_Student in db.vw_Student where (vw_Student.ST_Num == ST_NUM) select vw_Student.ST_TeacherID;
            var TeacherID = studentlist.First().ToString();

            var deadLine = from T_Deadline in db.T_Deadline where ((T_Deadline.TypeID == 2) && (T_Deadline.TeacherID == TeacherID)) select T_Deadline;
            if (deadLine.Any())
            {
                deadLineTime = deadLine.ToList().First().Time.ToString();
            }
            else
            {
                ShowNotify("辅导员当前未设置特殊请假截止时间，请联系辅导员！");
                UIHelper.DropDownList("LL_Type").Reset();
                return UIHelper.Result();
            }

            string text = LL_Type;
            if (LL_Type == "节假日请假")
            {
                if (Convert.ToDateTime(deadLineTime) >= DateTime.Now)//截止时间大于当前时间
                {
                    //节假日请假尚未截止
                    UIHelper.FormRow("holiday_formrow_1").Hidden(false);
                    UIHelper.FormRow("holiday_formrow_2").Hidden(false);
                    UIHelper.FormRow("holiday_formrow_3").Hidden(false);
                    UIHelper.TextArea("leaveReason").Hidden(true);

                }
                else
                {
                    ShowNotify("节假日请假已经截至！");
                    UIHelper.DropDownList("LL_Type").Reset();
                }
            }
            else if (LL_Type == "短期请假" || LL_Type == "长期请假")
            {
                UIHelper.FormRow("holiday_formrow_1").Hidden(true);
                UIHelper.FormRow("holiday_formrow_2").Hidden(true);
                UIHelper.FormRow("holiday_formrow_3").Hidden(true);
                UIHelper.TextArea("leaveReason").Hidden(false);
            }
            else if (LL_Type == "早晚自习请假")
            {
                string ST_Num = Session["UserID"].ToString();
                var student = from vw_Student in db.vw_Student where vw_Student.ST_Num == ST_Num select vw_Student;
                vw_Student modelStudent = student.ToList().First();

                //获取具有早晚自习请假的年级
                string FreshmanYear = System.Configuration.ConfigurationManager.AppSettings["FreshmanYear"].ToString().Trim();

                if (modelStudent.ST_Grade.Trim() != FreshmanYear)
                {
                    ShowNotify("您没有早晚自习！");
                    UIHelper.DropDownList("LL_Type").Reset();
                }
            }
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult leaveSchool_btnSubmit_Click(FormCollection formInfo)
        {
            string ST_Num = Session["UserID"].ToString();
            string LL_Type = formInfo["LL_Type"];
            string leaveDate = formInfo["leaveDate"];
            string leaveTime = formInfo["leaveTime"];
            string backDate = formInfo["backDate"];
            string backTime = formInfo["backTime"];
            string leaveReason = formInfo["leaveReason"];
            string leaveWay = formInfo["leaveWay"];
            string backWay = formInfo["backWay"];
            string address = formInfo["address"];
            string holidayType = formInfo["holidayType"];

            //节假日请假时，请假原因为节假日请假的类型
            if (LL_Type == "节假日请假")
            {
                leaveReason = formInfo["holidayType"];
            }

            string LV_Time_Go = leaveDate + " " + leaveTime + ":00";
            string LV_Time_Back = backDate + " " + backTime + ":00";

            if (Convert.ToDateTime(LV_Time_Go) < Convert.ToDateTime(LV_Time_Back))
            {
                DateTime time_go = Convert.ToDateTime(LV_Time_Go);
                DateTime time_back = Convert.ToDateTime(LV_Time_Back);
                TimeSpan time_days = time_back - time_go;
                int days = time_days.Days;

                if (LL_Type == "短期请假")
                {
                    if (days < 3)//短期请假小于三天
                    {
                        //生成请假单号
                        string LV_NUM = DateTime.Now.ToString("yyMMdd");
                        var exist = from T_LeaveList in db.T_LeaveList where (T_LeaveList.StudentID == ST_Num) && (((T_LeaveList.TimeLeave >= time_go) && (T_LeaveList.TimeLeave <= time_back)) || ((T_LeaveList.TimeBack >= time_go) && (T_LeaveList.TimeBack <= time_back)) || ((T_LeaveList.TimeLeave <= time_go) && (T_LeaveList.TimeBack >= time_back))) select T_LeaveList;

                        if (exist.Any())
                        {
                            bool flag = true;
                            foreach (qingjia_MVC.Models.T_LeaveList leaveList in exist.ToList())
                            {
                                if (leaveList.StateBack == "0")
                                {
                                    flag = false;
                                    break;
                                }

                            }
                            if (flag)
                            {
                                //插入数据库操作
                                if (Insert_LeaveList(LV_NUM, ST_Num, LL_Type, time_go, time_back, leaveReason, leaveWay, backWay, address, holidayType, null, null, null) == 1)
                                {
                                    string script = String.Format("alert('请假申请成功！');");
                                    PageContext.RegisterStartupScript(ActiveWindow.GetHideRefreshReference() +
                                        script);
                                }
                                else
                                {
                                    alertInfo("提交失败", "数据库提交失败，请重新尝试!", "Information");
                                }
                            }
                            else
                            {
                                alertInfo("错误提示", "您已提交过此时间段的请假申请，请不要重复提交！", "Information");
                            }
                        }
                        else
                        {
                            //插入数据库操作
                            if (Insert_LeaveList(LV_NUM, ST_Num, LL_Type, time_go, time_back, leaveReason, leaveWay, backWay, address, holidayType, null, null, null) == 1)
                            {
                                string script = String.Format("alert('请假申请成功');");
                                PageContext.RegisterStartupScript(ActiveWindow.GetHideRefreshReference() +
                                    script);
                            }
                            else
                            {
                                alertInfo("提交失败", "数据库提交失败，请重新尝试!", "Information");
                            }
                        }
                    }
                    else
                    {
                        alertInfo("错误提示", "短期请假不能超过3天!", "Error");
                    }
                }
                else if (LL_Type == "长期请假")
                {
                    if (days >= 3)//长期请假
                    {
                        //生成请假单号
                        string LV_NUM = DateTime.Now.ToString("yyMMdd");
                        var exist = from T_LeaveList in db.T_LeaveList where (T_LeaveList.StudentID == ST_Num) && (((T_LeaveList.TimeLeave >= time_go) && (T_LeaveList.TimeLeave <= time_back)) || ((T_LeaveList.TimeBack >= time_go) && (T_LeaveList.TimeBack <= time_back)) || ((T_LeaveList.TimeLeave <= time_go) && (T_LeaveList.TimeBack >= time_back))) select T_LeaveList;

                        if (exist.Any())
                        {
                            bool flag = true;
                            foreach (qingjia_MVC.Models.T_LeaveList leaveList in exist.ToList())
                            {
                                if (leaveList.StateBack == "0")
                                {
                                    flag = false;
                                    break;
                                }
                            }
                            if (flag)
                            {
                                //插入数据库操作
                                if (Insert_LeaveList(LV_NUM, ST_Num, LL_Type, time_go, time_back, leaveReason, leaveWay, backWay, address, holidayType, null, null, null) == 1)
                                {
                                    string script = String.Format("alert('请假申请成功');");
                                    PageContext.RegisterStartupScript(ActiveWindow.GetHideRefreshReference() +
                                        script);
                                }
                                else
                                {
                                    alertInfo("提交失败", "数据库提交失败，请重新尝试!", "Information");
                                }
                            }
                            else
                            {
                                alertInfo("错误提示", "您已提交过此时间段的请假申请，请不要重复提交！", "Information");
                            }
                        }
                        else
                        {
                            //插入数据库操作
                            if (Insert_LeaveList(LV_NUM, ST_Num, LL_Type, time_go, time_back, leaveReason, leaveWay, backWay, address, holidayType, null, null, null) == 1)
                            {
                                string script = String.Format("alert('请假申请成功');");
                                PageContext.RegisterStartupScript(ActiveWindow.GetHideRefreshReference() +
                                    script);
                            }
                            else
                            {
                                alertInfo("提交失败", "数据库提交失败，请重新尝试!", "Information");
                            }
                        }
                    }
                    else
                    {
                        alertInfo("错误提示", "长期请假短期请假不能少于3天!", "Error");
                    }
                }
                else if (LL_Type == "节假日请假")
                {
                    //生成请假单号
                    string LV_NUM = DateTime.Now.ToString("yyMMdd");
                    var exist = from T_LeaveList in db.T_LeaveList where (T_LeaveList.StudentID == ST_Num) && (((T_LeaveList.TimeLeave >= time_go) && (T_LeaveList.TimeLeave <= time_back)) || ((T_LeaveList.TimeBack >= time_go) && (T_LeaveList.TimeBack <= time_back)) || ((T_LeaveList.TimeLeave <= time_go) && (T_LeaveList.TimeBack >= time_back))) select T_LeaveList;

                    if (exist.Any())
                    {
                        bool flag = true;
                        foreach (qingjia_MVC.Models.T_LeaveList leaveList in exist.ToList())
                        {
                            if (leaveList.StateBack == "0")
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                        {
                            //插入数据库操作
                            if (Insert_LeaveList(LV_NUM, ST_Num, LL_Type, time_go, time_back, leaveReason, leaveWay, backWay, address, holidayType, null, null, null) == 1)
                            {
                                string script = String.Format("alert('请假申请成功');");
                                PageContext.RegisterStartupScript(ActiveWindow.GetHideRefreshReference() +
                                    script);
                            }
                            else
                            {
                                alertInfo("提交失败", "数据库提交失败，请重新尝试!", "Information");
                            }
                        }
                        else
                        {
                            alertInfo("错误提示", "您已提交过此时间段的请假申请，请不要重复提交！", "Information");
                        }
                    }
                    else
                    {
                        //插入数据库操作
                        if (Insert_LeaveList(LV_NUM, ST_Num, LL_Type, time_go, time_back, leaveReason, leaveWay, backWay, address, holidayType, null, null, null) == 1)
                        {
                            string script = String.Format("alert('请假申请成功');");
                            PageContext.RegisterStartupScript(ActiveWindow.GetHideRefreshReference() +
                                script);
                        }
                        else
                        {
                            alertInfo("提交失败", "数据库提交失败，请重新尝试!", "Information");
                        }
                    }
                }
                else
                {
                    //非离校请假类型
                }
            }
            else
            {
                alertInfo("错误提示", "请假开始时间不能小于结束时间!", "Error");
            }
            return UIHelper.Result();
        }

        #endregion

        #region 特殊请假

        //GET: Leave/LeaveList/leavespecial
        public ActionResult leavespecial()
        {
            leavespecial_LoadData();
            return View();
        }

        public void leavespecial_LoadData()
        {
            //加载个人基本信息
            string ST_Num = Session["UserID"].ToString();
            var ST_Info = from vw_Student in db.vw_Student where (vw_Student.ST_Num == ST_Num) select vw_Student;
            if (ST_Info.Any())
            {
                ViewData["LL_ST_Info"] = ST_Info.ToList().First();
            }
            else
            {

            }

            //非数据库数据
            List<string> type = new List<string>();
            type.Add("晚点名请假");
            type.Add("早晚自习请假");
            ViewBag.LeaveType = type;

            //ViewBag.ChildType
            List<string> ChildType = new List<string>();
            ChildType.Add("公假");
            ChildType.Add("事假");
            ChildType.Add("病假");
            ViewBag.ChildType = ChildType;
        }

        public ActionResult leaveSpecial_btnSubmit_Click(FormCollection formInfo)
        {
            string ST_Num = Session["UserID"].ToString();
            string LL_Type = formInfo["LL_Type"] + "(" + formInfo["LL_Type_Child"] + ")";
            string leaveDate = formInfo["leaveDate"];
            string leaveTime = "00:00";
            string backDate = formInfo["leaveDate"];
            string backTime = formInfo["leaveTime"];
            string leaveReason = formInfo["leaveReason"];
            string leaveWay = null;
            string backWay = null;
            string address = null;
            string holidayType = null;


            if (formInfo["LL_Type"] == "晚点名请假")
            {
                string LV_NUM = DateTime.Now.ToString("yyMMdd");//流水号生成
                string studytime_sp = leaveDate + " " + leaveTime + ":00";
                DateTime time_go = Convert.ToDateTime(studytime_sp);
                DateTime time_back = time_go;
                var exist = from T_LeaveList in db.T_LeaveList where ((T_LeaveList.StudentID == ST_Num) && (T_LeaveList.TimeLeave == time_go) && (T_LeaveList.TypeID == 2)) select T_LeaveList;

                if (exist.Any())
                {
                    bool flag = true;
                    foreach (qingjia_MVC.Models.T_LeaveList leaveList in exist.ToList())
                    {
                        if (leaveList.StateBack == "0")
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        //插入数据库操作
                        if (Insert_LeaveList(LV_NUM, ST_Num, LL_Type, time_go, time_back, leaveReason, leaveWay, backWay, address, holidayType, null, null, null) == 1)
                        {
                            string script = String.Format("alert('请假申请成功！');");
                            PageContext.RegisterStartupScript(ActiveWindow.GetHideRefreshReference() +
                                script);
                        }
                        else
                        {
                            alertInfo("提交失败", "数据库提交失败，请重新尝试!", "Information");
                        }
                    }
                    else
                    {
                        alertInfo("错误提示", "您已提交过此时间段的请假申请，请不要重复提交！", "Information");
                    }
                }
                else
                {
                    //插入数据库操作
                    if (Insert_LeaveList(LV_NUM, ST_Num, LL_Type, time_go, time_back, leaveReason, leaveWay, backWay, address, holidayType, null, null, null) == 1)
                    {
                        string script = String.Format("alert('请假申请成功');");
                        PageContext.RegisterStartupScript(ActiveWindow.GetHideRefreshReference() +
                            script);
                    }
                    else
                    {
                        alertInfo("提交失败", "数据库提交失败，请重新尝试!", "Information");
                    }
                }
            }
            else if (formInfo["LL_Type"] == "早晚自习请假")
            {
                string LV_NUM = DateTime.Now.ToString("yyMMdd");//流水号生成
                string studytime_sp = leaveDate + " " + leaveTime + ":00";
                DateTime time_go = Convert.ToDateTime(studytime_sp);
                DateTime time_back = time_go;
                var exist = from T_LeaveList in db.T_LeaveList where ((T_LeaveList.StudentID == ST_Num) && (T_LeaveList.TimeLeave == time_go) && (T_LeaveList.TypeID == 2)) select T_LeaveList;

                if (exist.Any())
                {
                    bool flag = true;
                    foreach (qingjia_MVC.Models.T_LeaveList leaveList in exist.ToList())
                    {
                        if (leaveList.StateBack == "0")
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        //插入数据库操作
                        if (Insert_LeaveList(LV_NUM, ST_Num, LL_Type, time_go, time_back, leaveReason, leaveWay, backWay, address, holidayType, null, null, null) == 1)
                        {
                            string script = String.Format("alert('请假申请成功！');");
                            PageContext.RegisterStartupScript(ActiveWindow.GetHideRefreshReference() +
                                script);
                        }
                        else
                        {
                            alertInfo("提交失败", "数据库提交失败，请重新尝试!", "Information");
                        }
                    }
                    else
                    {
                        alertInfo("错误提示", "您已提交过此时间段的请假申请，请不要重复提交！", "Information");
                    }
                }
                else
                {
                    //插入数据库操作
                    if (Insert_LeaveList(LV_NUM, ST_Num, LL_Type, time_go, time_back, leaveReason, leaveWay, backWay, address, holidayType, null, null, null) == 1)
                    {
                        string script = String.Format("alert('请假申请成功');");
                        PageContext.RegisterStartupScript(ActiveWindow.GetHideRefreshReference() +
                            script);
                    }
                    else
                    {
                        alertInfo("提交失败", "数据库提交失败，请重新尝试!", "Information");
                    }
                }
            }
            return UIHelper.Result();
        }

        #endregion

        #region 上课请假备案

        //GET: Leave/LeaveList/leaveclass
        public ActionResult leaveclass()
        {
            leaveclass_LoadData();
            return View();
        }

        //leaveclass 页面加载数据
        public void leaveclass_LoadData()
        {
            //加载个人基本信息
            string ST_Num = Session["UserID"].ToString();
            var ST_Info = from vw_Student in db.vw_Student where (vw_Student.ST_Num == ST_Num) select vw_Student;
            if (ST_Info.Any())
            {
                ViewData["LL_ST_Info"] = ST_Info.ToList().First();
            }

            //非数据库数据
            List<string> type = new List<string>();
            type.Add("第一大节（08:00~09:40）");
            type.Add("第二大节（10:10~11:50）");
            type.Add("第三大节（14:00~15:40）");
            type.Add("第四大节（16:00~17:40）");
            type.Add("第五大节（18:30~21:40）");
            ViewBag.LeaveType = type;

            //ViewBag.ChildType
            List<string> ChildType = new List<string>();
            ChildType.Add("公假");
            ChildType.Add("事假");
            ChildType.Add("病假");
            ViewBag.ChildType = ChildType;
        }

        //leaveclass 页面提交操作
        public ActionResult leaveClass_btnSubmit_Click(FormCollection formInfo)
        {
            string ST_Num = Session["UserID"].ToString();
            string LL_Type = "上课请假备案(" + formInfo["LL_Type_Child"] + ")";
            string leaveDate = formInfo["leaveDate"];
            string leaveTime = formInfo["LL_Type"].ToString().Substring(5, 5);
            string backDate = formInfo["leaveDate"];
            string backTime = formInfo["LL_Type"].ToString().Substring(11, 5);
            string leaveReason = formInfo["leaveReason"];
            string lesson = "";

            //string lesson = formInfo["LL_Type"];
            if (formInfo["LL_Type"].ToString().Contains("第一大节"))
            {
                lesson = "1";
            }
            else if (formInfo["LL_Type"].ToString().Contains("第二大节"))
            {
                lesson = "2";
            }
            else if (formInfo["LL_Type"].ToString().Contains("第三大节"))
            {
                lesson = "3";
            }
            else if (formInfo["LL_Type"].ToString().Contains("第四大节"))
            {
                lesson = "4";
            }
            else if (formInfo["LL_Type"].ToString().Contains("第五大节"))
            {
                lesson = "5";
            }
            else
            {
                lesson = "";
            }

            string teacher = formInfo["txbTeacher"];

            string LV_NUM = DateTime.Now.ToString("yyMMdd");//流水号生成
            DateTime time_go = Convert.ToDateTime(leaveDate + " " + leaveTime + ":00");
            DateTime time_back = Convert.ToDateTime(backDate + " " + backTime + ":00");

            var exist = from T_LeaveList in db.T_LeaveList where ((T_LeaveList.StudentID == ST_Num) && (T_LeaveList.Lesson == lesson) && (T_LeaveList.TypeID == 3) && (T_LeaveList.TimeLeave == time_go)) select T_LeaveList;

            if (exist.Any())
            {
                bool flag = true;
                foreach (qingjia_MVC.Models.T_LeaveList leaveList in exist.ToList())
                {
                    if (leaveList.StateBack == "0")
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    //插入数据库操作
                    if (Insert_LeaveList(LV_NUM, ST_Num, LL_Type, time_go, time_back, leaveReason, null, null, null, null, null, lesson, teacher) == 1)
                    {
                        string script = String.Format("alert('请假申请成功！');");
                        PageContext.RegisterStartupScript(ActiveWindow.GetHideRefreshReference() +
                            script);
                    }
                    else
                    {
                        alertInfo("提交失败", "数据库提交失败，请重新尝试!", "Information");
                    }
                }
                else
                {
                    alertInfo("错误提示", "您已提交过此时间段的请假申请，请不要重复提交！", "Information");
                }
            }
            else
            {
                //插入数据库操作
                if (Insert_LeaveList(LV_NUM, ST_Num, LL_Type, time_go, time_back, leaveReason, null, null, null, null, null, lesson, teacher) == 1)
                {
                    string script = String.Format("alert('请假申请成功');");
                    PageContext.RegisterStartupScript(ActiveWindow.GetHideRefreshReference() +
                        script);
                }
                else
                {
                    alertInfo("提交失败", "数据库提交失败，请重新尝试!", "Information");
                }
            }
            return UIHelper.Result();
        }

        #endregion

        #region 实习请假
        //GET:Leave/LeaveList/leaveinternship
        public ActionResult leaveinternship()
        {
            leaveinternship_LoadData();
            return View();
        }

        //leaveinternship 加载数据
        protected void leaveinternship_LoadData()
        {
            string ST_Num = Session["UserID"].ToString();
            var ST_Info = from vw_Student in db.vw_Student where (vw_Student.ST_Num == ST_Num) select vw_Student;
            if (ST_Info.Any())
            {
                ViewData["LL_ST_Info"] = ST_Info.ToList().First();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult filePhoto_FileSelected1(HttpPostedFileBase filePhoto1, FormCollection formInfo)//此处的filePhoto1和前台的ID以及参数的filePhoto1需要相对应（一模一样），所以必须写三个函数来处理三张图片
        {
            if (filePhoto1 != null)
            {
                string fileName = filePhoto1.FileName;

                if (!ValidateFileType(fileName))
                {
                    // 清空文件上传组件
                    UIHelper.FileUpload("filePhoto1").Reset();
                    ShowNotify("无效的文件类型！");
                }
                else
                {
                    fileName = fileName.Substring(fileName.LastIndexOf(".") + 1, (fileName.Length - fileName.LastIndexOf(".") - 1)); //扩展名
                    fileName = DateTime.Now.ToString() + "_" + Session["UserID"].ToString() + "_" + "pic1." + fileName;
                    fileName = fileName.Replace(":", "").Replace(" ", "").Replace("\\", "_").Replace("/", "_");

                    filePhoto1.SaveAs(Server.MapPath(@"~\media\upload\internship\" + fileName));//此处的路径为保存在磁盘的路径，要用双反斜杠（避免转义,或者把@放在双引号前）

                    //生成缩略图并保存
                    GetPicThumbnail(Server.MapPath(@"~\media\upload\internship\" + fileName), Server.MapPath(@"~\media\upload\internship\thumbnail\" + fileName), 1200, 1200, 50);

                    UIHelper.Image("imgPhoto1").ImageUrl("~/media/upload/internship/thumbnail/" + fileName);//此处的路径因为是url，所以要用单正斜杠
                    UIHelper.TextBox("imgUrl1").Text("~/media/upload/internship/" + fileName);//将url路径保存至隐藏的空间中，方便提交时提取

                    // 清空文件上传组件（上传后要记着清空，否则点击提交表单时会再次上传！！）
                    UIHelper.FileUpload("filePhoto1").Reset();
                }
            }

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult filePhoto_FileSelected2(HttpPostedFileBase filePhoto2, FormCollection formInfo)
        {
            if (filePhoto2 != null)
            {
                string fileName = filePhoto2.FileName;

                if (!ValidateFileType(fileName))
                {
                    // 清空文件上传组件
                    UIHelper.FileUpload("filePhoto2").Reset();
                    ShowNotify("无效的文件类型！");
                }
                else
                {
                    fileName = fileName.Substring(fileName.LastIndexOf(".") + 1, (fileName.Length - fileName.LastIndexOf(".") - 1)); //扩展名
                    fileName = DateTime.Now.ToString() + "_" + Session["UserID"].ToString() + "_" + "pic2." + fileName;
                    fileName = fileName.Replace(":", "").Replace(" ", "").Replace("\\", "_").Replace("/", "_");

                    filePhoto2.SaveAs(Server.MapPath(@"~\media\upload\internship\" + fileName));//此处的路径为保存在磁盘的路径，要用双反斜杠（避免转义,或者把@放在双引号前）

                    //生成缩略图并保存
                    GetPicThumbnail(Server.MapPath(@"~\media\upload\internship\" + fileName), Server.MapPath(@"~\media\upload\internship\thumbnail\" + fileName), 1200, 1200, 50);

                    UIHelper.Image("imgPhoto2").ImageUrl("~/media/upload/internship/thumbnail/" + fileName);//此处的路径因为是url，所以要用单正斜杠
                    UIHelper.TextBox("imgUrl2").Text("~/media/upload/internship/" + fileName);//将url路径保存至隐藏的空间中，方便提交时提取

                    // 清空文件上传组件（上传后要记着清空，否则点击提交表单时会再次上传！！）
                    UIHelper.FileUpload("filePhoto2").Reset();
                }
            }

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult filePhoto_FileSelected3(HttpPostedFileBase filePhoto3, FormCollection formInfo)
        {
            if (filePhoto3 != null)
            {
                string fileName = filePhoto3.FileName;

                if (!ValidateFileType(fileName))
                {
                    // 清空文件上传组件
                    UIHelper.FileUpload("filePhoto3").Reset();
                    ShowNotify("无效的文件类型！");
                }
                //else
                //{
                //    fileName = fileName.Replace(":", "_").Replace(" ", "_").Replace("\\", "_").Replace("/", "_");
                //    fileName = DateTime.Now.Ticks.ToString() + "_" + fileName;

                //    filePhoto3.SaveAs(Server.MapPath("~/upload/" + fileName));

                //    UIHelper.Image("imgPhoto3").ImageUrl("~/upload/" + fileName);

                //    // 清空文件上传组件（上传后要记着清空，否则点击提交表单时会再次上传！！）
                //    UIHelper.FileUpload("filePhoto3").Reset();
                //}
                else
                {
                    fileName = fileName.Substring(fileName.LastIndexOf(".") + 1, (fileName.Length - fileName.LastIndexOf(".") - 1)); //扩展名
                    fileName = DateTime.Now.ToString() + "_" + Session["UserID"].ToString() + "_" + "pic3." + fileName;
                    fileName = fileName.Replace(":", "").Replace(" ", "").Replace("\\", "_").Replace("/", "_");

                    filePhoto3.SaveAs(Server.MapPath(@"~\media\upload\internship\" + fileName));//此处的路径为保存在磁盘的路径，要用双反斜杠（避免转义,或者把@放在双引号前）

                    //生成缩略图并保存
                    GetPicThumbnail(Server.MapPath(@"~\media\upload\internship\" + fileName), Server.MapPath(@"~\media\upload\internship\thumbnail\" + fileName), 1200, 1200, 50);

                    UIHelper.Image("imgPhoto3").ImageUrl("~/media/upload/internship/thumbnail/" + fileName);//此处的路径因为是url，所以要用单正斜杠
                    UIHelper.TextBox("imgUrl3").Text("~/media/upload/internship/" + fileName);//将url路径保存至隐藏的空间中，方便提交时提取

                    // 清空文件上传组件（上传后要记着清空，否则点击提交表单时会再次上传！！）
                    UIHelper.FileUpload("filePhoto3").Reset();
                }
            }

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult leaveInternship_btnSubmit_Click(FormCollection formInfo)
        {
            string ST_Num = Session["UserID"].ToString();
            string IntershipCompany = formInfo["IntershipCompany"];
            string IntershipAddress = formInfo["IntershipAddress"];
            string PrincipalName = formInfo["PrincipalName"];
            string PrincipalTel = formInfo["PrincipalTel"];
            string Note = formInfo["Note"];
            string leaveDate = formInfo["leaveDate"];
            //string leaveTime = formInfo["leaveTime"];
            string backDate = formInfo["backDate"];
            //string backTime = formInfo["backTime"];
            string imgUrl1 = formInfo["imgUrl1"].ToString();
            string imgUrl2 = formInfo["imgUrl2"].ToString();
            string imgUrl3 = formInfo["imgUrl3"].ToString();

            if ((imgUrl1 == "") || (imgUrl2 == ""))
            {
                alertInfo("错误提示", "前两项证明材料必须上传!", "Information");
            }
            else
            {
                //判断并保存本次请假信息
                string LV_Time_Go = leaveDate + " " + "00:00:00";
                string LV_Time_Back = backDate + " " + "00:00:00";

                if (Convert.ToDateTime(LV_Time_Go) < Convert.ToDateTime(LV_Time_Back))
                {
                    DateTime TimeLeave = Convert.ToDateTime(LV_Time_Go);
                    DateTime TimeBack = Convert.ToDateTime(LV_Time_Back);
                    TimeSpan time_days = TimeBack - TimeLeave;
                    int days = time_days.Days;
                    //生成请假单号
                    string LV_Num = DateTime.Now.ToString("yyMMdd");
                    var exist = from T_LeaveIntership in db.T_LeaveIntership where (T_LeaveIntership.StudentID == ST_Num) && (((T_LeaveIntership.TimeLeave >= TimeLeave) && (T_LeaveIntership.TimeLeave <= TimeBack)) || ((T_LeaveIntership.TimeBack >= TimeLeave) && (T_LeaveIntership.TimeBack <= TimeBack)) || ((T_LeaveIntership.TimeLeave <= TimeLeave) && (T_LeaveIntership.TimeBack >= TimeBack))) select T_LeaveIntership;
                    if (exist.Any())
                    {
                        bool flag = true;
                        foreach (qingjia_MVC.Models.T_LeaveIntership leaveintership in exist.ToList())
                        {
                            if (leaveintership.StateBack == "0")
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                        {
                            //插入数据库操作
                            if (Insert_LeaveInternship(LV_Num, ST_Num, IntershipCompany, IntershipAddress, PrincipalName, PrincipalTel, Note, TimeLeave, TimeBack, imgUrl1, imgUrl2, imgUrl3) == 1)
                            {
                                string script = String.Format("alert('实习请假申请成功');");
                                PageContext.RegisterStartupScript(ActiveWindow.GetHideRefreshReference() +
                                    script);
                            }
                            else
                            {
                                alertInfo("提交失败", "数据库提交失败，请重新尝试!", "Information");
                            }
                        }
                        else
                        {
                            alertInfo("错误提示", "您已提交过此时间段的请假申请，请不要重复提交！", "Information");
                        }
                    }
                    else
                    {
                        //插入数据库操作
                        if (Insert_LeaveInternship(LV_Num, ST_Num, IntershipCompany, IntershipAddress, PrincipalName, PrincipalTel, Note, TimeLeave, TimeBack, imgUrl1, imgUrl2, imgUrl3) == 1)
                        {
                            string script = String.Format("alert('请假申请成功');");
                            PageContext.RegisterStartupScript(ActiveWindow.GetHideRefreshReference() +
                                script);
                        }
                        else
                        {
                            alertInfo("提交失败", "数据库提交失败，请重新尝试!", "Information");
                        }
                    }
                }
                else
                {
                    alertInfo("错误提示", "请假开始时间不能小于结束时间!", "Error");
                }
            }
            return UIHelper.Result();
        }

        //GET:Leave/LeaveList/leaveinternshiplist 获取实习请假记录
        public ActionResult leaveinternshiplist()
        {
            ViewBag.listTable = Get_LIL_DataTable();
            return View();
        }

        /// <summary>
        /// 获取实习请假记录
        /// </summary>
        /// <param name="LL_ID">请假单号</param>
        /// <returns></returns>
        public DataTable Get_LIL_DataTable()
        {
            string roleId = Session["RoleID"].ToString();
            string grade;
            DataTable dtSource = new DataTable();
            //此处将list转为DataTable FineUI的Grid绑定时间类型数据时会发生错误，尚未找到原因。
            //解决办法：将list转为DataTable绑定到Grid，并且将DataTable中值类型为DateTime的列转为字符串类型
            if (roleId == "3")
            {
                grade = Session["Grade"].ToString();
                var internshiplist = from vw_LeaveIntership in db.vw_LeaveIntership where (vw_LeaveIntership.ST_Grade == grade) orderby vw_LeaveIntership.ID descending select vw_LeaveIntership;
                //List 转换为 DataTable
                dtSource = internshiplist.ToDataTable(rec => new object[] { internshiplist });
            }
            else if (roleId == "1")
            {
                string userId = Session["UserID"].ToString();
                var internshiplist = from vw_LeaveIntership in db.vw_LeaveIntership where (vw_LeaveIntership.StudentID == userId) orderby vw_LeaveIntership.ID descending select vw_LeaveIntership;
                //List 转换为 DataTable
                dtSource = internshiplist.ToDataTable(rec => new object[] { internshiplist });
            }
            else
            {
                return null;
            }
            #region 更改DataTable中某一列的属性
            DataTable dtClone = new DataTable();
            dtClone = dtSource.Clone();
            foreach (DataColumn col in dtClone.Columns)
            {
                if (col.ColumnName == "SubmitTime" || col.ColumnName == "TimeLeave" || col.ColumnName == "TimeBack")
                {
                    col.DataType = typeof(string);
                }
            }

            DataColumn newCol = new DataColumn();
            newCol.ColumnName = "auditState";
            newCol.DataType = typeof(string);
            dtClone.Columns.Add(newCol);

            foreach (DataRow row in dtSource.Rows)
            {
                DataRow rowNew = dtClone.NewRow();
                rowNew["ID"] = row["ID"];
                rowNew["StudentID"] = row["StudentID"];
                rowNew["SubmitTime"] = ((DateTime)row["SubmitTime"]).ToString("yyyy-MM-dd HH:mm:ss");//按指定格式输出
                rowNew["StateLeave"] = row["StateLeave"];
                rowNew["StateBack"] = row["StateBack"];
                rowNew["TimeLeave"] = ((DateTime)row["TimeLeave"]).ToString("yyyy-MM-dd HH:mm:ss").Substring(0, 10);
                rowNew["TimeBack"] = ((DateTime)row["TimeBack"]).ToString("yyyy-MM-dd HH:mm:ss").Substring(0, 10);
                rowNew["IntershipCompany"] = row["IntershipCompany"];
                rowNew["IntershipAddress"] = row["IntershipAddress"];
                rowNew["PrincipalName"] = row["PrincipalName"];
                rowNew["PrincipalTel"] = row["PrincipalTel"];
                rowNew["Note"] = row["Note"];
                rowNew["Evidence1"] = row["Evidence1"];
                rowNew["Evidence2"] = row["Evidence2"];
                rowNew["Evidence3"] = row["Evidence3"];
                rowNew["ST_Name"] = row["ST_Name"];
                rowNew["ST_Tel"] = row["ST_Tel"];
                rowNew["ContactOne"] = row["ContactOne"];
                rowNew["OneTel"] = row["OneTel"];
                rowNew["ST_Sex"] = row["ST_Sex"];
                rowNew["ST_Dor"] = row["ST_Dor"];
                rowNew["ST_Class"] = row["ST_Class"];
                rowNew["ST_Grade"] = row["ST_Grade"];
                rowNew["ST_Teacher"] = row["ST_Teacher"];
                rowNew["ST_TeacherID"] = row["ST_TeacherID"];

                //审核状态属性
                rowNew["auditState"] = "Error";
                if (row["StateLeave"].ToString() == "0" && row["StateBack"].ToString() == "0")
                {
                    rowNew["auditState"] = "待审核";
                }
                if (row["StateLeave"].ToString() == "1" && row["StateBack"].ToString() == "0")
                {
                    rowNew["auditState"] = "待销假";
                }
                if (row["StateLeave"].ToString() == "1" && row["StateBack"].ToString() == "1")
                {
                    rowNew["auditState"] = "已销假";
                }
                if (row["StateLeave"].ToString() == "2" && row["StateBack"].ToString() == "1")
                {
                    rowNew["auditState"] = "已驳回";
                }

                dtClone.Rows.Add(rowNew);
            }
            #endregion

            return dtClone;
        }

        #endregion

        #region 通知方法

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

        #endregion

        #region 插入T_LeaveList表

        /// <summary>
        /// 插入请假表
        /// </summary>
        /// <param name="LV_Num">请假单号</param>
        /// <param name="ST_Num">学号</param>
        /// <param name="LL_Type">请假类型名称</param>
        /// <param name="TimeLeave">请假时间</param>
        /// <param name="TimeBack">返校时间</param>
        /// <param name="LeaveReason">请假原因</param>
        /// <param name="leaveWay">离校方式</param>
        /// <param name="backWay">返校方式</param>
        /// <param name="leaveAddress">离校地址</param>
        /// <param name="holidayType">假期去向</param>
        /// <param name="notes">请假驳回理由</param>
        /// <param name="lesson">课程</param>
        /// <param name="teacher">老师</param>
        /// <returns>插入数据库结果</returns>
        protected int Insert_LeaveList(string LV_Num, string ST_Num, string LL_Type, DateTime TimeLeave, DateTime TimeBack, string LeaveReason, string leaveWay, string backWay, string leaveAddress, string holidayType, string notes, string lesson, string teacher)
        {
            string endString = "0001";
            var leavelist = from T_LeaveList in db.T_LeaveList where (T_LeaveList.ID.StartsWith(LV_Num)) orderby T_LeaveList.ID descending select T_LeaveList.ID;
            if (leavelist.Any())
            {
                string leaveNumTop = leavelist.First().ToString().Trim();
                int end = Convert.ToInt32(leaveNumTop.Substring(6, 4));
                end++;
                endString = end.ToString("0000");//按照此格式Tostring
            }
            //请假单号
            LV_Num += endString;//请假单号形如1509050001
            //提交时间
            DateTime nowTime = DateTime.Now;
            //请假类型编号
            var type = from T_Type in db.T_Type where (T_Type.Name == LL_Type) select T_Type;

            int TypeID = (int)((T_Type)type.ToList().First()).FatherID;
            int TypeChildID = (int)((T_Type)type.ToList().First()).ID;

            T_LeaveList LL = new T_LeaveList();
            LL.ID = LV_Num;
            LL.StudentID = ST_Num;
            LL.Reason = LeaveReason;
            LL.SubmitTime = nowTime;
            LL.StateLeave = "0";
            LL.StateBack = "0";
            LL.Notes = notes;
            LL.TypeID = TypeID;
            LL.TypeChildID = TypeChildID;
            LL.TimeLeave = TimeLeave;
            LL.TimeBack = TimeBack;
            LL.LeaveWay = leaveWay;
            LL.BackWay = backWay;
            LL.Address = leaveAddress;
            LL.Lesson = lesson;
            LL.Teacher = teacher;
            db.T_LeaveList.Add(LL);
            try
            {
                return db.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                return 0;
            }
        }

        #endregion

        #region 插入T_LeaveIntership表

        protected int Insert_LeaveInternship(string LV_Num, string ST_Num, string IntershipCompany, string IntershipAddress, string PrincipalName, string PrincipalTel, string Note, DateTime TimeLeave, DateTime TimeBack, string imgUrl1, string imgUrl2, string imgUrl3)
        {
            string endString = "01";
            var leaveinternship = from T_LeaveIntership in db.T_LeaveIntership where (T_LeaveIntership.ID.StartsWith(LV_Num)) orderby T_LeaveIntership.ID descending select T_LeaveIntership.ID;
            if (leaveinternship.Any())
            {
                string leaveNumTop = leaveinternship.First().ToString().Trim();
                int end = Convert.ToInt32(leaveNumTop.Substring(8, 2));
                end++;
                endString = end.ToString("00");//按照此格式Tostring
            }
            //请假单号
            LV_Num = LV_Num + "99" + endString;//请假单号形如1709059901
            //提交时间
            DateTime nowTime = DateTime.Now;
            T_LeaveIntership LI = new T_LeaveIntership();
            LI.ID = LV_Num;
            LI.StudentID = ST_Num;
            LI.SubmitTime = nowTime;
            LI.StateLeave = "0";
            LI.StateBack = "0";
            LI.TimeLeave = TimeLeave;
            LI.TimeBack = TimeBack;
            LI.IntershipCompany = IntershipCompany;
            LI.IntershipAddress = IntershipAddress;
            LI.PrincipalName = PrincipalName;
            LI.PrincipalTel = PrincipalTel;
            LI.Note = Note;
            LI.Evidence1 = imgUrl1;
            LI.Evidence2 = imgUrl2;
            LI.Evidence3 = imgUrl3;
            db.T_LeaveIntership.Add(LI);
            try
            {
                return db.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                return 0;
            }
        }

        #endregion

        #region LeaveList

        //GET: LeaveList/leavelist
        public ActionResult leavelist()
        {
            string RoleID = Session["RoleID"].ToString();
            string UserID = Session["UserID"].ToString();

            //此处的ViewBag.RoleID用于View中的判断，判断是否可以执行
            ViewBag.RoleID = RoleID;
            if (RoleID == "3")//登录用户为辅导员
            {
                Get_LL_DataTable(UserID, "total", RoleID);
            }
            else if (RoleID == "1")//登录用户为学生
            {
                Get_LL_DataTable(UserID, "total", RoleID);
            }
            else
            {

            }
            return View();
        }

        /// <summary>
        /// 获取请假记录DataTable格式
        /// </summary>
        /// <param name="ST_NUM">账号</param>
        /// <param name="type">请假类型</param>
        /// <returns></returns>
        public DataTable Get_LL_DataTable(string UserID, string type, string RoleID)
        {
            DataTable dtSource = new DataTable();

            //此处将list转为DataTable FineUI的Grid绑定时间类型数据时会发生错误，尚未找到原因。
            //解决办法：将list转为DataTable绑定到Grid，并且将DataTable中值类型为DateTime的列转为字符串类型
            if (RoleID == "1")//学生
            {
                #region 根据学生学号获取LeaveList、转换为DataTable格式
                var leavelist = from vw_LeaveList in db.vw_LeaveList where (vw_LeaveList.StudentID == UserID) orderby vw_LeaveList.ID descending select vw_LeaveList;

                if (type == "btnShort")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.StudentID == UserID) && (vw_LeaveList.LeaveType == "短期请假")) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnLong")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.StudentID == UserID) && (vw_LeaveList.LeaveType == "长期请假")) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnHoliday")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.StudentID == UserID) && (vw_LeaveList.LeaveType == "节假日请假")) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnCall")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.StudentID == UserID) && (vw_LeaveList.LeaveType.StartsWith("晚点名请假"))) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnClass")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.StudentID == UserID) && (vw_LeaveList.LeaveType.StartsWith("上课请假"))) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnZixi")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.StudentID == UserID) && (vw_LeaveList.LeaveType.StartsWith("早晚自习"))) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                //List 转换为 DataTable
                dtSource = leavelist.ToDataTable(rec => new object[] { leavelist });
                #endregion
            }
            else if (RoleID == "3")//辅导员
            {
                string TeacherID = Session["UserID"].ToString();
                //获得老师姓名
                T_Teacher teacher = (from T_Teacher in db.T_Teacher where (T_Teacher.ID == TeacherID) select T_Teacher).ToList().First();
                string TeacherName = teacher.Name.ToString();

                //根据老师姓名获取LeaveLIst、转换为DataTable格式
                #region 根据学生学号获取LeaveList、转换为DataTable格式

                var leavelist = from vw_LeaveList in db.vw_LeaveList where (vw_LeaveList.ST_Teacher == TeacherName) orderby vw_LeaveList.ID descending select vw_LeaveList;

                if (type == "btnShort")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.ST_Teacher == TeacherName) && (vw_LeaveList.LeaveType == "短期请假")) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnLong")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.ST_Teacher == TeacherName) && (vw_LeaveList.LeaveType == "长期请假")) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnHoliday")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.ST_Teacher == TeacherName) && (vw_LeaveList.LeaveType == "节假日请假")) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnCall")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.ST_Teacher == TeacherName) && (vw_LeaveList.LeaveType.StartsWith("晚点名请假"))) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnClass")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.ST_Teacher == TeacherName) && (vw_LeaveList.LeaveType.StartsWith("上课请假"))) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnZixi")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.ST_Teacher == TeacherName) && (vw_LeaveList.LeaveType.StartsWith("早晚自习"))) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                //List 转换为 DataTable
                dtSource = leavelist.ToDataTable(rec => new object[] { leavelist });
                #endregion
            }
            else if (RoleID == "2")
            {
                //班级账号、尚未完成
            }

            #region 更改DataTable中某一列的属性
            DataTable dtClone = new DataTable();
            dtClone = dtSource.Clone();
            foreach (DataColumn col in dtClone.Columns)
            {
                if (col.ColumnName == "SubmitTime" || col.ColumnName == "TimeLeave" || col.ColumnName == "TimeBack")
                {
                    col.DataType = typeof(string);
                }
                if (col.ColumnName == "Lesson")
                {
                    col.DataType = typeof(string);
                }
            }

            DataColumn newCol = new DataColumn();
            newCol.ColumnName = "auditState";
            newCol.DataType = typeof(string);
            dtClone.Columns.Add(newCol);

            foreach (DataRow row in dtSource.Rows)
            {
                DataRow rowNew = dtClone.NewRow();
                rowNew["ID"] = row["ID"];
                rowNew["Reason"] = row["Reason"];
                rowNew["StateLeave"] = row["StateLeave"];
                rowNew["StateBack"] = row["StateBack"];
                rowNew["Notes"] = row["Notes"];
                rowNew["TypeID"] = row["TypeID"];
                rowNew["SubmitTime"] = ((DateTime)row["SubmitTime"]).ToString("yyyy-MM-dd HH:mm:ss");//按指定格式输出
                rowNew["TimeLeave"] = ((DateTime)row["TimeLeave"]).ToString("yyyy-MM-dd HH:mm:ss");
                rowNew["TimeBack"] = ((DateTime)row["TimeBack"]).ToString("yyyy-MM-dd HH:mm:ss");
                rowNew["LeaveWay"] = row["LeaveWay"];
                rowNew["BackWay"] = row["BackWay"];
                rowNew["Address"] = row["Address"];
                rowNew["TypeChildID"] = row["TypeChildID"];
                rowNew["Teacher"] = row["Teacher"];
                rowNew["ST_Name"] = row["ST_Name"];
                rowNew["ST_Tel"] = row["ST_Tel"];
                rowNew["ST_Grade"] = row["ST_Grade"];
                rowNew["ST_Class"] = row["ST_Class"];
                rowNew["ST_Teacher"] = row["ST_Teacher"];
                rowNew["StudentID"] = row["StudentID"];
                rowNew["LeaveType"] = row["LeaveType"];
                rowNew["AuditName"] = row["AuditName"];

                //审核状态属性
                rowNew["auditState"] = "Error";
                if (row["StateLeave"].ToString() == "0" && row["StateBack"].ToString() == "0")
                {
                    rowNew["auditState"] = "待审核";
                }
                if (row["StateLeave"].ToString() == "1" && row["StateBack"].ToString() == "0")
                {
                    rowNew["auditState"] = "待销假";
                }
                if (row["StateLeave"].ToString() == "1" && row["StateBack"].ToString() == "1")
                {
                    rowNew["auditState"] = "已销假";
                }
                if (row["StateLeave"].ToString() == "2" && row["StateBack"].ToString() == "1")
                {
                    rowNew["auditState"] = "已驳回";
                }

                //请假课段属性
                rowNew["Lesson"] = "";
                if (row["Lesson"].ToString() == "1")
                {
                    rowNew["Lesson"] = "第一大节（08:00~09:40）";
                }
                if (row["Lesson"].ToString() == "2")
                {
                    rowNew["Lesson"] = "第二大节（10:10~11:50）";
                }
                if (row["Lesson"].ToString() == "3")
                {
                    rowNew["Lesson"] = "第三大节（14:00~15:30）";
                }
                if (row["Lesson"].ToString() == "4")
                {
                    rowNew["Lesson"] = "第四大节（16:00~17:40）";
                }
                if (row["Lesson"].ToString() == "5")
                {
                    rowNew["Lesson"] = "第五大节（18:30~21:40）";
                }

                dtClone.Rows.Add(rowNew);
            }
            #endregion

            //绑定数据源
            ViewBag.leavetable = dtClone;
            return dtClone;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="LL_ID">请假单号</param>
        /// <returns></returns>
        public DataTable Get_LeaveList(string LL_ID)
        {
            //此处将list转为DataTable FineUI的Grid绑定时间类型数据时会发生错误，尚未找到原因。
            //解决办法：将list转为DataTable绑定到Grid，并且将DataTable中值类型为DateTime的列转为字符串类型

            #region 获取LeaveList、转换为DataTable格式
            DataTable dtSource = new DataTable();
            var leavelist = from vw_LeaveList in db.vw_LeaveList where (vw_LeaveList.ID == LL_ID) orderby vw_LeaveList.ID descending select vw_LeaveList;

            //List 转换为 DataTable
            dtSource = leavelist.ToDataTable(rec => new object[] { leavelist });
            #endregion

            #region 更改DataTable中某一列的属性
            DataTable dtClone = new DataTable();
            dtClone = dtSource.Clone();
            foreach (DataColumn col in dtClone.Columns)
            {
                if (col.ColumnName == "SubmitTime" || col.ColumnName == "TimeLeave" || col.ColumnName == "TimeBack")
                {
                    col.DataType = typeof(string);
                }
                if (col.ColumnName == "Lesson")
                {
                    col.DataType = typeof(string);
                }
            }

            DataColumn newCol = new DataColumn();
            newCol.ColumnName = "auditState";
            newCol.DataType = typeof(string);
            dtClone.Columns.Add(newCol);

            foreach (DataRow row in dtSource.Rows)
            {
                DataRow rowNew = dtClone.NewRow();
                rowNew["ID"] = row["ID"];
                rowNew["Reason"] = row["Reason"];
                rowNew["StateLeave"] = row["StateLeave"];
                rowNew["StateBack"] = row["StateBack"];
                rowNew["Notes"] = row["Notes"];
                rowNew["TypeID"] = row["TypeID"];
                rowNew["SubmitTime"] = ((DateTime)row["SubmitTime"]).ToString("yyyy-MM-dd HH:mm:ss");//按指定格式输出
                rowNew["TimeLeave"] = ((DateTime)row["TimeLeave"]).ToString("yyyy-MM-dd HH:mm:ss");
                rowNew["TimeBack"] = ((DateTime)row["TimeBack"]).ToString("yyyy-MM-dd HH:mm:ss");
                rowNew["LeaveWay"] = row["LeaveWay"];
                rowNew["BackWay"] = row["BackWay"];
                rowNew["Address"] = row["Address"];
                rowNew["TypeChildID"] = row["TypeChildID"];
                rowNew["Teacher"] = row["Teacher"];
                rowNew["ST_Name"] = row["ST_Name"];
                rowNew["ST_Tel"] = row["ST_Tel"];
                rowNew["ST_Grade"] = row["ST_Grade"];
                rowNew["ST_Class"] = row["ST_Class"];
                rowNew["ST_Teacher"] = row["ST_Teacher"];
                rowNew["StudentID"] = row["StudentID"];
                rowNew["LeaveType"] = row["LeaveType"];
                rowNew["AuditName"] = row["AuditName"];

                //审核状态属性
                rowNew["auditState"] = "Error";
                if (row["StateLeave"].ToString() == "0" && row["StateBack"].ToString() == "0")
                {
                    rowNew["auditState"] = "待审核";
                }
                if (row["StateLeave"].ToString() == "1" && row["StateBack"].ToString() == "0")
                {
                    rowNew["auditState"] = "待销假";
                }
                if (row["StateLeave"].ToString() == "1" && row["StateBack"].ToString() == "1")
                {
                    rowNew["auditState"] = "已销假";
                }
                if (row["StateLeave"].ToString() == "2" && row["StateBack"].ToString() == "1")
                {
                    rowNew["auditState"] = "已驳回";
                }

                //请假课段属性
                rowNew["Lesson"] = "";
                if (row["Lesson"].ToString() == "1")
                {
                    rowNew["Lesson"] = "第一大节（08:00~09:40）";
                }
                if (row["Lesson"].ToString() == "2")
                {
                    rowNew["Lesson"] = "第二大节（10:10~11:50）";
                }
                if (row["Lesson"].ToString() == "3")
                {
                    rowNew["Lesson"] = "第三大节（14:00~15:30）";
                }
                if (row["Lesson"].ToString() == "4")
                {
                    rowNew["Lesson"] = "第四大节（16:00~17:40）";
                }
                if (row["Lesson"].ToString() == "5")
                {
                    rowNew["Lesson"] = "第五大节（18:30~21:40）";
                }

                dtClone.Rows.Add(rowNew);
            }
            #endregion

            //绑定数据源
            ViewBag.leavetable = dtClone;

            return dtClone;
        }

        //删除记录操作
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnDeleteClick(JArray selectedRows, JArray gridLeaveList_fields)
        {
            foreach (string rowId in selectedRows)
            {
                T_LeaveList leavelist = db.T_LeaveList.Find(rowId);
                db.T_LeaveList.Remove(leavelist);
            }
            db.SaveChanges();

            //UpdateGrid(Grid1_fields);
            UIHelper.Grid("gridLeaveList").DataSource(Get_LL_DataTable(Session["UserID"].ToString(), "", Session["RoleID"].ToString()), gridLeaveList_fields);
            Alert.Show("删除成功！");
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult printWindow_Close()
        {
            Alert.Show("触发了窗体的关闭事件！");
            return UIHelper.Result();
        }


        #region 根据按钮名称检索请假记录
        //更新Grid数据
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnTotal_ReloadData(JArray fields)
        {
            staticLeaveType = "total";
            UIHelper.Grid("gridLeaveList").DataSource(Get_LL_DataTable(Session["UserID"].ToString(), staticLeaveType, Session["RoleID"].ToString()), fields);
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnShort_ReloadData(JArray fields)
        {
            staticLeaveType = "btnShort";
            UIHelper.Grid("gridLeaveList").DataSource(Get_LL_DataTable(Session["UserID"].ToString(), staticLeaveType, Session["RoleID"].ToString()), fields);
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnLong_ReloadData(JArray fields)
        {
            staticLeaveType = "btnLong";
            UIHelper.Grid("gridLeaveList").DataSource(Get_LL_DataTable(Session["UserID"].ToString(), staticLeaveType, Session["RoleID"].ToString()), fields);
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnHoliday_ReloadData(JArray fields)
        {
            staticLeaveType = "btnHoliday";
            UIHelper.Grid("gridLeaveList").DataSource(Get_LL_DataTable(Session["UserID"].ToString(), staticLeaveType, Session["RoleID"].ToString()), fields);
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnCall_ReloadData(JArray fields)
        {
            staticLeaveType = "btnCall";
            UIHelper.Grid("gridLeaveList").DataSource(Get_LL_DataTable(Session["UserID"].ToString(), staticLeaveType, Session["RoleID"].ToString()), fields);
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnClass_ReloadData(JArray fields)
        {
            staticLeaveType = "btnClass";
            UIHelper.Grid("gridLeaveList").DataSource(Get_LL_DataTable(Session["UserID"].ToString(), staticLeaveType, Session["RoleID"].ToString()), fields);
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnZixi_ReloadData(JArray fields)
        {
            staticLeaveType = "btnZixi";
            UIHelper.Grid("gridLeaveList").DataSource(Get_LL_DataTable(Session["UserID"].ToString(), staticLeaveType, Session["RoleID"].ToString()), fields);
            return UIHelper.Result();
        }
        #endregion

        #region 根据搜索条件检索请假记录

        //搜索框一 学生姓名搜索搜索
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TwinTriggerBox1_Trigger2Click(string text, JArray fields)
        {
            //权限检查
            if (CheckPermission())
            {
                // 点击 TwinTriggerBox 的搜索按钮
                var TwinTriggerBox1 = UIHelper.TwinTriggerBox("TwinTriggerBox1");
                string ST_NUM = "";

                if (!String.IsNullOrEmpty(text))
                {
                    // 执行搜索动作
                    var ST_Num_List = from vw_Student in db.vw_Student where (vw_Student.ST_Name == text) select vw_Student.ST_Num;
                    if (ST_Num_List.Any())
                    {
                        if (ST_Num_List.ToList().Count == 1)
                        {
                            ST_NUM = ST_Num_List.ToList().First().ToString();
                            ShowNotify(String.Format("检索完成！", text));
                            //此处查询操发起者为老师，查找的目标为学生  所以RoleID应为1  代表学生
                            //UIHelper.Grid("gridLeaveList").DataSource(Get_LL_DataTable(ST_NUM, staticLeaveType, Session["RoleID"].ToString()), fields);
                            UIHelper.Grid("gridLeaveList").DataSource(Get_LL_DataTable(ST_NUM, staticLeaveType, "1"), fields);
                        }
                        else
                        {
                            ShowNotify(String.Format("姓名为{0}的学生不唯一，请根据其他信息检索！", text));
                        }
                    }
                    else
                    {
                        ShowNotify(String.Format("姓名为{0}的学生不存在，请重新输入！", text));
                    }

                    TwinTriggerBox1.ShowTrigger1(true);
                }
                else
                {
                    ShowNotify("请输入你要搜索的关键词！");
                }
            }
            else
            {
                ShowNotify("您不具备查询权限！");
            }

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TwinTriggerBox1_Trigger1Click(string content)
        {
            // 点击 TwinTriggerBox 的取消按钮
            var TwinTriggerBox1 = UIHelper.TwinTriggerBox("TwinTriggerBox1");

            ShowNotify("取消搜索！");

            // 执行清空动作
            TwinTriggerBox1.Text("");
            TwinTriggerBox1.ShowTrigger1(false);

            return UIHelper.Result();
        }

        //搜索框二 学号搜索
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TwinTriggerBox2_Trigger2Click(string text, JArray fields)
        {
            if (CheckPermission())
            {
                // 点击 TwinTriggerBox 的搜索按钮
                var TwinTriggerBox2 = UIHelper.TwinTriggerBox("TwinTriggerBox2");

                if (!String.IsNullOrEmpty(text))
                {
                    // 执行搜索动作
                    var ST_Info_List = from vw_Student in db.vw_Student where (vw_Student.ST_Num == text) select vw_Student;
                    if (ST_Info_List.Any())
                    {
                        ShowNotify(String.Format("检索完成！"));
                        UIHelper.Grid("gridLeaveList").DataSource(Get_LL_DataTable(text, staticLeaveType, Session["RoleID"].ToString()), fields);
                    }
                    else
                    {
                        ShowNotify(String.Format("学号为{0}的学生不存在，请检查后重新检索！", text));
                    }
                    TwinTriggerBox2.ShowTrigger1(true);
                }
                else
                {
                    ShowNotify("请输入你要搜索的关键词！");
                }
            }
            else
            {
                ShowNotify("您不具备查询权限！");
            }

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TwinTriggerBox2_Trigger1Click(string content)
        {
            // 点击 TwinTriggerBox 的取消按钮
            var TwinTriggerBox2 = UIHelper.TwinTriggerBox("TwinTriggerBox2");


            ShowNotify("取消搜索！");

            // 执行清空动作
            TwinTriggerBox2.Text("");
            TwinTriggerBox2.ShowTrigger1(false);

            return UIHelper.Result();
        }

        //搜索框三 请假单号搜索
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TwinTriggerBox3_Trigger2Click(string text, JArray fields)
        {
            if (CheckPermission())
            {
                // 点击 TwinTriggerBox 的搜索按钮
                var TwinTriggerBox3 = UIHelper.TwinTriggerBox("TwinTriggerBox3");

                if (!String.IsNullOrEmpty(text))
                {
                    // 执行搜索动作
                    DataTable dt = Get_LeaveList(text);
                    if (dt.Rows.Count == 1)
                    {
                        ShowNotify(String.Format("检索完成！"));
                        UIHelper.Grid("gridLeaveList").DataSource(dt, fields);
                    }
                    else
                    {
                        ShowNotify(String.Format("请假单号为{0}的请假记录不存在！", text));
                    }
                    TwinTriggerBox3.ShowTrigger1(true);
                }
                else
                {
                    ShowNotify("请输入你要搜索的关键词！");
                }
            }
            else
            {
                ShowNotify("您不具备查询权限！");
            }

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TwinTriggerBox3_Trigger1Click(string content)
        {
            // 点击 TwinTriggerBox 的取消按钮
            var TwinTriggerBox3 = UIHelper.TwinTriggerBox("TwinTriggerBox3");


            ShowNotify("取消搜索！");

            // 执行清空动作
            TwinTriggerBox3.Text("");
            TwinTriggerBox3.ShowTrigger1(false);

            return UIHelper.Result();
        }

        public bool CheckPermission()
        {
            string RoleID = Session["RoleID"].ToString();
            //具有查询权限、返回true，否则返回false

            if (RoleID == "1")//学生
            {
                return false;
            }
            else if (RoleID == "3")//辅导员
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region 按班级查找  尚未完成
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ddlST_Class_SelectedIndexChanged(string ddlST_Class, string ddlST_ClassDropDownList1_text, JArray fields)
        {
            //按班级、请假类型查找
            //尚未完成

            return UIHelper.Result();
        }
        #endregion
        #endregion

        /// 无损压缩图片    
        /// <param name="sFile">原图片</param>    
        /// <param name="dFile">压缩后保存位置</param>    
        /// <param name="dHeight">高度</param>    
        /// <param name="dWidth"></param>    
        /// <param name="flag">压缩质量(数字越小压缩率越高) 1-100</param>    
        /// <returns></returns>
        public static bool GetPicThumbnail(string sFile, string dFile, int dHeight, int dWidth, int flag)
        {
            System.Drawing.Image iSource = System.Drawing.Image.FromFile(sFile);
            ImageFormat tFormat = iSource.RawFormat;
            int sW = 0, sH = 0;

            //按比例缩放  
            Size tem_size = new Size(iSource.Width, iSource.Height);

            if (tem_size.Width > dHeight || tem_size.Width > dWidth)
            {
                if ((tem_size.Width * dHeight) > (tem_size.Width * dWidth))
                {
                    sW = dWidth;
                    sH = (dWidth * tem_size.Height) / tem_size.Width;
                }
                else
                {
                    sH = dHeight;
                    sW = (tem_size.Width * dHeight) / tem_size.Height;
                }
            }
            else
            {
                sW = tem_size.Width;
                sH = tem_size.Height;
            }

            Bitmap ob = new Bitmap(dWidth, dHeight);
            Graphics g = Graphics.FromImage(ob);

            g.Clear(Color.WhiteSmoke);
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(iSource, new Rectangle((dWidth - sW) / 2, (dHeight - sH) / 2, sW, sH), 0, 0, iSource.Width, iSource.Height, GraphicsUnit.Pixel);

            g.Dispose();
            //以下代码为保存图片时，设置压缩质量    
            EncoderParameters ep = new EncoderParameters();
            long[] qy = new long[1];
            qy[0] = flag;//设置压缩的比例1-100    
            EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
            ep.Param[0] = eParam;
            try
            {
                ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo jpegICIinfo = null;
                for (int x = 0; x < arrayICI.Length; x++)
                {
                    if (arrayICI[x].FormatDescription.Equals("JPEG"))
                    {
                        jpegICIinfo = arrayICI[x];
                        break;
                    }
                }
                if (jpegICIinfo != null)
                {
                    ob.Save(dFile, jpegICIinfo, ep);//dFile是压缩后的新路径    
                }
                else
                {
                    ob.Save(dFile, tFormat);
                }
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                iSource.Dispose();
                ob.Dispose();
            }
        }

    }
}