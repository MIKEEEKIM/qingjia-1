using System.Collections.Generic;

namespace qingjia_MVC.Models.API.Student
{
    public class StudentInfo
    {
        public string StudentID { get; set; }
        public string ST_Name { get; set; }
        public string ST_Class { get; set; }
        public string ST_Grade { get; set; }
        public string ST_TeacherID { get; set; }
        public string ST_TeacherName { get; set; }
        public string ST_Tel { get; set; }
        public string ST_QQ { get; set; }
        public string ST_Email { get; set; }
        public string ST_Sex { get; set; }
        public string ST_ContactName { get; set; }
        public string ST_ContactTel { get; set; }
        public string ST_Dor { get; set; }

        public List<string> leaveTypes = new List<string>();
        public WeeklyMeetingInfo weeklyMeetingInfo { get; set; }
        public HolidayInfo holidayInfo { get; set; }
    }

    public class WeeklyMeetingInfo
    {
        public string time { get; set; }
        public string deadLine { get; set; }
        public string location { get; set; }
    }

    public class HolidayInfo
    {
        public string startTime { get; set; }
        public string endTime { get; set; }
        public string deadLine { get; set; }
    }
}