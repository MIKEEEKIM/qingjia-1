using FineUIMvc;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using qingjia_MVC.Common;
using qingjia_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace qingjia_MVC.Areas.CallName.Controllers
{
    public class NameForTController : Controller
    {
        private Entities db = new Entities();

        // GET: CallName/NameList
        public ActionResult Index()
        {
            string teacherid = Session["UserID"].ToString();
            ViewBag.ddlClass_DataSource = PublicMethod.getClassList(teacherid);
           
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnSearch_Click(FormCollection form,string ddlClass_text)
        {
            string classID = form["ddlClass"].ToString();
            string teacherid = Session["UserID"].ToString();
            DateTime time = (from b in db.vw_ClassBatch
                             where b.TeacherID == teacherid && b.ClassName == ddlClass_text
                             select b.Datetime)
                          .First();

            var stu_list = (from l in db.vw_LeaveList
                            where l.LeaveType.StartsWith("晚点名请假") && l.StateLeave == "1" && l.StateBack == "0" && l.ST_Class == ddlClass_text && l.TimeLeave == time
                            select l.ST_Name)
                           .Distinct();

            StringBuilder stu_names = new StringBuilder();
            if (stu_list.Count() > 0)
            {
                
                foreach (var item in stu_list)
                {
                    stu_names.Append(item.ToString());
                    stu_names.Append("；");
                }
            }
            else
            {
                stu_names.Append("无");
            }
            UIHelper.TextArea("taName").Text(stu_names.ToString());

            return UIHelper.Result();
        }

        
        public ActionResult setNightNameList()
        {
            string teacherid = Session["UserID"].ToString();
            string time = (from b in db.vw_ClassBatch
                             where b.TeacherID == teacherid
                             select b.Datetime)
                          .First().ToString();
            SimpleDateFormat formart = new SimpleDateFormat("yyyy-MM-dd hh:mm:ss");
            DateTime date = formart.Parse(time);

            //db.sp_getNightNameList(date, teacherid);
            //此处存疑，2017.9.3日重新生成model时，此行报错，提示存储过程找不到两个参数的重载，故删去date。但再重新生成model之前时不报错的，所以可能还会有bug
            db.sp_getNightNameList(teacherid);
            return getExcel(date);
        }
        
        public ActionResult getExcel(DateTime date)
        {
            string teacherid = Session["UserID"].ToString();
            string teacher_name = db.T_Teacher.Find(teacherid).Name;
            #region 设置工作表标题
            int year = date.Year;
            int month = date.Month;
            int day = date.Day;
            string[] batch = { "一", "二", "三" };
            string teacher = db.T_Teacher.Where(t => t.ID == teacherid).Select(t=>t.Name).First().ToString();
            string[] classes = new string[3];
            string[] titles = new string[3];
            for (int i = 0; i < 3; i++)
            {
                var list = from cb in db.vw_ClassBatch
                           where cb.TeacherID == teacherid && cb.Batch == i + 1
                           select cb.ClassName;
                StringBuilder str_class = new StringBuilder();
                foreach (var item in list)
                {
                    str_class.Append(item.ToString());
                    str_class.Append(" ");
                }
                classes[i] = str_class.ToString();
            }
            for (int j = 0; j < 3; j++)
            {
                titles[j] = String.Format("{0}年{1}月{2}日第{3}批晚点名名单--辅导员：{4}({5})", year, month, day, batch[j], teacher, classes[j]);
            }
            #endregion
            
            List<vw_NightNameList> dt1 = (from n in db.vw_NightNameList
                                          where n.ST_Teacher == teacher_name && n.Batch == 1
                                          select n)
                                         .ToList();
            List<vw_NightNameList> dt2 = (from n in db.vw_NightNameList
                                          where n.ST_Teacher == teacher_name && n.Batch == 2
                                          select n)
                                         .ToList();
            List<vw_NightNameList> dt3 = (from n in db.vw_NightNameList
                                          where n.ST_Teacher == teacher_name && n.Batch == 3
                                          select n)
                                         .ToList();
            

            //文件名称
            DateTime time = DateTime.Now;
            string strExcelName = time.ToString("yyyyMMddhhmmss");
            strExcelName = "晚点名名单" + strExcelName + ".xls";
            
            //创建工作簿、工作表
            HSSFWorkbook newExcel = new HSSFWorkbook();
            if (dt1.Count > 0)
            {
                HSSFSheet sheet1 = (HSSFSheet)newExcel.CreateSheet("第一批");
                setSheet(newExcel, sheet1, dt1, titles[0]);
            }
            if (dt2.Count > 0)
            {
                HSSFSheet sheet2 = (HSSFSheet)newExcel.CreateSheet("第二批");
                setSheet(newExcel, sheet2, dt2, titles[1]);
            }
            if (dt3.Count > 0)
            {
                HSSFSheet sheet3 = (HSSFSheet)newExcel.CreateSheet("第三批");
                setSheet(newExcel, sheet3, dt3, titles[2]);
            }
            // 写入到客户端
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            newExcel.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return File(ms, "application/vnd.ms-excel", strExcelName);
        }
        
        public void setSheet(HSSFWorkbook newExcel,HSSFSheet sheet, List<vw_NightNameList> dt, string title)
        {
            #region 设置行宽，列高
            sheet.SetColumnWidth(0, 15 * 256);
            sheet.SetColumnWidth(1, 30 * 256);
            sheet.SetColumnWidth(2, 30 * 256);
            sheet.SetColumnWidth(3, 15 * 256);
            sheet.SetColumnWidth(4, 15 * 256);
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
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 4));   //合并单元格(起始行，结束行，起始列，结束列)

            //第二行 字段名 
            HSSFRow row_name = (HSSFRow)sheet.CreateRow(1);

            HSSFCell cell_name_1 = (HSSFCell)row_name.CreateCell(0);
            cell_name_1.SetCellValue("批次");
            cell_name_1.CellStyle = style_name;

            HSSFCell cell_name_2 = (HSSFCell)row_name.CreateCell(1);
            cell_name_2.SetCellValue("班级");
            cell_name_2.CellStyle = style_name;

            HSSFCell cell_name_3 = (HSSFCell)row_name.CreateCell(2);
            cell_name_3.SetCellValue("学号");
            cell_name_3.CellStyle = style_name;

            HSSFCell cell_name_4 = (HSSFCell)row_name.CreateCell(3);
            cell_name_4.SetCellValue("姓名");
            cell_name_4.CellStyle = style_name;

            HSSFCell cell_name_5 = (HSSFCell)row_name.CreateCell(4);
            cell_name_5.SetCellValue("状态");
            cell_name_5.CellStyle = style_name;

            //数据
            int i = 2;
            foreach (vw_NightNameList item in dt)
            {
                HSSFRow row = (HSSFRow)sheet.CreateRow(i++);//写入行  
                row.CreateCell(0).SetCellValue(item.Batch);
                row.CreateCell(1).SetCellValue(item.ClassName);
                row.CreateCell(2).SetCellValue(item.ST_Num);
                row.CreateCell(3).SetCellValue(item.ST_Name);
                row.CreateCell(4).SetCellValue(item.State);
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