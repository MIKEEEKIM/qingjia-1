namespace qingjia_MVC.Models.API.User
{
    public class TeacherMeetingHolidayInfo
    {
        public bool meetingSetted = false;
        public string meetingDate { get; set; }
        public string meetingLocation { get; set; }
        public string meetingTimeSpan { get; set; }
        public string meetingDeadline { get; set; }

        public int meetingPercentage = 100;

        public bool holidaySetted = false;
        public string holidayName { get; set; }
        public string holidayDeadLine { get; set; }
        public string holidayStartDate { get; set; }
        public string holidayEndDate { get; set; }

        public int holidatPercentage = 100;
    }
}