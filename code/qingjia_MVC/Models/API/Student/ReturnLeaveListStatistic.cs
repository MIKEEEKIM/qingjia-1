using System.Collections.Generic;

namespace qingjia_MVC.Models.API.Student
{
    public class ReturnLeaveListStatistic
    {
        public Dictionary<string, int> statisticByStatus = new Dictionary<string, int>();
        public Dictionary<string, int> statisticByLeaveType = new Dictionary<string, int>();
    }
}