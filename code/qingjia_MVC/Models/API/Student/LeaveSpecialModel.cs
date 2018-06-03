namespace qingjia_MVC.Models.API.Student
{
    public class LeaveSpecialModel
    {
        public string access_token { get; set; }
        public string leaveTypeID { get; set; }
        public string leaveTypeChildrenID { get; set; }
        public string leave_date { get; set; }
        public string leave_reason { get; set; }

        public bool Check()
        {
            if (access_token == null || leaveTypeID == null || leaveTypeChildrenID == null || leave_date == null || leave_reason == null || access_token == "" || leaveTypeID == "" || leaveTypeChildrenID == "" || leave_date == "" || leave_reason == "")
            {
                return false;
            }
            else
            {
                if (leaveTypeID.Trim() == "5" || leaveTypeID.Trim() == "6" || leaveTypeID.Trim() == "7")
                {
                    if (leaveTypeChildrenID.Trim() == "1" || leaveTypeChildrenID.Trim() == "2" || leaveTypeChildrenID.Trim() == "3")
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}