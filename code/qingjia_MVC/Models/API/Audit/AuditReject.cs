namespace qingjia_MVC.Models.API.Audit
{
    public class AuditReject
    {
        public string access_token { get; set; }
        public string LL_ID { get; set; }
        public string rejectReason { get; set; }
    }
}