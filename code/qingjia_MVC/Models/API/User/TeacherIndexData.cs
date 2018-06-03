using System.Collections.Generic;

namespace qingjia_MVC.Models.API.User
{
    public class TeacherIndexData
    {
        public TeacherLeaveListPending teacherLeaveListPendingData { get; set; }

        public TeacherStudentLeave teacherStudentLeaveData { get; set; }

        public List<T_LoginInfo> systemloginData { get; set; }

        public TeacherMeetingHolidayInfo teacherMeetingData { get; set; }

        public TeacherMeetingHolidayInfo teacherHolidayData { get; set; }
    }
}