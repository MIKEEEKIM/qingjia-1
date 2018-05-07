using System.Collections.Generic;

namespace qingjia_MVC.Models.API.User
{
    public class TeacherLeaveListStatistic
    {
        public Dictionary<string, int> statisticByLeavetype { get; set; }
        public Dictionary<string, int> statisticByClass { get; set; }
    }
}