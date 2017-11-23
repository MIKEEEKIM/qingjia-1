using FineUIMvc;
using qingjia_MVC.Common;
using qingjia_MVC.Controllers;
using qingjia_MVC.Models;
using System;
using System.IO;
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
            string script = String.Format("alert('" + Message + "');");
            PageContext.RegisterStartupScript(ActiveWindow.GetHideReference() + script);
            return UIHelper.Result();
        }

        private void LoadData()
        {
            //获取打印的请假单号，将请假单号用ViewBag传到View中，在View中调用Controller中的方法，获得生成图片的二进制流
            string LV_NUM = Request.QueryString["id"].ToString();
            //ViewBag.printNum = LV_NUM;
            string picUrl = UpLoadQiNiu.UpLoadData(Common.Print.Print_Form(LV_NUM), LV_NUM);
            if (picUrl != "")
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

        //public FileResult PrintForm(string LV_NUM)
        //{
        //    byte[] photo = new byte[0];
        //    byte[] file = Common.Print.Print_Form(LV_NUM);
        //    photo = new byte[file.Length];
        //    file.Read(photo, 0, photo.Length);
        //    file.Close();
        //    return File(photo, "image/jpeg");
        //}
    }
}