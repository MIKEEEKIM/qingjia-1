using qingjia_MVC.Models;
using qingjia_MVC.Models.API;
using qingjia_MVC.Models.API.Admin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using Z.EntityFramework.Plus;

namespace qingjia_MVC.Controllers.API.Admin
{
    /// <summary>
    /// 管理员数据接口
    /// </summary>
    [RoutePrefix("api/admin")]
    public class AdminController : BaseApiController
    {
        [HttpGet, Route("get")]
        public ApiResult Get(string access_token)
        {
            #region 令牌验证
            result = Check(access_token);
            if (result != null)
            {
                return result;
            }
            #endregion

            #region 逻辑操作
            try
            {
                AccountInfo accountInfo = GetAccountInfo(access_token);
                if (accountInfo.userRoleID != "0")
                {
                    return Error("此接口仅限管理员访问");
                }

                ReturnData data = new ReturnData();

                #region 用户数据
                List<string> gradeList = db.vw_Class.Select(q => q.Grade).ToList();
                HashSet<string> _gradeList = new HashSet<string>(gradeList);

                Dictionary<string, int> accountData = new Dictionary<string, int>();
                int male = 0;
                int female = 0;

                foreach (var item in _gradeList)
                {
                    int num = 0;
                    num = db.vw_Student.Where(q => q.ST_Grade == item).Count();
                    accountData.Add(item.Trim() + "级", num);

                    male = male + db.vw_Student.Where(q => q.ST_Grade == item && q.ST_Sex.Trim() == "男").Count();
                    female = female + db.vw_Student.Where(q => q.ST_Grade == item && q.ST_Sex.Trim() == "女").Count();
                }
                accountData.Add("男生", male);
                accountData.Add("女生", female);

                data.accountData = accountData;
                #endregion

                #region 短信发送数据
                List<MessageStatistic> messageList = new List<MessageStatistic>();
                List<T_SendList> sendMsgList = db.T_SendList.OrderByDescending(q => q.timeString).ToList();
                HashSet<string> _sendMsgList = new HashSet<string>();
                int i = 0;
                int j = 0;
                int k = 0;
                string _time = "";
                foreach (var item in sendMsgList)
                {
                    if (_sendMsgList.Add(item.timeString.ToString("yyyy-MM-dd")))
                    {
                        if (_time != "")
                        {
                            messageList.Add(new MessageStatistic
                            {
                                time = _time,
                                go = i,
                                back = j,
                                failed = k
                            });
                            if (item.MessageType.Trim() == "go")
                            {
                                i = 1;
                                j = 0;
                                k = 0;
                            }
                            if (item.MessageType.Trim() == "back")
                            {
                                i = 0;
                                j = 1;
                                k = 0;
                            }
                            if (item.MessageType.Trim() == "failed")
                            {
                                i = 0;
                                j = 0;
                                k = 1;
                            }
                            _time = item.timeString.ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            if (item.MessageType.Trim() == "go")
                            {
                                i = 1;
                                j = 0;
                                k = 0;
                            }
                            if (item.MessageType.Trim() == "back")
                            {
                                i = 0;
                                j = 1;
                                k = 0;
                            }
                            if (item.MessageType.Trim() == "failed")
                            {
                                i = 0;
                                j = 0;
                                k = 1;
                            }
                            _time = item.timeString.ToString("yyyy-MM-dd");
                        }
                    }
                    else
                    {
                        if (item.MessageType.Trim() == "go")
                        {
                            i++;
                        }
                        if (item.MessageType.Trim() == "back")
                        {
                            j++;
                        }
                        if (item.MessageType.Trim() == "failed")
                        {
                            k++;
                        }
                    }
                }
                messageList.Add(new MessageStatistic
                {
                    time = _time,
                    go = i,
                    back = j,
                    failed = k
                });

                data.msgSendData = messageList;
                #endregion

                return Success("获取成功", data);
            }
            catch (Exception ex)
            {
                return SystemError(ex);
            }
            #endregion
        }

        [HttpGet, Route("downloadtemplet")]
        public HttpResponseMessage DownLoadTemplet()
        {
            var root = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/Templet");
            string fileName = "学生模板.xlsx";
            string path = Path.Combine(root, fileName);
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(path, FileMode.Open);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = fileName;
            //result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(fileName));
            result.Content.Headers.ContentLength = stream.Length;
            return result;
        }
    }
}