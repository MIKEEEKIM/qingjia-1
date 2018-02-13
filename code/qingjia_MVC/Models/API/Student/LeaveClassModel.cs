namespace qingjia_MVC.Models.API.Student
{
    public class LeaveClassModel
    {
        public string access_token { get; set; }
        public string leaveTypeChildrenID { get; set; }
        public string leave_date { get; set; }
        public string leave_reason { get; set; }
        public string teacher_name { get; set; }
        public string lesson { get; set; }

        public bool Check()
        {
            if (this.access_token == null || this.leaveTypeChildrenID == null || this.leave_date == null || this.leave_reason == null || this.teacher_name == null || this.lesson == null)
            {
                return false;
            }
            else
            {
                if (leaveTypeChildrenID.Trim() == "公假" || leaveTypeChildrenID.Trim() == "事假" || leaveTypeChildrenID.Trim() == "病假")
                {
                    if (lesson == "1" || lesson == "2" || lesson == "3" || lesson == "4" || lesson == "5")
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}