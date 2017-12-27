using qingjia_MVC.Common;
using System;

namespace qingjia_MVC
{
    public partial class LeavePrint : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string LV_NUM = Request.QueryString["LV_NUM"].ToString();
            picUrl.Src = GetLeaveFormUrl(LV_NUM);
        }

        private string GetLeaveFormUrl(string LV_NUM)
        {
            string url = UpLoadQiNiu.UpLoadData(Print.Print_Form(LV_NUM), LV_NUM);
            if (url != "" && url != null)
            {
                return url;
            }
            else
            {
                return "";
            }
        }
    }
}