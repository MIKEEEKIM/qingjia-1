namespace qingjia_MVC.Models.API.Common
{
    public class LeaveListModel
    {
        public string LL_ID { get; set; }
        public string submitTime { get; set; }
        public string studentID { get; set; }
        public string reason { get; set; }
        public string leaveState { get; set; }
        public string leaveStateCode { get; set; }
        public string rejectReason { get; set; }
        public string leaveTypeID { get; set; }
        public string leaveTypeName { get; set; }
        public string leaveTime { get; set; }
        public string backTime { get; set; }
        public string leaveWay { get; set; }
        public string backWay { get; set; }
        public string address { get; set; }
        public string lesson { get; set; }
        public string teacher { get; set; }
        public string auditTeacher { get; set; }
        public string ST_Name { get; set; }
        public string ST_Tel { get; set; }
        public string ST_Class { get; set; }
        public string ST_Grade { get; set; }
        public string ST_TeacherName { get; set; }
        public string intershipCompany { get; set; }
        public string intershipAddress { get; set; }
        public string principalName { get; set; }
        public string principalTel { get; set; }
        public string pic_one { get; set; }
        public string pic_two { get; set; }
        public string pic_three { get; set; }
        public int isPrint { get; set; }
    }
}