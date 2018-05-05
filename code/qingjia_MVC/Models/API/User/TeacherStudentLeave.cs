using System.Collections.Generic;

namespace qingjia_MVC.Models.API.User
{
    public class TeacherStudentLeave
    {
        public List<string> studentIDList { get; set; }
        public Dictionary<string, vw_Student> studentList { get; set; }
    }
}