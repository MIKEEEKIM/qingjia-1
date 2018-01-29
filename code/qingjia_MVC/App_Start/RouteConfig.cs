﻿using System.Web.Mvc;
using System.Web.Routing;

namespace qingjia_MVC
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                //defaults: new { controller = "Account", action = "Index", id = UrlParameter.Optional }//网站主页   正常运行网站时  将此设定为主页
                defaults: new { controller = "ApiTest", action = "Default", id = UrlParameter.Optional }//API接口主页   测试API接口是  将此设定为主页
            );
        }
    }
}
