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
            Lesson = _LL.Lesson;
            LeaveTypeID = _LL.LeaveType.ToString();
            LeaveTypeName = _LL.LeaveTypeName;
            LeaveTypeChildrenID = _LL.LeaveTypeChildrenID.ToString();
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
            ID = _LL.ID;
            ID = _LL.ID;
            ID = _LL.ID;
            ID = _LL.ID;
            ID = _LL.ID;
            ID = _LL.ID;
            ID = _LL.ID;
            ID = _LL.ID;
        }
    }
}