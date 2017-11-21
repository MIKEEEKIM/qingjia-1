using System;

namespace qingjia_YiBan.HomePage
{
    public partial class imagetest : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            imgLeave.Src = Request.QueryString["picUrl"].ToString();
        }
    }
}