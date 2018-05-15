using qingjia_MVC.Models.API.Common;
using System.Collections.Generic;

namespace qingjia_MVC.Models.API.Student
{
    public class ReturnLeaveListInfo
    {
        public int leaveNum = 0;
        public int weeklyMeetingNum = 0;
        public int selfStudyNum = 0;
        public int classLeaveNum = 0;

        public Dictionary<string, string> leaveType = new Dictionary<string, string>();
        public Dictionary<string, string> weeklyMeetingType = new Dictionary<string, string>();
        public Dictionary<string, string> selfStudyType = new Dictionary<string, string>();
        public Dictionary<string, string> classLeaveType = new Dictionary<string, string>();

        public List<LeaveListModel> leavelistArray = new List<LeaveListModel>();
    }
}