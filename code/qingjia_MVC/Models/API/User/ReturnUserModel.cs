namespace qingjia_MVC.Models.API.User
{
    public class ReturnUserModel
    {
        public string access_token { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string RoleID { get; set; }
        public string IsMonitor = "0";//0代表非班级负责人 1代表为班级负责人 非学生账号 此参数均为0
    }
}