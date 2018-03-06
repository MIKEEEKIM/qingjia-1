namespace qingjia_MVC.Common
{
    //强制返回404错误
    public static class ForceHttpStatusCodeResult
    {
        public const string ForceHttpUnauthorizedHeaderName = "ForceHttpUnauthorizedHeader";
        public const string ForceHttpUnauthorizedHeaderValue = "true";

        public static void SetForceHttpUnauthorizedHeader()
        {
            //添加响应头 返回401错误
            System.Web.HttpContext.Current.Response.AddHeader(ForceHttpUnauthorizedHeaderName, ForceHttpUnauthorizedHeaderValue);
        }
    }
}