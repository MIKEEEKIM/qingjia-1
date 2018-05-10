using System.Collections.Generic;

namespace qingjia_MVC.Models.API.Setting
{
    public class SetWeeklyMeetingInfo
    {
        public string access_token { get; set; }
        public string deadline { get; set; }
        public List<BatchModel> batchInfo { get; set; }
    }
    public class BatchModel
    {
        public int batchID { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public string location { get; set; }
        public List<string> classID { get; set; }
        public int AutoUpdateTime { get; set; }
        public int AutoUpdateTimeSpan { get; set; }
    }
}