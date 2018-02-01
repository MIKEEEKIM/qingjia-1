using qingjia_MVC.Common;
using qingjia_MVC.Models;
using qingjia_MVC.Models.API;
using System.Web.Http;

namespace qingjia_MVC.Controllers.API.Common
{
    [RoutePrefix("api/common")]
    public class PrintPic : BaseApiController
    {
        /// <summary>
        /// Get 打印接口
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="LL_ID"></param>
        /// <returns></returns>
        [HttpGet, Route("print")]
        public ApiResult Print(string access_token, string LL_ID)
        {
            #region 令牌 权限验证
            result = Check(access_token);
            if (result != null)
            {
                return result;
            }
            #endregion

            #region 逻辑操作
            //验证是否具有打印请假条权限
            try
            {
                AccountInfo accountInfo = GetAccountInfo(access_token);
                vw_New_LeaveList _LL = GetLeaveListModel(LL_ID);
                if (_LL != null)
                {
                    bool flag = false;
                    if (accountInfo.userRoleID == "1")
                    {
                        if (accountInfo.userID == _LL.StudentID)
                        {
                            flag = true;
                        }
                    }
                    if (accountInfo.userRoleID == "2")
                    {
                        if (accountInfo.userName == _LL.ST_Class)
                        {
                            flag = true;
                        }
                    }
                    if (accountInfo.userRoleID == "3")
                    {
                        if (accountInfo.userID == _LL.ST_TeacherID)
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        string picUrl = UpLoadQiNiu.Stat(LL_ID);
                        if (picUrl == null)
                        {
                            picUrl = UpLoadQiNiu.UpLoadData(qingjia_MVC.Common.Print.Print_Form(new _Print_LL(_LL)), LL_ID);
                        }

                        return Success("生成请假条成功", picUrl);
                    }
                    else
                    {
                        return Error("您不具备打印此请假条权限！");
                    }
                }
                return Error("此请假记录不存在，请重新输入！");
            }
            catch
            {
                return SystemError();
            }
            #endregion
        }
    }
}