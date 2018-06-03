using System.Collections.Generic;

namespace qingjia_MVC.Models.API.Admin
{
    public class ReturnData
    {
        public Dictionary<string, int> accountData { get; set; }
        public List<MessageStatistic> msgSendData { get; set; }
    }
}