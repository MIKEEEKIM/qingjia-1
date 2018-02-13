namespace qingjia_MVC.Models.API.Student
{
    public class ChangePassword
    {
        public string access_token { get; set; }
        public string old_psd { get; set; }
        public string new_psd { get; set; }
    }
}