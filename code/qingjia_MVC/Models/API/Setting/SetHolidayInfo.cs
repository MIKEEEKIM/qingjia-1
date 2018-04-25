namespace qingjia_MVC.Models.API.Setting
{
    public class SetHolidayInfo
    {
        public string access_token { get; set; }
        public string name { get; set; }
        public string startdate { get; set; }
        public string starttime { get; set; }
        public string enddate { get; set; }
        public string endtime { get; set; }
        public int autoaudit { get; set; }
    }
}