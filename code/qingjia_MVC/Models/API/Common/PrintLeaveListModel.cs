using System;

namespace qingjia_MVC.Models.API.Common
{
    public class PrintLeaveListModel
    {
        public string ID { get; set; }
        public string Lesson { get; set; }
        public string LeaveTypeID { get; set; }
        public string LeaveTypeName { get; set; }
        public string LeaveTypeChildrenID { get; set; }
        public string LeaveTypeChildrenName { get; set; }
        public string TeacherName { get; set; }
        public DateTime LeaveTime { get; set; }
        public DateTime BackTime { get; set; }
        public string ST_Class { get; set; }
        public string ST_Name { get; set; }
        public string StudentID { get; set; }
        public string ST_Tel { get; set; }
        public string ST_TeacherName { get; set; }
        public string Reason { get; set; }
        public string ST_Dor { get; set; }
        public string ST_ContactName { get; set; }
        public string ST_ContactTel { get; set; }

        public PrintLeaveListModel()
        {

        }

        public PrintLeaveListModel(vw_New_LeaveList _LL)
        {
            ID = _LL.ID;
            Lesson = "Error";
            if (_LL.Lesson == "1")
            {
                Lesson = "第一大节";
            }
            if (_LL.Lesson == "2")
            {
                Lesson = "第二大节";
            }
            if (_LL.Lesson == "3")
            {
                Lesson = "第三大节";
            }
            if (_LL.Lesson == "4")
            {
                Lesson = "第四大节";
            }
            if (_LL.Lesson == "5")
            {
                Lesson = "第五大节";
            }
            LeaveTypeID = _LL.LeaveType.ToString().Trim();
            LeaveTypeName = _LL.LeaveTypeName;
            LeaveTypeChildrenID = _LL.LeaveTypeChildrenID.ToString().Trim();
            LeaveTypeChildrenName = "";
            if (LeaveTypeChildrenID == "1")
            {
                LeaveTypeChildrenName = "公假";
            }
            if (LeaveTypeChildrenID == "2")
            {
                LeaveTypeChildrenName = "事假";
            }
            if (LeaveTypeChildrenID == "3")
            {
                LeaveTypeChildrenName = "病假";
            }
            TeacherName = _LL.Teacher;
            LeaveTime = (DateTime)_LL.LeaveTime;
            BackTime = (DateTime)_LL.BackTime;
            ST_Class = _LL.ST_Class;
            ST_Name = _LL.ST_Name;
            StudentID = _LL.StudentID;
            ST_Tel = _LL.ST_Tel;
            ST_TeacherName = (_LL.ST_Teacher == null) ? "" : _LL.ST_Teacher.ToString().Trim();
            Reason = (_LL.Reason == null) ? "" : _LL.Reason.ToString().Trim();
            ST_Dor = (_LL.ST_Dor == null) ? "" : _LL.ST_Dor.ToString().Trim();
            ST_ContactName = (_LL.ST_ContactOne == null) ? "" : _LL.ST_ContactOne.ToString().Trim();
            ST_ContactTel = (_LL.ST_OneTel == null) ? "" : _LL.ST_OneTel.ToString().Trim();
        }
    }
}