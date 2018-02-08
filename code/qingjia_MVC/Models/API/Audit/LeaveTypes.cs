namespace qingjia_MVC.Models.API.Audit
{
    public class LeaveTypes
    {
        public string leaveTypeID { get; set; }
        public string leaveTypeName { get; set; }
        public int count { get; set; }
        public string leaveTypeDescription { get; set; }
        public string enableMessage { get; set; }

        //public LeaveTypes()
        //{
        //}

        //public LeaveTypes(string _leaveTypeID, string _leaveTypeName, string _leaveTypeDescription, string _enableMessage)
        //{
        //    leaveTypeID = _leaveTypeID;
        //    leaveTypeName = _leaveTypeName;
        //    leaveTypeDescription = _leaveTypeDescription;
        //    enableMessage = _enableMessage;
        //}
    }
}