using System.Web.Mvc;

namespace qingjia_MVC.Controllers
{
    public class ApiTestController : Controller
    {
        // GET: BaseApi
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Default()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        public ActionResult Api_Test()
        {
            ViewBag.Title = "Api_Test";

            return View();
        }

        public ActionResult Api_Audit()
        {
            ViewBag.Title = "Api_Audit";

            return View();
        }

        public ActionResult Api_Setting()
        {
            ViewBag.Title = "Api_Setting";

            return View();
        }

        public ActionResult Api_Login()
        {
            ViewBag.Title = "Api_Login";

            return View();
        }

        public ActionResult Api_Student()
        {
            ViewBag.Title = "Api_Student";

            return View();
        }

        public ActionResult Api_Teacher()
        {
            ViewBag.Title = "Api_Teacher";

            return View();
        }
    }
}