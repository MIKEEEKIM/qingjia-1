using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using qingjia_MVC.Content;
using qingjia_MVC.Controllers;
using qingjia_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace qingjia_MVC.Areas.Vacation.Controllers
{

    /// <summary>
    /// table 数据模型
    /// </summary>
    public class LL_Table
    {
        public string ST_Num { get; set; }
        public string ST_Class { get; set; }
        public string ST_Name { get; set; }
        public string ST_Go { get; set; }
        public string TimeLeave { get; set; }
        public string TimeBack { get; set; }
        public string LeaveWay { get; set; }
        public string BackWay { get; set; }
        public string Address { get; set; }
        public string Contact { get; set; }
        public string Tel { get; set; }
    }

    public class VacationController : BaseController//继承自basecontroller可实现 跳转控制
    {
        //实例化数据库
        Entities db = new Entities();

        // GET: Vacation/Vacation
        public ActionResult Index()
        {
            LoadData();
            return View();
        }

        /// <summary>
        /// Dropdownlist绑定
        /// </summary>
        protected void LoadData()
        {
            //获取用户信息
            string UserID = Session["UserID"].ToString();
            string RoleID = Session["RoleID"].ToString();

            if (RoleID == "3")//辅导员端
            {
                var classNameList = from T_Class in db.T_Class where (T_Class.TeacherID == UserID) select T_Class.ClassName;
                ViewBag.classSource = classNameList;
            }
            else if (RoleID == "2")//班级账号
            {
                //获取班级名称

                var className = from T_Class in db.T_Class where (T_Class.ID == UserID) select T_Class.ClassName;
                ViewBag.classSource = className;
            }
            else
            {

            }
        }

        public ActionResult GetTable()
        {
            //获取用户信息
            string UserID = Session["UserID"].ToString();
            string RoleID = Session["RoleID"].ToString();

            if (RoleID == "3")//辅导员
            {
                //获取Url中的传值
                string startDate = Request["startDate"].ToString();
                string startTime = Request["startTime"].ToString();
                string endDate = Request["endDate"].ToString();
                string endTime = Request["endTime"].ToString();
                string className = Request["className"].ToString();

                //从数据库中读取请假数据
                DateTime start = Convert.ToDateTime(startDate + " " + startTime);
                DateTime end = Convert.ToDateTime(endDate + " " + endTime);

                //table标题
                string startstring = startDate.Substring(0, 4) + "年" + startDate.Substring(5, 2) + "月" + startDate.Substring(8, 2) + "日";
                string endstring = endDate.Substring(0, 4) + "年" + endDate.Substring(5, 2) + "月" + endDate.Substring(8, 2) + "日";
                ViewBag.tableTitle = className + "班 " + startstring + "--" + endstring + "离校去向统计表";

                return PartialView("_tablePartial", Search_LL(start, end, UserID, RoleID, className));
            }
            else if (RoleID == "2")//班级
            {
                //获取Url中的传值
                string startDate = Request["startDate"].ToString();
                string startTime = Request["startTime"].ToString();
                string endDate = Request["endDate"].ToString();
                string endTime = Request["endTime"].ToString();

                //获取班级名称
                string className = (string)(from T_Class in db.T_Class where (T_Class.ID == UserID) select T_Class.ClassName).First();

                //从数据库中读取请假数据
                DateTime start = Convert.ToDateTime(startDate + " " + startTime);
                DateTime end = Convert.ToDateTime(endDate + " " + endTime);

                //table标题
                string startstring = startDate.Substring(0, 4) + "年" + startDate.Substring(5, 2) + "月" + startDate.Substring(8, 2) + "日";
                string endstring = endDate.Substring(0, 4) + "年" + endDate.Substring(5, 2) + "月" + endDate.Substring(8, 2) + "日";
                ViewBag.tableTitle = className + "班 " + startstring + "--" + endstring + "离校去向统计表";

                return PartialView("_tablePartial", Search_LL(start, end, UserID, RoleID, className));
            }
            else
            {
                //未知错误
                return null;
            }
        }

        /// <summary>
        /// 从数据库中查询数据
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="UserID"></param>
        /// <param name="RoleID"></param>
        /// <param name="className"></param>
        protected List<LL_Table> Search_LL(DateTime start, DateTime end, string UserID, string RoleID, string className)
        {
            DataTable dtStudent = new DataTable();
            DataTable dtLeaveList = new DataTable();
            List<LL_Table> tableLeaveList = new List<LL_Table>();

            //获取班级同学名单
            var studentList = from vw_Student in db.vw_Student where (vw_Student.ST_Class == className) select vw_Student;
            dtStudent = studentList.ToDataTable(rec => new object[] { studentList });
            //获取班级同学请假记录 StateLeave == 1 选取通过的请假条、驳回的请假不能导出
            var LeaveList = from LL in db.vw_LeaveList where ((LL.ST_Class == className) && (LL.TypeChildID == 6) && (LL.StateLeave == "1") && ((LL.TimeLeave <= start && LL.TimeBack > start && LL.TimeBack <= end) || (LL.TimeLeave <= start && LL.TimeBack >= end) || (LL.TimeLeave >= start && LL.TimeBack <= end) || (LL.TimeLeave >= start && LL.TimeLeave < end && LL.TimeBack >= end))) select LL;
            dtLeaveList = LeaveList.ToDataTable(rec => new object[] { LeaveList });

            DataTable dtSource = new DataTable();
            DataColumn col_ST_Num = new DataColumn("ST_Num", typeof(string));
            dtSource.Columns.Add(col_ST_Num);
            DataColumn col_ST_Class = new DataColumn("ST_Class", typeof(string));
            dtSource.Columns.Add(col_ST_Class);
            DataColumn col_ST_Name = new DataColumn("ST_Name", typeof(string));
            dtSource.Columns.Add(col_ST_Name);
            DataColumn col_ST_Go = new DataColumn("ST_Go", typeof(string));
            dtSource.Columns.Add(col_ST_Go);
            DataColumn col_TimeLeave = new DataColumn("TimeLeave", typeof(string));
            dtSource.Columns.Add(col_TimeLeave);
            DataColumn col_TimeBack = new DataColumn("TimeBack", typeof(string));
            dtSource.Columns.Add(col_TimeBack);
            DataColumn col_LeaveWay = new DataColumn("LeaveWay", typeof(string));
            dtSource.Columns.Add(col_LeaveWay);
            DataColumn col_BackWay = new DataColumn("BackWay", typeof(string));
            dtSource.Columns.Add(col_BackWay);
            DataColumn col_Address = new DataColumn("Address", typeof(string));
            dtSource.Columns.Add(col_Address);
            DataColumn col_Contact = new DataColumn("Contact", typeof(string));
            dtSource.Columns.Add(col_Contact);
            DataColumn col_Tel = new DataColumn("Tel", typeof(string));
            dtSource.Columns.Add(col_Tel);

            foreach (DataRow rowStudent in dtStudent.Rows)
            {
                //标记是否包含离校请假记录
                bool flag = false;

                DataRow newRow = dtSource.NewRow();
                newRow["ST_Num"] = rowStudent["ST_Num"].ToString();
                newRow["ST_Class"] = rowStudent["ST_Class"].ToString();
                newRow["ST_Name"] = rowStudent["ST_Name"].ToString();
                newRow["Contact"] = rowStudent["ContactOne"].ToString();
                newRow["Tel"] = rowStudent["OneTel"].ToString();
                foreach (DataRow rowLL in dtLeaveList.Rows)
                {
                    if (rowLL["StudentID"].ToString() == rowStudent["ST_Num"].ToString())
                    {
                        flag = true;
                        newRow["ST_Go"] = rowLL["Reason"].ToString();
                        newRow["TimeLeave"] = rowLL["TimeLeave"].ToString();
                        newRow["TimeBack"] = rowLL["TimeBack"].ToString();
                        newRow["LeaveWay"] = rowLL["LeaveWay"].ToString();
                        newRow["BackWay"] = rowLL["BackWay"].ToString();
                        newRow["Address"] = rowLL["Address"].ToString();
                    }
                }
                if (!flag)//无请假记录
                {
                    newRow["ST_Go"] = "留校";
                    newRow["TimeLeave"] = "";
                    newRow["TimeBack"] = "";
                    newRow["LeaveWay"] = "";
                    newRow["BackWay"] = "";
                    newRow["Address"] = "";
                }

                dtSource.Rows.Add(newRow);
            }

            //添加到List中  tableLeaveList
            foreach (DataRow row in dtSource.Rows)
            {
                LL_Table item = new LL_Table();
                item.ST_Num = row["ST_Num"].ToString();
                item.ST_Class = row["ST_Class"].ToString();
                item.ST_Name = row["ST_Name"].ToString();
                item.ST_Go = row["ST_Go"].ToString();
                item.TimeLeave = row["TimeLeave"].ToString();
                item.TimeBack = row["TimeBack"].ToString();
                item.LeaveWay = row["LeaveWay"].ToString();
                item.BackWay = row["BackWay"].ToString();
                item.Address = row["Address"].ToString();
                item.Contact = row["Contact"].ToString();
                item.Tel = row["Tel"].ToString();
                //添加到List中
                tableLeaveList.Add(item);
            }
            return tableLeaveList;
        }

        public ActionResult DownLoad()
        {
            //获取用户信息
            string UserID = Session["UserID"].ToString();
            string RoleID = Session["RoleID"].ToString();

            if (RoleID == "3")//辅导员
            {
                //获取Url中的传值
                string startDate = Request["startDate"].ToString();
                string startTime = Request["startTime"].ToString();
                string endDate = Request["endDate"].ToString();
                string endTime = Request["endTime"].ToString();
                string className = Request["className"].ToString();

                //从数据库中读取请假数据
                DateTime start = Convert.ToDateTime(startDate + " " + startTime);
                DateTime end = Convert.ToDateTime(endDate + " " + endTime);

                //导出Excel标题
                string startstring = startDate.Substring(0, 4) + "年" + startDate.Substring(5, 2) + "月" + startDate.Substring(8, 2) + "日";
                string endstring = endDate.Substring(0, 4) + "年" + endDate.Substring(5, 2) + "月" + endDate.Substring(8, 2) + "日";
                string Title = className + "班 " + startstring + "--" + endstring + "离校去向统计表";

                return ToExcel(Search_LL(start, end, UserID, RoleID, className), className, Title);
            }
            else if (RoleID == "2")//班级
            {
                //获取Url中的传值
                string startDate = Request["startDate"].ToString();
                string startTime = Request["startTime"].ToString();
                string endDate = Request["endDate"].ToString();
                string endTime = Request["endTime"].ToString();

                //获取班级名称
                string className = (string)(from T_Class in db.T_Class where (T_Class.ID == UserID) select T_Class.ClassName).First();

                //从数据库中读取请假数据
                DateTime start = Convert.ToDateTime(startDate + " " + startTime);
                DateTime end = Convert.ToDateTime(endDate + " " + endTime);

                //导出Excel标题
                string startstring = startDate.Substring(0, 4) + "年" + startDate.Substring(5, 2) + "月" + startDate.Substring(8, 2) + "日";
                string endstring = endDate.Substring(0, 4) + "年" + endDate.Substring(5, 2) + "月" + endDate.Substring(8, 2) + "日";
                string Title = className + "班 " + startstring + "--" + endstring + "离校去向统计表";

                return ToExcel(Search_LL(start, end, UserID, RoleID, className), className, Title);
            }
            else
            {
                //未知错误
                return null;
            }
        }

        protected ActionResult ToExcel(List<LL_Table> table_LL, string className, string title)
        {
            //文件名称   必须包含 .xls
            string fileName = "节假日去向表--" + className + ".xls";

            //创建工作簿、工作表
            HSSFWorkbook newExcel = new HSSFWorkbook();
            HSSFSheet sheet = (HSSFSheet)newExcel.CreateSheet("离校去向表");
            setSheet(newExcel, sheet, table_LL, title);

            //输出
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            newExcel.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return File(ms, "application/vnd.ms-excel", fileName);
        }

        public void setSheet(HSSFWorkbook newExcel, HSSFSheet sheet, List<LL_Table> dt, string title)
        {
            #region 设置行宽，列高
            sheet.SetColumnWidth(0, 30 * 256);
            sheet.SetColumnWidth(1, 30 * 256);
            sheet.SetColumnWidth(2, 30 * 256);
            sheet.SetColumnWidth(3, 30 * 256);
            sheet.SetColumnWidth(4, 30 * 256);
            sheet.SetColumnWidth(5, 30 * 256);
            sheet.SetColumnWidth(6, 30 * 256);
            sheet.SetColumnWidth(7, 30 * 256);
            sheet.SetColumnWidth(8, 30 * 256);
            sheet.SetColumnWidth(9, 30 * 256);
            sheet.SetColumnWidth(10, 30 * 256);
            sheet.SetColumnWidth(11, 30 * 256);
            sheet.SetColumnWidth(12, 30 * 256);
            sheet.DefaultRowHeight = 15 * 20;
            #endregion

            #region 设置字体
            HSSFFont font_title = (HSSFFont)newExcel.CreateFont();
            font_title.FontHeightInPoints = 10;

            HSSFFont font_name = (HSSFFont)newExcel.CreateFont();
            font_name.FontHeightInPoints = 7;
            font_name.IsBold = true;

            HSSFFont font_data = (HSSFFont)newExcel.CreateFont();
            font_data.FontHeightInPoints = 7;
            #endregion

            #region 设置样式
            //1、标题的样式
            HSSFCellStyle style_title = (HSSFCellStyle)newExcel.CreateCellStyle();
            style_title.Alignment = HorizontalAlignment.Center;
            style_title.VerticalAlignment = VerticalAlignment.Center;
            style_title.SetFont(font_title);

            //2、字段名的样式
            HSSFCellStyle style_name = (HSSFCellStyle)newExcel.CreateCellStyle();
            style_name.Alignment = HorizontalAlignment.Center;
            style_name.VerticalAlignment = VerticalAlignment.Center;
            style_name.SetFont(font_name);
            style_name.BorderTop = BorderStyle.Thin;
            style_name.BorderBottom = BorderStyle.Thin;
            style_name.BorderLeft = BorderStyle.Thin;
            style_name.BorderRight = BorderStyle.Thin;

            //3、批次的样式
            HSSFCellStyle style_batch = (HSSFCellStyle)newExcel.CreateCellStyle();
            style_batch.Alignment = HorizontalAlignment.Center;
            style_batch.VerticalAlignment = VerticalAlignment.Center;
            style_batch.FillPattern = FillPattern.SolidForeground;
            style_batch.FillForegroundColor = HSSFColor.Grey40Percent.Index;
            style_batch.SetFont(font_data);
            style_batch.BorderTop = BorderStyle.Thin;
            style_batch.BorderBottom = BorderStyle.Thin;
            style_batch.BorderLeft = BorderStyle.Thin;
            style_batch.BorderRight = BorderStyle.Thin;

            //4、数据的样式
            HSSFCellStyle style_data = (HSSFCellStyle)newExcel.CreateCellStyle();
            style_data.Alignment = HorizontalAlignment.Center;
            style_data.VerticalAlignment = VerticalAlignment.Center;
            style_data.SetFont(font_data);
            style_data.BorderTop = BorderStyle.Thin;
            style_data.BorderBottom = BorderStyle.Thin;
            style_data.BorderLeft = BorderStyle.Thin;
            style_data.BorderRight = BorderStyle.Thin;
            #endregion

            #region 设置内容
            //第一行 标题
            HSSFRow row_title = (HSSFRow)sheet.CreateRow(0);
            HSSFCell cell_title = (HSSFCell)row_title.CreateCell(0);
            cell_title.SetCellValue(title);
            cell_title.CellStyle = style_title;
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 12));   //合并单元格(起始行，结束行，起始列，结束列)

            //第二行 字段名 
            HSSFRow row_name = (HSSFRow)sheet.CreateRow(1);

            HSSFCell cell_name_1 = (HSSFCell)row_name.CreateCell(0);
            cell_name_1.SetCellValue("序号");
            cell_name_1.CellStyle = style_name;

            HSSFCell cell_name_2 = (HSSFCell)row_name.CreateCell(1);
            cell_name_2.SetCellValue("学号");
            cell_name_2.CellStyle = style_name;

            HSSFCell cell_name_3 = (HSSFCell)row_name.CreateCell(2);
            cell_name_3.SetCellValue("班级");
            cell_name_3.CellStyle = style_name;

            HSSFCell cell_name_4 = (HSSFCell)row_name.CreateCell(3);
            cell_name_4.SetCellValue("姓名");
            cell_name_4.CellStyle = style_name;

            HSSFCell cell_name_5 = (HSSFCell)row_name.CreateCell(4);
            cell_name_5.SetCellValue("节假日去向");
            cell_name_5.CellStyle = style_name;

            HSSFCell cell_name_6 = (HSSFCell)row_name.CreateCell(5);
            cell_name_6.SetCellValue("离校时间");
            cell_name_6.CellStyle = style_name;

            HSSFCell cell_name_7 = (HSSFCell)row_name.CreateCell(6);
            cell_name_7.SetCellValue("返校时间");
            cell_name_7.CellStyle = style_name;

            HSSFCell cell_name_8 = (HSSFCell)row_name.CreateCell(7);
            cell_name_8.SetCellValue("离校方式");
            cell_name_8.CellStyle = style_name;

            HSSFCell cell_name_9 = (HSSFCell)row_name.CreateCell(8);
            cell_name_9.SetCellValue("返校方式");
            cell_name_9.CellStyle = style_name;

            HSSFCell cell_name_10 = (HSSFCell)row_name.CreateCell(9);
            cell_name_10.SetCellValue("离校去向地址");
            cell_name_10.CellStyle = style_name;

            HSSFCell cell_name_11 = (HSSFCell)row_name.CreateCell(10);
            cell_name_11.SetCellValue("联系人");
            cell_name_11.CellStyle = style_name;

            HSSFCell cell_name_12 = (HSSFCell)row_name.CreateCell(11);
            cell_name_12.SetCellValue("联系方式");
            cell_name_12.CellStyle = style_name;

            HSSFCell cell_name_13 = (HSSFCell)row_name.CreateCell(12);
            cell_name_13.SetCellValue("签名确认");
            cell_name_13.CellStyle = style_name;

            //数据
            int n = 2;
            int i = 1;
            foreach (LL_Table item in dt)
            {
                HSSFRow row = (HSSFRow)sheet.CreateRow(n++);//写入行  
                row.CreateCell(0).SetCellValue(i++);
                row.CreateCell(1).SetCellValue(item.ST_Num);
                row.CreateCell(2).SetCellValue(item.ST_Class);
                row.CreateCell(3).SetCellValue(item.ST_Name);
                row.CreateCell(4).SetCellValue(item.ST_Go);
                row.CreateCell(5).SetCellValue(item.TimeLeave);
                row.CreateCell(6).SetCellValue(item.TimeBack);
                row.CreateCell(7).SetCellValue(item.LeaveWay);
                row.CreateCell(8).SetCellValue(item.BackWay);
                row.CreateCell(9).SetCellValue(item.Address);
                row.CreateCell(10).SetCellValue(item.Contact);
                row.CreateCell(11).SetCellValue(item.Tel);
                row.CreateCell(12).SetCellValue("");
                foreach (ICell cell in row)
                {
                    if (cell.ColumnIndex == 0)
                    {
                        cell.CellStyle = style_batch;
                    }
                    else
                    {
                        cell.CellStyle = style_data;
                    }
                }
            }
            #endregion
        }
    }
}