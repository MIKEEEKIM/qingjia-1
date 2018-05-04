using System.Collections.Generic;

namespace qingjia_MVC.Models.API.Setting
{
    public class HolidayLeaveListInfo
    {
        public Dictionary<string, DataList> ll_info_groupbyclass { get; set; }
        public Dictionary<string, int> ll_info_status { get; set; }
    }
}