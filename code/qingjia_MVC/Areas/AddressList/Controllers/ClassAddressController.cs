using qingjia_MVC.Models;
using System.Linq;
using System.Web.Mvc;

namespace qingjia_MVC.Areas.AddressList.Controllers
{
    public class ClassAddressController : Controller
    {
        private imaw_qingjiaEntities db = new imaw_qingjiaEntities();

        // GET: AddressList/ClassAddress
        public ActionResult Index()
        {
            string classname = db.T_Student.Find(Session["UserID"].ToString()).ClassName.ToString();
            var stuList = db.T_Student.Where(stu => stu.ClassName == classname);
            return View(stuList.ToList());
        }
    }
}