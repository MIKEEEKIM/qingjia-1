using System.Collections.Generic;

namespace qingjia_MVC.Models.API.Setting
{
    public class SetLeaveType
    {
        public string access_token { get; set; }
        public List<string> leaveTypeIdList { get; set; }
    }
}