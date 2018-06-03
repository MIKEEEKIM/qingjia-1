namespace qingjia_MVC.Models.API.Student
{
    public class LeaveSchoolModel
    {
        public string access_token { get; set; }
        public string leaveTypeID { get; set; }
        public string leave_date { get; set; }
        public string leave_time { get; set; }
        public string back_date { get; set; }
        public string back_time { get; set; }
        public string leave_reason { get; set; }
        public string leave_way { get; set; }
        public string back_way { get; set; }
        public string address { get; set; }

        public bool Check()
        {
            if (access_token == null || leaveTypeID == null || leave_date == null || leave_time == null || back_date == null || back_time == null || leave_reason == null || access_token == "" || leaveTypeID == "" || leave_date == "" || leave_time == "" || back_date == "" || back_time == "" || leave_reason == "")
            {
                return false;
            }
            if (leaveTypeID.Trim() == "1" || leaveTypeID.Trim() == "2" || leaveTypeID.Trim() == "4")
            {
                if (leaveTypeID.Trim() == "1")
                {
                    leave_way = "";
                    back_way = "";
                    address = "";
                    return true;
                }
                if (leaveTypeID.Trim() == "2")
                {
                    leave_way = "";
                    back_way = "";
                    address = "";
                    return true;
                }
                if (leaveTypeID.Trim() == "4")
                {
                    if (leave_reason == "回家" || leave_reason == "旅游" || leave_reason == "因公外出" || leave_reason == "其他")
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}