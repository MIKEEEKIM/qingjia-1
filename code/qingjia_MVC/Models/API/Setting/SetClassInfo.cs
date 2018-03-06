using System.Collections.Generic;

namespace qingjia_MVC.Models.API.Setting
{
    public class SetClassInfo
    {
        public string access_token { get; set; }

        public List<ClassInfoItem> classInfo { get; set; }
    }

    public class ClassInfoItem
    {
        public string classID { get; set; }

        public string monitorID { get; set; }
    }
}