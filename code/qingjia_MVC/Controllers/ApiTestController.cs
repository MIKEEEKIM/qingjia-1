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
    }
}