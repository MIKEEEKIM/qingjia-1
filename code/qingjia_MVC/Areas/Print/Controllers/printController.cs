using FineUIMvc;
using qingjia_MVC.Common;
using qingjia_MVC.Controllers;
using qingjia_MVC.Models;
using System;
using System.Web.Mvc;

namespace qingjia_MVC.Areas.Print.Controllers
{

    public class printController : BaseController
    {
        private imaw_qingjiaEntities db = new imaw_qingjiaEntities();

        // GET: Print/print
        public ActionResult Index()
        {
            LoadData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Window1_Close(string Message)
        {
            string script = string.Format("alert('" + Message + "');");
            PageContext.RegisterStartupScript(ActiveWindow.GetHideReference() + script);
            return UIHelper.Result();
        }

        /// <summary>
        /// 加载请假条
        /// </summary>
        private void LoadData()
        {
            //获取打印的请假单号，将请假单号用ViewBag传到View中，在View中调用Controller中的方法，获得生成图片的二进制流
            string LV_NUM = Request.QueryString["id"].ToString();
            string picUrl = UpLoadQiNiu.Stat(LV_NUM);
            //ViewBag.printNum = LV_NUM;
            if (picUrl == null)
            {
                picUrl = UpLoadQiNiu.UpLoadData(Common.Print.Print_Form(LV_NUM), LV_NUM);
            }
            if (picUrl != "" && picUrl != null)
            {
                ShowNotify("加载成功，右键保存到桌面，打印即可。");
                ViewBag.picUrl = picUrl;
            }
            else
            {
                ShowNotify("加载失败，请联系辅导员。");
                ViewBag.picUrl = "";
            }
        }
    }
}