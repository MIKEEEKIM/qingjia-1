using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using qingjia_MVC.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace qingjia_MVC.Areas.CallName.Controllers
{
    public class NameForCController : Controller
    {
        private Entities db = new Entities();

        // GET: CallName/NameForC
        public ActionResult Index()
        {
            LoadData();
            return View();
        }

        public void LoadData()
        {
            string classname = db.T_Student.Find(Session["UserID"].ToString()).ClassName.ToString();
            DateTime time = (from b in db.vw_ClassBatch
                             where b.ClassName == classname
                             select b.Datetime)
                          .First();

            var stu_list = (from l in db.vw_LeaveList
                            where l.LeaveType.StartsWith("晚点名请假") && l.StateLeave == "1" && l.StateBack == "0" && l.ST_Class == classname && l.TimeLeave == time
                            select l.ST_Name
                           ).Distinct();

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
            ViewBag.taName = stu_names.ToString();
        }

        
        public ActionResult setNightNameList()
        {
            string classname = db.T_Student.Find(Session["UserID"].ToString()).ClassName.ToString();
            string teacherid = "1214001";
            string time = (from b in db.vw_ClassBatch
                           where b.ClassName == classname
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
            #region 设置工作表标题
            int year = date.Year;
            int month = date.Month;
            int day = date.Day;
            string classname = Session["ClassName"].ToString();
            string teacher = db.T_Teacher.Where(t => t.ID == teacherid).Select(t => t.Name).First().ToString();
            string title = String.Format("{0}年{1}月{2}日晚点名名单--辅导员：{3}({4})", year, month, day, teacher, classname);
            #endregion

            List<vw_NightNameList> dt = (from n in db.vw_NightNameList
                                         join t in db.T_Teacher
                                         on n.ST_Teacher equals t.Name
                                         where t.ID == teacherid && n.ClassName == classname
                                         select n)
                                         .ToList();

            //文件名称
            DateTime time = DateTime.Now;
            string strExcelName = time.ToString("yyyyMMddhhmmss");
            strExcelName = "晚点名名单" + strExcelName + ".xls";

            //创建工作簿、工作表
            HSSFWorkbook newExcel = new HSSFWorkbook();
            if (dt.Count > 0)
            {
                HSSFSheet sheet1 = (HSSFSheet)newExcel.CreateSheet("第一批");
                setSheet(newExcel, sheet1, dt, title);
            }

            // 写入到客户端
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            newExcel.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return File(ms, "application/vnd.ms-excel", strExcelName);
        }

        public void setSheet(HSSFWorkbook newExcel, HSSFSheet sheet, List<vw_NightNameList> dt, string title)
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
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 3));   //合并单元格(起始行，结束行，起始列，结束列)

            //第二行 字段名 
            HSSFRow row_name = (HSSFRow)sheet.CreateRow(1);

            HSSFCell cell_name_1 = (HSSFCell)row_name.CreateCell(0);
            cell_name_1.SetCellValue("批次");
            cell_name_1.CellStyle = style_name;
            /*
            HSSFCell cell_name_2 = (HSSFCell)row_name.CreateCell(1);
            cell_name_2.SetCellValue("班级");
            cell_name_2.CellStyle = style_name;
            */
            HSSFCell cell_name_2 = (HSSFCell)row_name.CreateCell(1);
            cell_name_2.SetCellValue("学号");
            cell_name_2.CellStyle = style_name;

            HSSFCell cell_name_3 = (HSSFCell)row_name.CreateCell(2);
            cell_name_3.SetCellValue("姓名");
            cell_name_3.CellStyle = style_name;

            HSSFCell cell_name_4 = (HSSFCell)row_name.CreateCell(3);
            cell_name_4.SetCellValue("状态");
            cell_name_4.CellStyle = style_name;

            //数据
            int i = 2;
            foreach (vw_NightNameList item in dt)
            {
                HSSFRow row = (HSSFRow)sheet.CreateRow(i++);//写入行  
                row.CreateCell(0).SetCellValue(item.Batch);
                //row.CreateCell(1).SetCellValue(item.ClassName);
                row.CreateCell(1).SetCellValue(item.ST_Num);
                row.CreateCell(2).SetCellValue(item.ST_Name);
                row.CreateCell(3).SetCellValue(item.State);
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