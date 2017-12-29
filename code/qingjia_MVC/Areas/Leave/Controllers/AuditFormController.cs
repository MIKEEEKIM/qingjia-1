using FineUIMvc;
using Newtonsoft.Json.Linq;
using qingjia_MVC.Common;
using qingjia_MVC.Content;
using qingjia_MVC.Controllers;
using qingjia_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace qingjia_MVC.Areas.Leave.Controllers
{
    public class AuditFormController : BaseController
    {
        //实例化数据库
        private imaw_qingjiaEntities db = new imaw_qingjiaEntities();
        private string staticLeaveType = "total";
        //Session["AuditLeaveType"]     请假审批当前请假类型
        //Session["AuditBackType"]      销假审批当前请假类型
        //Session["AuditState"]         请销假状态
        //Session["AuditClassName"]     当前班级状态

        // GET: Leave/AuditForm/AuditLeave
        public ActionResult AuditLeave()
        {
            //设置Grid标题
            ViewBag.GridLeaveTitle = "全部请假";

            Session["AuditState"] = "leave";
            Session["AuditLeaveType"] = "total";
            Get_LL_DataTable("total");
            LL_Count_Leave();
            //加载班级下拉列表数据
            LoadClassDDL();
            return View();
        }

        // GET: Leave/AuditForm/AuditBack
        public ActionResult AuditBack()
        {
            //设置Grid标题
            ViewBag.GridBackTitle = "全部请假";

            Session["AuditState"] = "back";
            Session["AuditBackType"] = "total";
            Get_LL_DataTable("total");
            LL_Count_Back();
            //加载班级下拉列表数据
            LoadClassDDL();
            return View();
        }

        // GET: Leave/AuditForm/AuditInternship
        public ActionResult AuditInternship()
        {
            //设置Grid标题
            ViewBag.GridBackTitle = "实习请假";
            ViewBag.leavetable = Get_LIL_DataTable("leave", null);
            ViewBag.backtable = Get_LIL_DataTable("back", null);
            return View();
        }

        public ActionResult ModifyLL()
        {
            string LL_ID = Request["LL_ID"].ToString();
            vw_LeaveList modelLL = (from vw_LeaveList in db.vw_LeaveList where (vw_LeaveList.ID == LL_ID) select vw_LeaveList).ToList().First();
            return View(modelLL);
        }

        #region 统计数据

        /// <summary>
        /// 统计条数：请假审批
        /// </summary>
        protected void LL_Count_Leave()
        {
            string grade = Session["Grade"].ToString();
            string ST_TeacherID = Session["UserID"].ToString();

            //检索各类请假条数
            int totalNum = 0;
            int shortNum = 0;
            int longNum = 0;
            int holidayNum = 0;
            int nightNum = 0;
            int classNum = 0;
            int zixiNum = 0;

            //提取待审核请假记录
            DataTable dtSource = new DataTable();
            var leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && (vw_LeaveList.ST_TeacherID == ST_TeacherID)) orderby vw_LeaveList.ID descending select vw_LeaveList;

            //List 转换为 DataTable
            dtSource = leavelist.ToDataTable(rec => new object[] { leavelist });

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
                rowNew["ContactOne"] = row["ContactOne"];
                rowNew["OneTel"] = row["OneTel"];

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

            foreach (DataRow row in dtClone.Rows)
            {
                if (row["LeaveType"].ToString() == "短期请假")
                {
                    shortNum++;
                }
                if (row["LeaveType"].ToString() == "长期请假")
                {
                    longNum++;
                }
                if (row["LeaveType"].ToString() == "节假日请假")
                {
                    holidayNum++;
                }
                if (row["LeaveType"].ToString().Substring(0, 3) == "晚点名")
                {
                    nightNum++;
                }
                if (row["LeaveType"].ToString().Substring(0, 2) == "上课")
                {
                    classNum++;
                }
                if (row["LeaveType"].ToString().Substring(0, 4) == "早晚自习")
                {
                    zixiNum++;
                }
            }
            totalNum = shortNum + longNum + holidayNum + nightNum + classNum + zixiNum;
            ViewBag.totalNumLeave = (totalNum == 0) ? "全部请假" : "全部请假<span class='badge badge-danger'>" + totalNum + "</span>";
            ViewBag.shortNumLeave = (shortNum == 0) ? "短期请假" : "短期请假<span class='badge badge-danger'>" + shortNum + "</span>";
            ViewBag.longNumLeave = (longNum == 0) ? "长期请假" : "长期请假<span class='badge badge-danger'>" + longNum + "</span>";
            ViewBag.holidayNumLeave = (holidayNum == 0) ? "节假日请假" : "节假日请假<span class='badge badge-danger'>" + holidayNum + "</span>";
            ViewBag.nightNumLeave = (nightNum == 0) ? "晚点名请假" : "晚点名请假<span class='badge badge-danger'>" + nightNum + "</span>";
            ViewBag.classNumLeave = (classNum == 0) ? "上课请假备案" : "上课请假备案<span class='badge badge-danger'>" + classNum + "</span>";
            ViewBag.zixiNumLeave = (zixiNum == 0) ? "早晚自习请假" : "早晚自习请假<span class='badge badge-danger'>" + zixiNum + "</span>";

            UIHelper.Button("btnTotal").Text(ViewBag.totalNumLeave);
            UIHelper.Button("btnShort").Text(ViewBag.shortNumLeave);
            UIHelper.Button("btnLong").Text(ViewBag.longNumLeave);
            UIHelper.Button("btnHoliday").Text(ViewBag.holidayNumLeave);
            UIHelper.Button("btnCall").Text(ViewBag.nightNumLeave);
            UIHelper.Button("btnClass").Text(ViewBag.classNumLeave);
            UIHelper.Button("btnZixi").Text(ViewBag.zixiNumLeave);
        }

        /// <summary>
        /// 统计条数：销假审批
        /// </summary>
        protected void LL_Count_Back()
        {
            string grade = Session["Grade"].ToString();
            string ST_TeacherID = Session["UserID"].ToString();

            //检索各类请假条数
            int totalNum = 0;
            int shortNum = 0;
            int longNum = 0;
            int holidayNum = 0;
            int nightNum = 0;
            int classNum = 0;

            //提取待销假记录
            DataTable dtSource = new DataTable();
            var leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.StateLeave == "1") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && (vw_LeaveList.ST_TeacherID == ST_TeacherID)) orderby vw_LeaveList.ID descending select vw_LeaveList;

            //List 转换为 DataTable
            dtSource = leavelist.ToDataTable(rec => new object[] { leavelist });

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
                rowNew["ContactOne"] = row["ContactOne"];
                rowNew["OneTel"] = row["OneTel"];

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

            foreach (DataRow row in dtClone.Rows)
            {
                if (row["LeaveType"].ToString() == "短期请假")
                {
                    shortNum++;
                }
                if (row["LeaveType"].ToString() == "长期请假")
                {
                    longNum++;
                }
                if (row["LeaveType"].ToString() == "节假日请假")
                {
                    holidayNum++;
                }
                if (row["LeaveType"].ToString().Substring(0, 3) == "晚点名")
                {
                    nightNum++;
                }
                if (row["LeaveType"].ToString().Substring(0, 2) == "上课")
                {
                    classNum++;
                }
            }
            totalNum = shortNum + longNum + holidayNum + nightNum + classNum;
            ViewBag.totalNumBack = (totalNum == 0) ? "全部请假" : "全部请假<span class='badge badge-danger'>" + totalNum + "</span>";
            ViewBag.shortNumBack = (shortNum == 0) ? "短期请假" : "短期请假<span class='badge badge-danger'>" + shortNum + "</span>";
            ViewBag.longNumBack = (longNum == 0) ? "长期请假" : "长期请假<span class='badge badge-danger'>" + longNum + "</span>";
            ViewBag.holidayNumBack = (holidayNum == 0) ? "节假日请假" : "节假日请假<span class='badge badge-danger'>" + holidayNum + "</span>";
            ViewBag.nightNumBack = (nightNum == 0) ? "晚点名请假" : "晚点名请假<span class='badge badge-danger'>" + nightNum + "</span>";
            ViewBag.classNumBack = (classNum == 0) ? "上课请假备案" : "上课请假备案<span class='badge badge-danger'>" + classNum + "</span>";

            UIHelper.Button("btnTotal").Text(ViewBag.totalNumBack);
            UIHelper.Button("btnShort").Text(ViewBag.shortNumBack);
            UIHelper.Button("btnLong").Text(ViewBag.longNumBack);
            UIHelper.Button("btnHoliday").Text(ViewBag.holidayNumBack);
            UIHelper.Button("btnCall").Text(ViewBag.nightNumBack);
            UIHelper.Button("btnClass").Text(ViewBag.classNumBack);
        }

        #endregion

        #region AuditLeaveList

        #region 获取请假记录
        /// <summary>
        /// 获取请假记录  DataTable格式
        /// </summary>
        /// <param name="type">请假类型</param>
        /// <returns></returns>
        public DataTable Get_LL_DataTable(string type)
        {
            string grade = Session["Grade"].ToString();
            string ST_TeacherID = Session["UserID"].ToString();

            if (Session["AuditState"].ToString() == "leave")
            {
                //此处将list转为DataTable FineUI的Grid绑定时间类型数据时会发生错误，尚未找到原因。
                //解决办法：将list转为DataTable绑定到Grid，并且将DataTable中值类型为DateTime的列转为字符串类型

                #region 获取LeaveList、转换为DataTable格式
                DataTable dtSource = new DataTable();
                var leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;

                if (type == "btnShort")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.LeaveType == "短期请假") && (vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnLong")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.LeaveType == "长期请假") && (vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnHoliday")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.LeaveType == "节假日请假") && (vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnCall")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.LeaveType.StartsWith("晚点名请假")) && (vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnClass")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.LeaveType.StartsWith("上课请假")) && (vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnZixi")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.LeaveType.StartsWith("早晚自习")) && (vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
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
                    rowNew["ContactOne"] = row["ContactOne"];
                    rowNew["OneTel"] = row["OneTel"];

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
            else if (Session["AuditState"].ToString() == "back")
            {
                //此处将list转为DataTable FineUI的Grid绑定时间类型数据时会发生错误，尚未找到原因。
                //解决办法：将list转为DataTable绑定到Grid，并且将DataTable中值类型为DateTime的列转为字符串类型

                #region 获取LeaveList、转换为DataTable格式
                DataTable dtSource = new DataTable();
                var leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.StateLeave == "1") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;

                if (type == "btnShort")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.LeaveType == "短期请假") && (vw_LeaveList.StateLeave == "1") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnLong")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.LeaveType == "长期请假") && (vw_LeaveList.StateLeave == "1") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnHoliday")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.LeaveType == "节假日请假") && (vw_LeaveList.StateLeave == "1") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnCall")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.LeaveType.StartsWith("晚点名请假")) && (vw_LeaveList.StateLeave == "1") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnClass")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.LeaveType.StartsWith("上课请假")) && (vw_LeaveList.StateLeave == "1") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
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
                    rowNew["ContactOne"] = row["ContactOne"];
                    rowNew["OneTel"] = row["OneTel"];

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
            else
            {
                //未知错误、此处代码退出到登录界面
                return null;
            }
        }

        /// <summary>
        /// 获取请假记录  DataTable格式  并进行排序
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="sortField">排序字段</param>
        /// <param name="sortDirection">排序方式</param>
        /// <returns></returns>
        public DataTable Get_LL_DataTable(string type, string sortField, string sortDirection)
        {
            string grade = Session["Grade"].ToString();
            string ST_TeacherID = Session["UserID"].ToString();

            if (Session["AuditState"].ToString() == "leave")
            {
                //此处将list转为DataTable FineUI的Grid绑定时间类型数据时会发生错误，尚未找到原因。
                //解决办法：将list转为DataTable绑定到Grid，并且将DataTable中值类型为DateTime的列转为字符串类型

                #region 获取LeaveList、转换为DataTable格式
                DataTable dtSource = new DataTable();
                var leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;

                if (type == "btnShort")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.LeaveType == "短期请假") && (vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnLong")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.LeaveType == "长期请假") && (vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnHoliday")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.LeaveType == "节假日请假") && (vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnCall")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.LeaveType.StartsWith("晚点名请假")) && (vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnClass")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.LeaveType.StartsWith("上课请假")) && (vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnZixi")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.LeaveType.StartsWith("早晚自习")) && (vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                //List 转换为 DataTable
                if (sortDirection == "ASC")
                {
                    leavelist = leavelist.OrderBy(ll => ll.ST_Class);
                }
                if (sortDirection == "DESC")
                {
                    leavelist = leavelist.OrderByDescending(ll => ll.ST_Class);
                }
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
                    rowNew["ContactOne"] = row["ContactOne"];
                    rowNew["OneTel"] = row["OneTel"];

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
            else if (Session["AuditState"].ToString() == "back")
            {
                //此处将list转为DataTable FineUI的Grid绑定时间类型数据时会发生错误，尚未找到原因。
                //解决办法：将list转为DataTable绑定到Grid，并且将DataTable中值类型为DateTime的列转为字符串类型

                #region 获取LeaveList、转换为DataTable格式
                DataTable dtSource = new DataTable();
                var leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.StateLeave == "1") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;

                if (type == "btnShort")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.LeaveType == "短期请假") && (vw_LeaveList.StateLeave == "1") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnLong")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.LeaveType == "长期请假") && (vw_LeaveList.StateLeave == "1") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnHoliday")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.LeaveType == "节假日请假") && (vw_LeaveList.StateLeave == "1") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnCall")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.LeaveType.StartsWith("晚点名请假")) && (vw_LeaveList.StateLeave == "1") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnClass")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.LeaveType.StartsWith("上课请假")) && (vw_LeaveList.StateLeave == "1") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                //List 转换为 DataTable
                if (sortDirection == "ASC")
                {
                    leavelist = leavelist.OrderBy(ll => ll.ST_Class);
                }
                if (sortDirection == "DESC")
                {
                    leavelist = leavelist.OrderByDescending(ll => ll.ST_Class);
                }
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
                    rowNew["ContactOne"] = row["ContactOne"];
                    rowNew["OneTel"] = row["OneTel"];

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
            else
            {
                //未知错误、此处代码退出到登录界面
                return null;
            }
        }

        /// <summary>
        /// 根据学号查找请假记录  DataTable格式
        /// </summary>
        /// <param name="ST_NUM"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public DataTable Get_LL_DataTable_BY_ST_Num(string type, string ST_NUM)
        {
            string grade = Session["Grade"].ToString();
            string ST_TeacherID = Session["UserID"].ToString();

            if (Session["AuditState"].ToString() == "leave")
            {
                //此处将list转为DataTable FineUI的Grid绑定时间类型数据时会发生错误，尚未找到原因。
                //解决办法：将list转为DataTable绑定到Grid，并且将DataTable中值类型为DateTime的列转为字符串类型

                #region 获取LeaveList、转换为DataTable格式
                DataTable dtSource = new DataTable();
                var leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.StudentID == ST_NUM) && (vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;

                if (type == "btnShort")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.StudentID == ST_NUM) && (vw_LeaveList.LeaveType == "短期请假") && (vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnLong")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.StudentID == ST_NUM) && (vw_LeaveList.LeaveType == "长期请假") && (vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnHoliday")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.StudentID == ST_NUM) && (vw_LeaveList.LeaveType == "节假日请假") && (vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnCall")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.StudentID == ST_NUM) && (vw_LeaveList.LeaveType.StartsWith("晚点名请假")) && (vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnClass")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.StudentID == ST_NUM) && (vw_LeaveList.LeaveType.StartsWith("上课请假")) && (vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnZixi")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.StudentID == ST_NUM) && (vw_LeaveList.LeaveType.StartsWith("早晚自习")) && (vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
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
                    rowNew["ContactOne"] = row["ContactOne"];
                    rowNew["OneTel"] = row["OneTel"];

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
            else if (Session["AuditState"].ToString() == "back")
            {
                //此处将list转为DataTable FineUI的Grid绑定时间类型数据时会发生错误，尚未找到原因。
                //解决办法：将list转为DataTable绑定到Grid，并且将DataTable中值类型为DateTime的列转为字符串类型

                #region 获取LeaveList、转换为DataTable格式
                DataTable dtSource = new DataTable();
                var leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.StudentID == ST_NUM) && (vw_LeaveList.StateLeave == "1") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;

                if (type == "btnShort")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.StudentID == ST_NUM) && (vw_LeaveList.LeaveType == "短期请假") && (vw_LeaveList.StateLeave == "1") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnLong")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.StudentID == ST_NUM) && (vw_LeaveList.LeaveType == "长期请假") && (vw_LeaveList.StateLeave == "1") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnHoliday")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.StudentID == ST_NUM) && (vw_LeaveList.LeaveType == "节假日请假") && (vw_LeaveList.StateLeave == "1") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnCall")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.StudentID == ST_NUM) && (vw_LeaveList.LeaveType.StartsWith("晚点名请假")) && (vw_LeaveList.StateLeave == "1") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnClass")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.StudentID == ST_NUM) && (vw_LeaveList.LeaveType.StartsWith("上课请假")) && (vw_LeaveList.StateLeave == "1") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
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
                    rowNew["ContactOne"] = row["ContactOne"];
                    rowNew["OneTel"] = row["OneTel"];

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
            else
            {
                //未知错误、此处代码退出到登录界面
                return null;
            }
        }

        /// <summary>
        /// 根据班级名称查找请假记录  DataTable格式
        /// </summary>
        /// <param name="type"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        public DataTable Get_LL_DataTable_BY_ClassName(string type, string className)
        {
            string grade = Session["Grade"].ToString();
            string ST_TeacherID = Session["UserID"].ToString();

            if (Session["AuditState"].ToString() == "leave")
            {
                //此处将list转为DataTable FineUI的Grid绑定时间类型数据时会发生错误，尚未找到原因。
                //解决办法：将list转为DataTable绑定到Grid，并且将DataTable中值类型为DateTime的列转为字符串类型

                #region 获取LeaveList、转换为DataTable格式
                DataTable dtSource = new DataTable();
                var leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.ST_Class == className) && (vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;

                if (type == "btnShort")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.ST_Class == className) && (vw_LeaveList.LeaveType == "短期请假") && (vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnLong")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.ST_Class == className) && (vw_LeaveList.LeaveType == "长期请假") && (vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnHoliday")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.ST_Class == className) && (vw_LeaveList.LeaveType == "节假日请假") && (vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnCall")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.ST_Class == className) && (vw_LeaveList.LeaveType.StartsWith("晚点名请假")) && (vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnClass")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.ST_Class == className) && (vw_LeaveList.LeaveType.StartsWith("上课请假")) && (vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnZixi")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.ST_Class == className) && (vw_LeaveList.LeaveType.StartsWith("早晚自习")) && (vw_LeaveList.StateLeave == "0") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
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
                    rowNew["ContactOne"] = row["ContactOne"];
                    rowNew["OneTel"] = row["OneTel"];

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
            else if (Session["AuditState"].ToString() == "back")
            {
                //此处将list转为DataTable FineUI的Grid绑定时间类型数据时会发生错误，尚未找到原因。
                //解决办法：将list转为DataTable绑定到Grid，并且将DataTable中值类型为DateTime的列转为字符串类型

                #region 获取LeaveList、转换为DataTable格式
                DataTable dtSource = new DataTable();
                var leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.ST_Class == className) && (vw_LeaveList.StateLeave == "1") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;

                if (type == "btnShort")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.ST_Class == className) && (vw_LeaveList.LeaveType == "短期请假") && (vw_LeaveList.StateLeave == "1") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnLong")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.ST_Class == className) && (vw_LeaveList.LeaveType == "长期请假") && (vw_LeaveList.StateLeave == "1") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnHoliday")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.ST_Class == className) && (vw_LeaveList.LeaveType == "节假日请假") && (vw_LeaveList.StateLeave == "1") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnCall")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.ST_Class == className) && (vw_LeaveList.LeaveType.StartsWith("晚点名请假")) && (vw_LeaveList.StateLeave == "1") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
                if (type == "btnClass")
                {
                    leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.ST_Class == className) && (vw_LeaveList.LeaveType.StartsWith("上课请假")) && (vw_LeaveList.StateLeave == "1") && (vw_LeaveList.StateBack == "0") && (vw_LeaveList.ST_Grade == grade) && vw_LeaveList.ST_TeacherID == ST_TeacherID) orderby vw_LeaveList.ID descending select vw_LeaveList;
                }
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
                    rowNew["ContactOne"] = row["ContactOne"];
                    rowNew["OneTel"] = row["OneTel"];

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
            else
            {
                //未知错误、此处代码退出到登录界面
                return null;
            }
        }
        #endregion

        /// <summary>
        /// 根据请假单号查找
        /// </summary>
        /// <param name="LL_ID">请假单号</param>
        /// <returns></returns>
        public DataTable Get_LeaveList(string LL_ID)
        {
            if (Session["AuditState"].ToString() == "leave")
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
                    rowNew["ContactOne"] = row["ContactOne"];
                    rowNew["OneTel"] = row["OneTel"];

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
            else if (Session["AuditState"].ToString() == "back")
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
                    rowNew["ContactOne"] = row["ContactOne"];
                    rowNew["OneTel"] = row["OneTel"];

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
            else
            {
                //未知错误、此处代码退出到登录界面
                return null;
            }
        }

        /// <summary>
        /// 同意请假操作，Click事件
        /// </summary>
        /// <param name="selectedRows"></param>
        /// <param name="gridLeaveList_fields"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnAgreeClick_Leave(JArray selectedRows, JArray gridLeaveList_fields, string sortField, string sortDirection)
        {
            //请假类型
            string type = Session["AuditLeaveType"].ToString();
            //短信模板
            MessageModel model = new MessageModel();
            //是否需要打印假条
            bool isPrint = false;

            #region 同意请假操作
            foreach (string rowId in selectedRows)
            {
                var LL = from vw_LeaveList in db.vw_LeaveList where (vw_LeaveList.ID == rowId) select vw_LeaveList;
                vw_LeaveList vw_LL = LL.ToList().First();
                if (vw_LL.TypeID == 1 || vw_LL.LeaveType.ToString().Substring(0, 3) == "晚点名")
                {
                    //离校请假和晚点名请假需要进行销假
                    T_LeaveList T_LL = db.T_LeaveList.Find(rowId);
                    //将请假记录状态修改为待销假状态
                    T_LL.StateLeave = "1";
                    T_LL.StateBack = "0";
                    //将审核人修改为辅导员姓名
                    T_LL.AuditTeacherID = Session["UserID"].ToString();
                }
                else
                {
                    //上课请假备案和早晚自习请假不需要销假
                    T_LeaveList T_LL = db.T_LeaveList.Find(rowId);
                    //将请假记录状态修改为已销假状态
                    T_LL.StateLeave = "1";
                    T_LL.StateBack = "1";
                }

                model.LV_Num = vw_LL.ID;
                model.ST_Num = vw_LL.StudentID;
                model.ST_Name = vw_LL.ST_Name;
                model.ST_Tel = vw_LL.ST_Tel;
                model.MessageType = "go";
            }
            try
            {
                db.SaveChanges();

                ShowNotify(string.Format("已同意请假！"));
            }
            catch
            {
                ShowNotify(string.Format("操作失败！"));
            }

            //发送短信
            SendSms.sendSms(model);

            //绑定Grid数据
            if (Session["AuditClassName"] == null || Session["AuditClassName"].ToString() == "-1")
            {
                UIHelper.Grid("gridLeaveList_Leave").DataSource(Get_LL_DataTable(type, sortField, sortDirection), gridLeaveList_fields);
            }
            else
            {
                string AuditClassName = Session["AuditClassName"].ToString();
                UIHelper.Grid("gridLeaveList_Leave").DataSource(Get_LL_DataTable_BY_ClassName(type, AuditClassName), gridLeaveList_fields);
            }

            //绑定Button数据
            LL_Count_Leave();

            return UIHelper.Result();
            #endregion
        }

        /// <summary>
        /// 同意销假操作，Click事件
        /// </summary>
        /// <param name="selectedRows"></param>
        /// <param name="gridLeaveList_fields"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnAgreeClick_Back(JArray selectedRows, JArray gridLeaveList_fields, string sortField, string sortDirection)
        {
            string type = Session["AuditBackType"].ToString();
            MessageModel model = new MessageModel();

            #region 同意销假操作
            foreach (string rowId in selectedRows)
            {
                //vw_LeaveList vw_LL = db.vw_LeaveList.Find(rowId);
                var LL = from vw_LeaveList in db.vw_LeaveList where (vw_LeaveList.ID == rowId) select vw_LeaveList;
                vw_LeaveList vw_LL = LL.ToList().First();
                if (vw_LL.TypeID == 1 || vw_LL.LeaveType.ToString().Substring(0, 3) == "晚点名")
                {
                    //离校请假和晚点名请假需要进行销假
                    T_LeaveList T_LL = db.T_LeaveList.Find(rowId);
                    //将请假记录状态修改为已销假状态
                    T_LL.StateLeave = "1";
                    T_LL.StateBack = "1";
                }
                else
                {
                    //上课请假备案和早晚自习请假不需要销假
                    T_LeaveList T_LL = db.T_LeaveList.Find(rowId);
                    //将请假记录状态修改为已销假状态
                    T_LL.StateLeave = "1";
                    T_LL.StateBack = "1";
                }

                model.LV_Num = vw_LL.ID;
                model.ST_Num = vw_LL.StudentID;
                model.ST_Name = vw_LL.ST_Name;
                model.ST_Tel = vw_LL.ST_Tel;
                model.MessageType = "back";
            }
            db.SaveChanges();
            //发送短信
            SendSms.sendSms(model);

            ShowNotify(String.Format("已同意销假！"));

            //绑定Grid数据
            if (Session["AuditClassName"] == null || Session["AuditClassName"].ToString() == "-1")
            {
                UIHelper.Grid("gridLeaveList_Back").DataSource(Get_LL_DataTable(type, sortField, sortDirection), gridLeaveList_fields);
            }
            else
            {
                string AuditClassName = Session["AuditClassName"].ToString();
                UIHelper.Grid("gridLeaveList_Back").DataSource(Get_LL_DataTable_BY_ClassName(type, AuditClassName), gridLeaveList_fields);
            }
            //绑定Button数据
            LL_Count_Back();

            return UIHelper.Result();
            #endregion
        }

        /// <summary>
        /// 驳回请假操作，Click事件
        /// </summary>
        /// <param name="formInfo"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnCancelClick(FormCollection formInfo, JArray fields, string sortField, string sortDirection)
        {
            string type = Session["AuditLeaveType"].ToString();
            string LL_NUM = Session["LL_NUM"].ToString();
            MessageModel model = new MessageModel();

            string reason = formInfo["Reason"].ToString();
            T_LeaveList LL = db.T_LeaveList.Find(LL_NUM);
            LL.Notes = reason;
            LL.StateLeave = "2";
            LL.StateBack = "1";

            db.SaveChanges();

            vw_LeaveList vw_LL = (from vw_LeaveList in db.vw_LeaveList where (vw_LeaveList.ID == LL_NUM) select vw_LeaveList).ToList().First();

            model.LV_Num = vw_LL.ID;
            model.ST_Num = vw_LL.StudentID;
            model.ST_Name = vw_LL.ST_Name;
            model.ST_Tel = vw_LL.ST_Tel;
            model.MessageType = "failed";
            //发送短信
            SendSms.sendSms(model);

            UIHelper.Window("cancelWindow").Close();
            ShowNotify(string.Format("驳回请假成功！"));
            //绑定Grid数据
            UIHelper.Grid("gridLeaveList_Leave").DataSource(Get_LL_DataTable(type, sortField, sortDirection), fields);
            //绑定Button数据
            LL_Count_Leave();

            return UIHelper.Result();
        }

        /// <summary>
        /// 弹出驳回框
        /// </summary>
        /// <param name="selectedRows"></param>
        /// <param name="gridLeaveList_fields"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult cancelWindow_Show(JArray selectedRows, JArray gridLeaveList_fields)
        {
            string LL_NUM = selectedRows.ToList().First().ToString();
            Session["LL_NUM"] = LL_NUM;
            UIHelper.Window("cancelWindow").Title("驳回 - " + LL_NUM);
            UIHelper.Window("cancelWindow").Show();
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult printWindow_Close()
        {
            Alert.Show("触发了窗体的关闭事件！");
            return UIHelper.Result();
        }

        /// <summary>
        /// 待做？？？？
        /// </summary>
        /// <param name="gridLeaveList"></param>
        /// <returns></returns>
        public ActionResult btnLeave_Click(Grid gridLeaveList)
        {
            //DataTable dt = gridLeaveList.ToDataTable();

            return UIHelper.Result();
        }

        #region 根据按钮名称检索请假记录
        //更新Grid数据

        #region 请假审批界面 请假类型按钮
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnTotal_ReloadData_Leave(JArray fields)
        {
            UIHelper.Grid("gridLeaveList_Leave").Title("全部请假");
            Session["AuditState"] = "leave";
            //staticLeaveType = "total";
            Session["AuditLeaveType"] = "total";
            UIHelper.Grid("gridLeaveList_Leave").DataSource(Get_LL_DataTable(Session["AuditState"].ToString()), fields);
            //绑定Button数据
            LL_Count_Leave();
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnShort_ReloadData_Leave(JArray fields)
        {
            UIHelper.Grid("gridLeaveList_Leave").Title("短期请假");
            Session["AuditState"] = "leave";
            //staticLeaveType = "btnShort";
            Session["AuditLeaveType"] = "btnShort";
            UIHelper.Grid("gridLeaveList_Leave").DataSource(Get_LL_DataTable(Session["AuditLeaveType"].ToString()), fields);
            //绑定Button数据
            LL_Count_Leave();
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnLong_ReloadData_Leave(JArray fields)
        {
            UIHelper.Grid("gridLeaveList_Leave").Title("长期请假");
            Session["AuditState"] = "leave";
            //staticLeaveType = "btnLong";
            Session["AuditLeaveType"] = "btnLong";
            UIHelper.Grid("gridLeaveList_Leave").DataSource(Get_LL_DataTable(Session["AuditLeaveType"].ToString()), fields);
            //绑定Button数据
            LL_Count_Leave();
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnHoliday_ReloadData_Leave(JArray fields)
        {
            UIHelper.Grid("gridLeaveList_Leave").Title("节假日请假");
            Session["AuditState"] = "leave";
            //staticLeaveType = "btnHoliday";
            Session["AuditLeaveType"] = "btnHoliday";
            UIHelper.Grid("gridLeaveList_Leave").DataSource(Get_LL_DataTable(Session["AuditLeaveType"].ToString()), fields);
            //绑定Button数据
            LL_Count_Leave();
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnCall_ReloadData_Leave(JArray fields)
        {
            UIHelper.Grid("gridLeaveList_Leave").Title("晚点名请假");
            Session["AuditState"] = "leave";
            //staticLeaveType = "btnCall";
            Session["AuditLeaveType"] = "btnCall";
            UIHelper.Grid("gridLeaveList_Leave").DataSource(Get_LL_DataTable(Session["AuditLeaveType"].ToString()), fields);
            //绑定Button数据
            LL_Count_Leave();
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnClass_ReloadData_Leave(JArray fields)
        {
            UIHelper.Grid("gridLeaveList_Leave").Title("上课请假备案");
            Session["AuditState"] = "leave";
            Session["AuditLeaveType"] = "btnClass";
            UIHelper.Grid("gridLeaveList_Leave").DataSource(Get_LL_DataTable(Session["AuditLeaveType"].ToString()), fields);
            //绑定Button数据
            LL_Count_Leave();
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnZixi_ReloadData_Leave(JArray fields)
        {
            UIHelper.Grid("gridLeaveList_Leave").Title("早晚自习请假");
            Session["AuditState"] = "leave";
            Session["AuditLeaveType"] = "btnZixi";
            UIHelper.Grid("gridLeaveList_Leave").DataSource(Get_LL_DataTable(Session["AuditLeaveType"].ToString()), fields);
            //绑定Button数据
            LL_Count_Leave();
            return UIHelper.Result();
        }
        #endregion

        #region 销假审批界面 请假类型按钮
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnTotal_ReloadData_Back(JArray fields)
        {
            UIHelper.Grid("gridLeaveList_Back").Title("全部请假");
            Session["AuditState"] = "back";
            //staticLeaveType = "total";
            Session["AuditBackType"] = "total";
            UIHelper.Grid("gridLeaveList_Back").DataSource(Get_LL_DataTable(Session["AuditBackType"].ToString()), fields);
            //绑定Button数据
            LL_Count_Back();
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnShort_ReloadData_Back(JArray fields)
        {
            UIHelper.Grid("gridLeaveList_Back").Title("短期请假");
            Session["AuditState"] = "back";
            //staticLeaveType = "btnShort";
            Session["AuditBackType"] = "btnShort";
            UIHelper.Grid("gridLeaveList_Back").DataSource(Get_LL_DataTable(Session["AuditBackType"].ToString()), fields);
            //绑定Button数据
            LL_Count_Back();
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnLong_ReloadData_Back(JArray fields)
        {
            UIHelper.Grid("gridLeaveList_Back").Title("长期请假");
            Session["AuditState"] = "back";
            //staticLeaveType = "btnLong";
            Session["AuditBackType"] = "btnLong";
            UIHelper.Grid("gridLeaveList_Back").DataSource(Get_LL_DataTable(Session["AuditBackType"].ToString()), fields);
            //绑定Button数据
            LL_Count_Back();
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnHoliday_ReloadData_Back(JArray fields)
        {
            UIHelper.Grid("gridLeaveList_Back").Title("节假日请假");
            Session["AuditState"] = "back";
            //staticLeaveType = "btnHoliday";
            Session["AuditBackType"] = "btnHoliday";
            UIHelper.Grid("gridLeaveList_Back").DataSource(Get_LL_DataTable(Session["AuditBackType"].ToString()), fields);
            //绑定Button数据
            LL_Count_Back();
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnCall_ReloadData_Back(JArray fields)
        {
            UIHelper.Grid("gridLeaveList_Back").Title("晚点名请假");
            Session["AuditState"] = "back";
            staticLeaveType = "btnCall";
            Session["AuditBackType"] = "btnCall";
            UIHelper.Grid("gridLeaveList_Back").DataSource(Get_LL_DataTable(Session["AuditBackType"].ToString()), fields);
            //绑定Button数据
            LL_Count_Back();
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnClass_ReloadData_Back(JArray fields)
        {
            UIHelper.Grid("gridLeaveList_Back").Title("上课请假备案");
            Session["AuditState"] = "back";
            staticLeaveType = "btnClass";
            Session["AuditBackType"] = "btnClass";
            UIHelper.Grid("gridLeaveList_Back").DataSource(Get_LL_DataTable(Session["AuditBackType"].ToString()), fields);
            //绑定Button数据
            LL_Count_Back();
            return UIHelper.Result();
        }
        #endregion

        #endregion

        #region 根据搜索条件检索请假记录

        //搜索框一 学生姓名搜索搜索 请假
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TwinTriggerBox1_Trigger2Click_Leave(string text, JArray fields)
        {
            Session["AuditState"] = "leave";

            // 点击 TwinTriggerBox 的搜索按钮
            var TwinTriggerBox1 = UIHelper.TwinTriggerBox("TwinTriggerBox1");
            string ST_NUM = "";

            if (!string.IsNullOrEmpty(text))
            {
                // 执行搜索动作
                var ST_Num_List = from vw_Student in db.vw_Student where (vw_Student.ST_Name == text) select vw_Student.ST_Num;
                if (ST_Num_List.Any())
                {
                    if (ST_Num_List.ToList().Count == 1)
                    {
                        ST_NUM = ST_Num_List.ToList().First().ToString();
                        ShowNotify(string.Format("检索完成！", text));
                        UIHelper.Grid("gridLeaveList_Leave").DataSource(Get_LL_DataTable_BY_ST_Num(ST_NUM, staticLeaveType), fields);
                    }
                    else
                    {
                        ShowNotify(string.Format("姓名为{0}的学生不唯一，请根据其他信息检索！", text));
                    }
                }
                else
                {
                    ShowNotify(string.Format("姓名为{0}的学生不存在，请重新输入！", text));
                }

                TwinTriggerBox1.ShowTrigger1(true);
            }
            else
            {
                ShowNotify("请输入你要搜索的关键词！");
            }
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TwinTriggerBox1_Trigger1Click_Leave(JArray fields)
        {
            Session["AuditState"] = "leave";

            // 点击 TwinTriggerBox 的取消按钮
            var TwinTriggerBox1 = UIHelper.TwinTriggerBox("TwinTriggerBox1");

            ShowNotify("取消搜索！");

            // 执行清空动作
            TwinTriggerBox1.Text("");
            TwinTriggerBox1.ShowTrigger1(false);

            //初始化Grid数据
            DataTable dtSource = Get_LL_DataTable("total");
            UIHelper.Grid("gridLeaveList_Leave").DataSource(dtSource, fields);

            return UIHelper.Result();
        }

        //搜索框二 学号搜索 请假
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TwinTriggerBox2_Trigger2Click_Leave(string text, JArray fields)
        {
            Session["AuditState"] = "leave";

            // 点击 TwinTriggerBox 的搜索按钮
            var TwinTriggerBox2 = UIHelper.TwinTriggerBox("TwinTriggerBox2");

            if (!String.IsNullOrEmpty(text))
            {
                // 执行搜索动作
                var ST_Info_List = from vw_Student in db.vw_Student where (vw_Student.ST_Num == text) select vw_Student;
                if (ST_Info_List.Any())
                {
                    ShowNotify(String.Format("检索完成！"));
                    UIHelper.Grid("gridLeaveList_Leave").DataSource(Get_LL_DataTable_BY_ST_Num(text, staticLeaveType), fields);
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

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TwinTriggerBox2_Trigger1Click_Leave(JArray fields)
        {
            Session["AuditState"] = "leave";

            // 点击 TwinTriggerBox 的取消按钮
            var TwinTriggerBox2 = UIHelper.TwinTriggerBox("TwinTriggerBox2");

            ShowNotify("取消搜索！");

            // 执行清空动作
            TwinTriggerBox2.Text("");
            TwinTriggerBox2.ShowTrigger1(false);

            //初始化Grid数据
            DataTable dtSource = Get_LL_DataTable("total");
            UIHelper.Grid("gridLeaveList_Leave").DataSource(dtSource, fields);

            return UIHelper.Result();
        }

        //搜索框三 请假单号搜索 请假
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TwinTriggerBox3_Trigger2Click_Leave(string text, JArray fields)
        {
            Session["AuditState"] = "leave";

            // 点击 TwinTriggerBox 的搜索按钮
            var TwinTriggerBox3 = UIHelper.TwinTriggerBox("TwinTriggerBox3");

            if (!String.IsNullOrEmpty(text))
            {
                // 执行搜索动作
                DataTable dt = Get_LeaveList(text);
                if (dt.Rows.Count == 1)
                {
                    ShowNotify(String.Format("检索完成！"));
                    UIHelper.Grid("gridLeaveList_Leave").DataSource(dt, fields);
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

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TwinTriggerBox3_Trigger1Click_Leave(JArray fields)
        {
            Session["AuditState"] = "leave";

            // 点击 TwinTriggerBox 的取消按钮
            var TwinTriggerBox3 = UIHelper.TwinTriggerBox("TwinTriggerBox3");


            ShowNotify("取消搜索！");

            // 执行清空动作
            TwinTriggerBox3.Text("");
            TwinTriggerBox3.ShowTrigger1(false);

            //初始化Grid数据
            DataTable dtSource = Get_LL_DataTable("total");
            UIHelper.Grid("gridLeaveList_Leave").DataSource(dtSource, fields);

            return UIHelper.Result();
        }

        //搜索框一 学生姓名搜索搜索 销假
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TwinTriggerBox1_Trigger2Click_Back(string text, JArray fields)
        {
            Session["AuditState"] = "back";

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
                        UIHelper.Grid("gridLeaveList_Back").DataSource(Get_LL_DataTable_BY_ST_Num(ST_NUM, staticLeaveType), fields);
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
            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TwinTriggerBox1_Trigger1Click_Back(JArray fields)
        {
            Session["AuditState"] = "back";

            // 点击 TwinTriggerBox 的取消按钮
            var TwinTriggerBox1 = UIHelper.TwinTriggerBox("TwinTriggerBox1");

            ShowNotify("取消搜索！");

            // 执行清空动作
            TwinTriggerBox1.Text("");
            TwinTriggerBox1.ShowTrigger1(false);

            //初始化Grid数据
            DataTable dtSource = Get_LL_DataTable("total");
            UIHelper.Grid("gridLeaveList_Back").DataSource(dtSource, fields);

            return UIHelper.Result();
        }

        //搜索框二 学号搜索 销假
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TwinTriggerBox2_Trigger2Click_Back(string text, JArray fields)
        {
            Session["AuditState"] = "back";

            // 点击 TwinTriggerBox 的搜索按钮
            var TwinTriggerBox2 = UIHelper.TwinTriggerBox("TwinTriggerBox2");

            if (!String.IsNullOrEmpty(text))
            {
                // 执行搜索动作
                var ST_Info_List = from vw_Student in db.vw_Student where (vw_Student.ST_Num == text) select vw_Student;
                if (ST_Info_List.Any())
                {
                    ShowNotify(String.Format("检索完成！"));
                    UIHelper.Grid("gridLeaveList_Back").DataSource(Get_LL_DataTable_BY_ST_Num(text, staticLeaveType), fields);
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

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TwinTriggerBox2_Trigger1Click_Back(JArray fields)
        {
            Session["AuditState"] = "back";

            // 点击 TwinTriggerBox 的取消按钮
            var TwinTriggerBox2 = UIHelper.TwinTriggerBox("TwinTriggerBox2");


            ShowNotify("取消搜索！");

            // 执行清空动作
            TwinTriggerBox2.Text("");
            TwinTriggerBox2.ShowTrigger1(false);

            //初始化Grid数据
            DataTable dtSource = Get_LL_DataTable("total");
            UIHelper.Grid("gridLeaveList_Back").DataSource(dtSource, fields);

            return UIHelper.Result();
        }

        //搜索框三 请假单号搜索 销假
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TwinTriggerBox3_Trigger2Click_Back(string text, JArray fields)
        {
            Session["AuditState"] = "back";

            // 点击 TwinTriggerBox 的搜索按钮
            var TwinTriggerBox3 = UIHelper.TwinTriggerBox("TwinTriggerBox3");

            if (!String.IsNullOrEmpty(text))
            {
                // 执行搜索动作
                DataTable dt = Get_LeaveList(text);
                if (dt.Rows.Count == 1)
                {
                    ShowNotify(String.Format("检索完成！"));
                    UIHelper.Grid("gridLeaveList_Back").DataSource(dt, fields);
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

            return UIHelper.Result();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TwinTriggerBox3_Trigger1Click_Back(JArray fields)
        {
            Session["AuditState"] = "back";

            // 点击 TwinTriggerBox 的取消按钮
            var TwinTriggerBox3 = UIHelper.TwinTriggerBox("TwinTriggerBox3");


            ShowNotify("取消搜索！");

            // 执行清空动作
            TwinTriggerBox3.Text("");
            TwinTriggerBox3.ShowTrigger1(false);

            //初始化Grid数据
            DataTable dtSource = Get_LL_DataTable("total");
            UIHelper.Grid("gridLeaveList_Back").DataSource(dtSource, fields);

            return UIHelper.Result();
        }
        #endregion

        #region 按班级查找  尚未完成
        /// <summary>
        /// 请假审批界面  班级下拉框事件
        /// </summary>
        /// <param name="ddlST_Class_Leave"></param>
        /// <param name="ddlST_Class_Leave_text"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ddlST_Class_Leave_SelectedIndexChanged(string ddlST_Class_Leave, string ddlST_Class_Leave_text, JArray fields)
        {
            Session["AuditState"] = "leave";

            string type = Session["AuditLeaveType"].ToString();
            DataTable dtSource = new DataTable();

            if (ddlST_Class_Leave == "-1")//-1代表选取全部班级
            {
                Session["AuditClassName"] = "-1";
                dtSource = Get_LL_DataTable(type);
            }
            else
            {
                Session["AuditClassName"] = ddlST_Class_Leave_text;
                dtSource = Get_LL_DataTable_BY_ClassName(type, ddlST_Class_Leave_text);
            }
            ShowNotify("检索完成！");
            UIHelper.Grid("gridLeaveList_Leave").DataSource(dtSource, fields);
            return UIHelper.Result();
        }

        /// <summary>
        /// 销假审批界面  班级下拉框事件
        /// </summary>
        /// <param name="ddlST_Class_Back"></param>
        /// <param name="ddlST_Class_Back_text"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ddlST_Class_Back_SelectedIndexChanged(string ddlST_Class_Back, string ddlST_Class_Back_text, JArray fields)
        {
            Session["AuditState"] = "back";

            string type = Session["AuditBackType"].ToString();
            DataTable dtSource = new DataTable();

            if (ddlST_Class_Back == "-1")//-1代表选取全部班级
            {
                Session["AuditClassName"] = "-1";
                dtSource = Get_LL_DataTable(type);
            }
            else
            {
                Session["AuditClassName"] = ddlST_Class_Back_text;
                dtSource = Get_LL_DataTable_BY_ClassName(type, ddlST_Class_Back_text);
            }
            ShowNotify("检索完成！");
            UIHelper.Grid("gridLeaveList_Back").DataSource(dtSource, fields);
            return UIHelper.Result();
        }

        /// <summary>
        /// 班级下拉框 数据绑定
        /// </summary>
        public void LoadClassDDL()
        {
            string grade = Session["Grade"].ToString();
            string ST_TeacherID = Session["UserID"].ToString();

            var classList = from T_Class in db.T_Class where (T_Class.Grade == grade && T_Class.TeacherID == ST_TeacherID) select T_Class;
            if (classList.Any())
            {
                DataTable dtSource = new DataTable();
                dtSource.Columns.Add("ClassID");
                dtSource.Columns.Add("ClassName");

                DataRow firstRow = dtSource.NewRow();
                firstRow["ClassID"] = "-1";
                firstRow["ClassName"] = "全部班级";
                dtSource.Rows.Add(firstRow);

                foreach (T_Class item in classList.ToList())
                {
                    DataRow row = dtSource.NewRow();
                    row["ClassID"] = item.ID;
                    row["ClassName"] = item.ClassName;

                    dtSource.Rows.Add(row);
                }
                ViewBag.ddlST_Class = dtSource;
            }
        }
        #endregion

        #endregion

        #region AduitInternshipList
        /// <summary>
        /// 获取实习请假记录  DataTable格式
        /// </summary>
        /// <param name="type">请假类型</param>
        /// <param name="ID">实习请假单号</param>
        /// <returns></returns>
        public DataTable Get_LIL_DataTable(string type, string ID)
        {
            string grade;
            string roleId = Session["RoleID"].ToString();
            if (roleId == "3")
            {
                grade = Session["Grade"].ToString();
            }
            else
            {
                grade = null;
            }
            //此处将list转为DataTable FineUI的Grid绑定时间类型数据时会发生错误，尚未找到原因。
            //解决办法：将list转为DataTable绑定到Grid，并且将DataTable中值类型为DateTime的列转为字符串类型
            DataTable dtSource = new DataTable();
            if (ID == null)
            {
                #region 获取LeaveList、转换为DataTable格式
                if (type == "leave")
                {
                    var internshiplist = from vw_LeaveIntership in db.vw_LeaveIntership where ((vw_LeaveIntership.StateLeave == "0") && (vw_LeaveIntership.StateBack == "0") && (vw_LeaveIntership.ST_Grade == grade)) orderby vw_LeaveIntership.ID descending select vw_LeaveIntership;
                    //List 转换为 DataTable
                    dtSource = internshiplist.ToDataTable(rec => new object[] { internshiplist });
                }
                else if (type == "back")
                {
                    var internshiplist = from vw_LeaveIntership in db.vw_LeaveIntership where ((vw_LeaveIntership.StateLeave == "1") && (vw_LeaveIntership.StateBack == "0") && (vw_LeaveIntership.ST_Grade == grade)) orderby vw_LeaveIntership.ID descending select vw_LeaveIntership;
                    //List 转换为 DataTable
                    dtSource = internshiplist.ToDataTable(rec => new object[] { internshiplist });
                }
                else
                    return null;
                #endregion
            }
            else
            {
                var internshiplist = from vw_LeaveIntership in db.vw_LeaveIntership where ((vw_LeaveIntership.ID == ID)) orderby vw_LeaveIntership.ID descending select vw_LeaveIntership;
                //List 转换为 DataTable
                dtSource = internshiplist.ToDataTable(rec => new object[] { internshiplist });
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

        /// <summary>
        /// 同意请假/销假操作，Click事件，实习请假
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnAgreeClick_Intership(JArray selectedRows, JArray gridLeaveList_fields1, JArray gridLeaveList_fields2, string type)
        {
            foreach (string rowId in selectedRows)
            {
                T_LeaveIntership T_LIL = db.T_LeaveIntership.Find(rowId);
                if (type == "leave")
                {
                    T_LIL.StateLeave = "1";
                    db.SaveChanges();
                    //绑定Grid数据
                    UIHelper.Grid("gridLeaveList_Leave").DataSource(Get_LIL_DataTable("leave", null), gridLeaveList_fields1);
                    UIHelper.Grid("gridLeaveList_Back").DataSource(Get_LIL_DataTable("back", null), gridLeaveList_fields2);
                }
                else if (type == "back")
                {
                    T_LIL.StateBack = "1";
                    db.SaveChanges();
                    //绑定Grid数据
                    UIHelper.Grid("gridLeaveList_Leave").DataSource(Get_LIL_DataTable("leave", null), gridLeaveList_fields1);
                    UIHelper.Grid("gridLeaveList_Back").DataSource(Get_LIL_DataTable("back", null), gridLeaveList_fields2);
                }
            }
            ShowNotify(String.Format("批准操作成功！"));
            return UIHelper.Result();
        }

        /// <summary>
        /// 驳回请假操作，Click事件，实习请假
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnCancelClick_Intership(JArray selectedRows, JArray gridLeaveList_fields1, JArray gridLeaveList_fields2)
        {
            foreach (string rowId in selectedRows)
            {
                T_LeaveIntership T_LIL = db.T_LeaveIntership.Find(rowId);
                T_LIL.StateLeave = "2";
                T_LIL.StateBack = "1";
                db.SaveChanges();
                //绑定Grid数据
                UIHelper.Grid("gridLeaveList_Leave").DataSource(Get_LIL_DataTable("leave", null), gridLeaveList_fields1);
                UIHelper.Grid("gridLeaveList_Back").DataSource(Get_LIL_DataTable("back", null), gridLeaveList_fields2);
            }
            ShowNotify(String.Format("驳回操作成功！"));
            return UIHelper.Result();
        }

        /// <summary>
        /// ajax获取学生请假记录详情
        /// </summary>
        /// <returns></returns>
        public ActionResult ajax_Intership_detail()
        {
            //var res = new JsonResult();
            JavaScriptSerializer jss = new JavaScriptSerializer();

            var selectedRows = Request["selectedRows"];
            string rowID = selectedRows.ToString();
            DataTable dt = Get_LIL_DataTable(null, rowID);
            //将datatable中的第一行(datarow)转化成Dictionary，再转换成json
            Dictionary<string, object> RowDic = new Dictionary<string, object>();
            foreach (DataColumn col in dt.Columns)
            {
                RowDic.Add(col.ColumnName, dt.Rows[0][col]);
            }
            string JsonStr = jss.Serialize(RowDic);
            return Content(JsonStr);
        }

        #endregion

        /// <summary>
        /// 调用短信模块发送短信
        /// </summary>
        /// <param name="sendSMessage"></param>
        /// <returns></returns>
        //public void sendSMessage(string Num, string Name, string Tel, string Type)
        //{
        //    MessageModel model = new MessageModel();
        //    model.LV_Num = Num;
        //    model.ST_Name = Name;
        //    model.ST_Tel = Tel;
        //    model.MessageType = Type;
        //    SendSms.sendSms(model);
        //}

    }
}