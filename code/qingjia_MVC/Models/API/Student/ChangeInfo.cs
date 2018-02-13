namespace qingjia_MVC.Models.API.Student
{
    public class ChangeInfo
    {
        public string access_token { get; set; }
        public string ST_Tel { get; set; }
        public string ST_QQ { get; set; }
        public string ST_Email { get; set; }
        public string ST_Guardian { get; set; }
        public string ST_GuardianName { get; set; }
        public string ST_GuardianTel { get; set; }
        public bool CheckInfo()
        {
            if (access_token == null || ST_Tel == null || ST_QQ == null || ST_Email == null || ST_Guardian == null || ST_GuardianName == null || ST_GuardianTel == null)
            {
                return false;
            }
            else
            {
                if (ST_Guardian == "父亲" || ST_Guardian == "母亲" || ST_Guardian == "其他")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}