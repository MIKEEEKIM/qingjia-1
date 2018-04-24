using System;
using System.Collections.Generic;

namespace qingjia_MVC.Models.API.AddressList
{
    public class StudentAddressInfo
    {
        public StudentListModel student = new StudentListModel();
        public bool IsLeave { get; set; }

        public int total = 0;
        public List<LeaveStatisticModel> leaveNumInfo { get; set; }

        public StudentAddressInfo()
        {
        }

        public StudentAddressInfo(vw_Student studentModel)
        {
            student.StudentID = studentModel.ST_Num.Trim();
            student.Name = studentModel.ST_Name.Trim();
            student.Email = studentModel.ST_Email.Trim();
            student.QQ = studentModel.ST_QQ.Trim();
            student.Tel = studentModel.ST_Tel.Trim();
            student.ContactName = studentModel.ContactOne.Trim();
            student.ContactTel = studentModel.OneTel.Trim();
            student.Sex = studentModel.ST_Sex.Trim();
            student.Dor = studentModel.ST_Dor.Trim();
            student.ClassName = studentModel.ST_Class.Trim();
            student.Grade = studentModel.ST_Grade.Trim();
            student.TeacherID = studentModel.ST_TeacherID.Trim();
            student.TeacherName = studentModel.ST_Teacher.Trim();
            student.MonitorID = studentModel.MonitorID == null ? "" : studentModel.MonitorID.ToString().Trim();
            student.MonitorName = studentModel.MonitorName == null ? "" : studentModel.MonitorName.ToString().Trim();
        }
    }

    public class LeaveStatisticModel
    {
        public string leaveTypeName { get; set; }
        public string leaveTypeID { get; set; }
        public int leaveTypeNum { get; set; }

        public LeaveStatisticModel()
        {

        }

        public LeaveStatisticModel(string ID, string Name, int Num)
        {
            this.leaveTypeID = ID;
            this.leaveTypeName = Name;
            this.leaveTypeNum = Num;
        }
    }
}