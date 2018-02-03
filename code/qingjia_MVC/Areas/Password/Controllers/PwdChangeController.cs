using FineUIMvc;
using qingjia_MVC.Controllers;
using qingjia_MVC.Models;
using System.Linq;
using System.Web.Mvc;

namespace qingjia_MVC.Areas.Password.Controllers
{
    public class PwdChangeController : BaseController
    {
        //实例化数据库连接
        private Entities db = new Entities();

        // GET: Password/PwdChange
        public ActionResult PwdChange()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult btnLogin_Click(FormCollection formvalues)
        {
            if (formvalues["tbxOldPwd"] != null && formvalues["tbxNewPwd"] != null)
            {
                //修改账号密码
                string UserID = Session["UserID"].ToString();
                string oldPwd = formvalues["tbxOldPwd"].ToString();
                string newPwd = formvalues["tbxNewPwd"].ToString();

                var accountList = from T_Account in db.T_Account where ((T_Account.Psd == oldPwd) && (T_Account.ID == UserID)) select T_Account;
                if (accountList.Any())
                {
                    //修改密码
                    T_Account account = accountList.ToList().First();
                    account.Psd = newPwd;
                    db.SaveChanges();
                    ShowNotify("密码更新成功！");
                }
                else
                {
                    //密码错误
                    ShowNotify("密码错误！");
                    //清空输入框
                }
            }
            if (formvalues["tbxPwd"] != null && formvalues["tbxST_Num"] != null)
            {
                //初始化学生密码
                string UserID = Session["UserID"].ToString();
                string UserPwd = formvalues["tbxPwd"].ToString();
                string ST_NUM = formvalues["tbxST_Num"].ToString();

                var accountlist = from T_Account in db.T_Account where ((T_Account.Psd == UserPwd) && (T_Account.ID == UserID)) select T_Account;
                if (accountlist.Any())
                {
                    //密码正确
                    T_Account account = db.T_Account.Find(ST_NUM);
                    if (account != null)
                    {
                        //此账号存在
                        account.Psd = ST_NUM.Substring(ST_NUM.Length - 6, 6);
                        db.SaveChanges();
                        ShowNotify("密码初始化成功，为学号后六位！");
                    }
                    else
                    {
                        //此账号不存在
                        ShowNotify("该学生账号不存在，请重新核对！");
                    }
                }
                else
                {
                    //密码错误
                }
            }
            return UIHelper.Result();
        }

        /// <summary>
        /// 判断当前用户是够为辅导员，只有辅导员可以初始化任一学生密码
        /// </summary>
        /// <param name="activeIndex"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TabStrip1_TabIndexChanged(int activeIndex)
        {
            string RoleID = Session["RoleID"].ToString();
            if (activeIndex == 1)
            {
                if (RoleID != "3")
                {
                    ShowNotify("当前用户不具备初始化学生密码权限！");
                    UIHelper.TabStrip("TabStrip1").ActiveTabIndex(0);
                }
            }
            return UIHelper.Result();
        }
    }
}