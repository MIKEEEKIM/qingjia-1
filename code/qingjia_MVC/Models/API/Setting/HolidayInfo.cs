namespace qingjia_MVC.Models.API.Setting
{
    public class HolidayInfo
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string DeadLine { get; set; }
        public string AutoAudit { get; set; }
        public string TeacherID { get; set; }
        public string TeacherName { get; set; }
        public string SubmitTime { get; set; }
        public string IsDelete { get; set; }
    }
}