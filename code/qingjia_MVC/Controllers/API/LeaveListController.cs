using qingjia_MVC.Common;
using qingjia_MVC.Models;
using qingjia_MVC.Models.API;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace qingjia_MVC.Controllers.API
{
    #region 数据模型

    //离校请假数据模型
    public class LeaveSchoolModel
    {
        public string access_token { get; set; }
        public string leave_type { get; set; }
        public string leave_date { get; set; }
        public string leave_time { get; set; }
        public string back_date { get; set; }
        public string back_time { get; set; }
        public string leave_reason { get; set; }
        public string leave_way { get; set; }
        public string back_way { get; set; }
        public string address { get; set; }

        public bool Check()
        {
            if (this.access_token == null || this.leave_type == null || this.leave_date == null || this.leave_time == null || this.back_date == null || this.back_time == null || this.leave_reason == null)
            {
                return false;
            }
            if (leave_type == "短期请假" || leave_type == "长期请假" || leave_type == "节假日请假")
            {
                if (leave_type == "短期请假")
                {
                    leave_way = "";
                    back_way = "";
                    address = "";
                    return true;
                }
                if (leave_type == "长期请假")
                {
                    leave_way = "";
                    back_way = "";
                    address = "";
                    return true;
                }
                if (leave_type == "节假日请假")
                {
                    if (leave_reason == "回家" || leave_reason == "旅游" || leave_reason == "因公外出" || leave_reason == "其他")
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public class LeaveSpecialModel
    {
        public string access_token { get; set; }
        public string leave_type { get; set; }
        public string leave_child_type { get; set; }
        public string leave_date { get; set; }
        public string leave_time { get; set; }
        public string back_date { get; set; }
        public string back_time { get; set; }
        public string leave_reason { get; set; }

        public bool Check()
        {
            if (this.access_token == null || this.leave_type == null || this.leave_child_type == null || this.leave_date == null || this.leave_time == null || this.back_date == null || this.back_time == null || this.leave_reason == null)
            {
                return false;
            }
            else
            {
                if (leave_type == "晚点名请假" || leave_type == "早晚自习请假")
                {
                    if (leave_child_type == "公假" || leave_child_type == "事假" || leave_child_type == "病假")
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public class LeaveClassModel
    {
        public string access_token { get; set; }
        public string leave_child_type { get; set; }
        public string leave_date { get; set; }
        public string leave_reason { get; set; }
        public string teacher_name { get; set; }
        public string lesson { get; set; }

        public bool Check()
        {
            if (this.access_token == null || this.leave_child_type == null || this.leave_date == null || this.leave_reason == null || this.teacher_name == null || this.lesson == null)
            {
                return false;
            }
            else
            {
                if (leave_child_type == "公假" || leave_child_type == "事假" || leave_child_type == "病假")
                {
                    if (lesson == "1" || lesson == "2" || lesson == "3" || lesson == "4" || lesson == "5")
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    #endregion

    [RoutePrefix("api/leavelist")]
    public class LeaveListController : ApiController
    {
        //实例化数据库
        private imaw_qingjiaEntities db = new imaw_qingjiaEntities();

        /// <summary>
        /// GET
        /// 
        /// 用户获取个人请假记录（待审核以及待销假）
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpGet, Route("getlist")]
        public ApiBaseResult GetList(string access_token)
        {
            ApiBaseResult result = Check(access_token);
            if (result == null)
            {
                result = new ApiBaseResult();

                #region 获取数据

                string UserID = access_token.Substring(0, access_token.IndexOf("_"));

                var leavelist = from vw_LeaveList in db.vw_LeaveList where ((vw_LeaveList.StudentID == UserID) && (vw_LeaveList.StateBack == "0")) select vw_LeaveList;
                if (leavelist.Any())
                {
                    List<LeaveList> data = new List<LeaveList>();
                    foreach (vw_LeaveList item in leavelist)
                    {
                        LeaveList ll = new LeaveList();
                        ll.ID = item.ID;
                        ll.Reasoon = item.Reason;
                        ll.SubmitTime = item.Reason;
                        ll.Type = item.LeaveType;
                        ll.State = "";
                        ll.TimeLeave = (DateTime)item.TimeLeave;
                        ll.TimeBack = (DateTime)item.TimeBack;
                        if (item.StateLeave == "0")
                        {
                            ll.State = "待审核";
                        }
                        if (item.StateLeave == "1")
                        {
                            ll.State = "待销假";
                        }
                        ll.RejectNote = (item.Notes == null) ? "" : item.Notes;
                        ll.LeaveWay = (item.LeaveWay == null) ? "" : item.LeaveWay;
                        ll.BackWay = (item.BackWay == null) ? "" : item.BackWay;
                        ll.LeaveAddress = (item.Address == null) ? "" : item.Address;
                        ll.Lesson = (item.Lesson == null) ? "" : item.Lesson;
                        ll.TeacherName = (item.Teacher == null) ? "" : item.Teacher;

                        data.Add(ll);
                    }
                    result.result = "success";
                    result.data = data;
                }
                else
                {
                    result.result = "success";
                    result.messages = "无待审核或待销假请假记录！";
                }

                #endregion

                return result;
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        /// GET
        /// 撤销请假记录
        /// 学生删除待审核请假记录
        /// </summary>
        /// <param name="leavelistID"></param>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpGet, Route("revoke")]
        public ApiBaseResult Revoke(string leavelistID, string access_token)
        {
            ApiBaseResult result = Check(access_token);
            if (result == null)
            {
                result = new ApiBaseResult();

                if (leavelistID == null)
                {
                    result.result = "error";
                    result.messages = "参数格式错误或缺少参数！";
                    return result;
                }

                #region 获取数据

                string UserID = access_token.Substring(0, access_token.IndexOf("_"));

                var list = from T_LeaveList in db.T_LeaveList where ((T_LeaveList.StudentID == UserID) && (T_LeaveList.ID == leavelistID)) select T_LeaveList;//注意，不能删除其他人的请假记录

                if (list.Any())
                {
                    T_LeaveList ll = db.T_LeaveList.Find(leavelistID);
                    if (ll.StateBack == "0" && ll.StateLeave == "0")
                    {
                        db.T_LeaveList.Remove(ll);
                        db.SaveChanges();
                        result.result = "success";
                        result.messages = "撤回成功！";
                    }
                    else
                    {
                        result.result = "error";
                        result.messages = "非待审核状态，不能撤回！";
                    }
                }
                else
                {
                    result.result = "error";
                    result.messages = "该条请假记录不存在！";
                }

                #endregion

                return result;
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        /// POST
        /// 
        /// 学生离校请假
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("leaveschool")]
        public ApiBaseResult LeaveSchool([FromBody]LeaveSchoolModel data)
        {
            ApiBaseResult result;
            if (data != null)
            {
                if (data.Check())
                {
                    result = Check(data.access_token);
                    if (result == null)
                    {
                        return leaveSchool(data);
                    }
                    else
                    {
                        return result;
                    }
                }
                else
                {
                    result = new ApiBaseResult();
                    result.result = "error";
                    result.messages = "参数格式错误或缺少参数！";
                    return result;
                }
            }
            else
            {
                result = new ApiBaseResult();
                result.result = "error";
                result.messages = "参数错误！";
                return result;
            }
        }

        /// <summary>
        /// POST
        /// 
        /// 学生特殊请假
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost, Route("leavespecial")]
        public ApiBaseResult LeaveSpecial([FromBody]LeaveSpecialModel data)
        {
            ApiBaseResult result;
            if (data != null)
            {
                if (data.Check())
                {
                    result = Check(data.access_token);
                    if (result == null)
                    {
                        return leaveSpecial(data);
                    }
                    else
                    {
                        return result;
                    }
                }
                else
                {
                    result = new ApiBaseResult();
                    result.result = "error";
                    result.messages = "参数格式错误或缺少参数！";
                    return result;
                }
            }
            else
            {
                result = new ApiBaseResult();
                result.result = "error";
                result.messages = "参数错误！";
                return result;
            }
        }

        /// <summary>
        /// POST
        /// 
        /// 学生上课请假备案
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost, Route("leavelesson")]
        public ApiBaseResult LeaveLesson([FromBody]LeaveClassModel data)
        {
            ApiBaseResult result;
            if (data != null)
            {
                if (data.Check())
                {
                    result = Check(data.access_token);
                    if (result == null)
                    {
                        return leaveClass(data);
                    }
                    else
                    {
                        return result;
                    }
                }
                else
                {
                    result = new ApiBaseResult();
                    result.result = "error";
                    result.messages = "参数格式错误或缺少参数！";
                    return result;
                }
            }
            else
            {
                result = new ApiBaseResult();
                result.result = "error";
                result.messages = "参数错误！";
                return result;
            }
        }

        [HttpGet, Route("downloadpic")]
        public IHttpActionResult DownLoadPic(string LV_NUM)
        {
            var browser = string.Empty;
            if (HttpContext.Current.Request.UserAgent != null)
            {
                browser = HttpContext.Current.Request.UserAgent.ToUpper();
            }
            //string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res\\images\\qingjia", "测试图片.jpg");
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            //FileStream fileStream = File.OpenRead(filePath);
            FileStream fileStream = Print.Print_Form(LV_NUM);
            httpResponseMessage.Content = new StreamContent(fileStream);
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = LV_NUM + ".jpg"
                //FileName = browser.Contains("FIREFOX") ? Path.GetFileName(filePath) : HttpUtility.UrlEncode(Path.GetFileName(filePath))
                //FileName = HttpUtility.UrlEncode(Path.GetFileName(filePath))
            };
            return ResponseMessage(httpResponseMessage);
        }

        [HttpGet, Route("pathtest")]
        public ApiBaseResult PathTest()
        {
            ApiBaseResult result = new ApiBaseResult();
            result.data = HttpContext.Current.Server.MapPath("~/");
            return result;
        }

        [HttpGet, Route("print")]
        public ApiBaseResult PrintLeave(string access_token, string LV_NUM)
        {
            ApiBaseResult result = Check(access_token);
            if (result == null)
            {
                result = new ApiBaseResult();

                #region 检查此人是否有打印此请假条的权限
                string[] sArray = access_token.Split('_');
                string UserID = sArray[0];
                string GuidString = sArray[1];

                var accountList = from T_Account in db.T_Account where (T_Account.YB_AccessToken == GuidString) select T_Account;
                if (accountList.Any())
                {
                    T_Account accountModel = accountList.ToList().First();
                    if (accountModel.RoleID.ToString().Trim() == "1")
                    {
                        var leavelist = from vw_LeaveList in db.vw_LeaveList
                                        where (vw_LeaveList.StudentID == accountModel.ID && vw_LeaveList.ID == LV_NUM)
                                        select vw_LeaveList;
                        if (leavelist.Any())
                        {
                            string url = UpLoadQiNiu.UploadStream(Print.Print_Form(LV_NUM), LV_NUM);
                            if (url != null)
                            {
                                result.result = "success";
                                result.data = url;
                            }
                            else
                            {
                                result.result = "error";
                                result.messages = "出现错误，请联系系统维护人员";
                            }
                        }
                        else
                        {
                            result.result = "error";
                            result.messages = "您没有请假单号为" + LV_NUM + "此条请假记录";
                        }
                    }
                    else
                    {
                        result.result = "error";
                        result.messages = "此接口仅支持学生账号访问";
                    }
                }
                else
                {
                    result.result = "error";
                    result.messages = "出现未知错误，请联系维护人员";
                }
                #endregion
            }
            return result;
        }

        #region 其他方法

        /// <summary>
        /// 检查Access_Token
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        public ApiBaseResult Check(string access_token)
        {
            ApiBaseResult result = new ApiBaseResult();

            if (access_token != null)
            {
                string[] sArray = access_token.Split('_');
                string UserID = sArray[0];
                string GuidString = sArray[1];

                T_Account account = db.T_Account.Find(UserID);
                if (account != null)
                {
                    if (account.YB_AccessToken == GuidString)
                    {
                        return null;
                    }
                    else
                    {
                        result.result = "error";
                        result.messages = "Access_Token错误";
                        return result;
                    }
                }
                else
                {
                    result.result = "error";
                    result.messages = "Access_Token错误";
                    return result;
                }
            }
            else
            {
                result.result = "error";
                result.messages = "Access_Token为空，传值错误";
                return result;
            }
        }

        //是否可以请假 存在bug
        #region 学生请假操作

        private ApiBaseResult leaveSchool(LeaveSchoolModel data)
        {
            ApiBaseResult result = new ApiBaseResult();

            try
            {
                #region 请假操作
                string access_token = data.access_token;
                string ST_Num = access_token.Substring(0, access_token.IndexOf("_"));
                string LL_Type = data.leave_type;
                string leaveDate = data.leave_date;
                string leaveTime = data.leave_time;
                string backDate = data.back_date;
                string backTime = data.back_time;
                string leaveReason = data.leave_reason;
                string leaveWay = data.leave_way;
                string backWay = data.back_way;
                string address = data.leave_reason;
                string holidayType = data.leave_reason;//节假日请假的原因只能为三种

                //节假日请假时，请假原因为节假日请假的类型
                if (LL_Type == "节假日请假")
                {
                    leaveReason = data.leave_reason;
                }

                //string LV_Time_Go = leaveDate + " " + leaveTime + ":00";
                //string LV_Time_Back = backDate + " " + backTime + ":00";
                string LV_Time_Go = leaveDate + " " + leaveTime;
                string LV_Time_Back = backDate + " " + backTime;

                if (Convert.ToDateTime(LV_Time_Go) < Convert.ToDateTime(LV_Time_Back))
                {
                    DateTime time_go = Convert.ToDateTime(LV_Time_Go);
                    DateTime time_back = Convert.ToDateTime(LV_Time_Back);
                    TimeSpan time_days = time_back - time_go;
                    int days = time_days.Days;

                    if (LL_Type == "短期请假")
                    {
                        if (days < 3)//短期请假小于三天
                        {
                            //生成请假单号
                            string LV_NUM = DateTime.Now.ToString("yyMMdd");
                            var exist = from T_LeaveList in db.T_LeaveList where (T_LeaveList.StudentID == ST_Num) && (((T_LeaveList.TimeLeave >= time_go) && (T_LeaveList.TimeLeave <= time_back)) || ((T_LeaveList.TimeBack >= time_go) && (T_LeaveList.TimeBack <= time_back)) || ((T_LeaveList.TimeLeave <= time_go) && (T_LeaveList.TimeBack >= time_back))) select T_LeaveList;

                            if (exist.Any())
                            {
                                bool flag = true;
                                foreach (qingjia_MVC.Models.T_LeaveList leaveList in exist.ToList())
                                {
                                    if (leaveList.StateBack == "0")
                                    {
                                        flag = false;
                                        break;
                                    }
                                }
                                if (flag)
                                {
                                    //插入数据库操作
                                    if (Insert_LeaveList(LV_NUM, ST_Num, LL_Type, time_go, time_back, leaveReason, leaveWay, backWay, address, holidayType, null, null, null) == 1)
                                    {
                                        result.result = "success";
                                        result.messages = "请假申请成功！";
                                    }
                                    else
                                    {
                                        result.result = "error";
                                        result.messages = "数据库提交失败，请重新尝试!";
                                    }
                                }
                                else
                                {
                                    result.result = "error";
                                    result.messages = "您已提交过此时间段的请假申请，请不要重复提交！";
                                }
                            }
                            else
                            {
                                //插入数据库操作
                                if (Insert_LeaveList(LV_NUM, ST_Num, LL_Type, time_go, time_back, leaveReason, leaveWay, backWay, address, holidayType, null, null, null) == 1)
                                {
                                    result.result = "success";
                                    result.messages = "请假申请成功!";
                                }
                                else
                                {
                                    result.result = "error";
                                    result.messages = "数据库提交失败，请重新尝试!";
                                }
                            }
                        }
                        else
                        {
                            result.result = "error";
                            result.messages = "短期请假不能超过3天!";
                        }
                    }
                    else if (LL_Type == "长期请假")
                    {
                        if (days >= 3)//长期请假
                        {
                            //生成请假单号
                            string LV_NUM = DateTime.Now.ToString("yyMMdd");
                            var exist = from T_LeaveList in db.T_LeaveList where (T_LeaveList.StudentID == ST_Num) && (((T_LeaveList.TimeLeave >= time_go) && (T_LeaveList.TimeLeave <= time_back)) || ((T_LeaveList.TimeBack >= time_go) && (T_LeaveList.TimeBack <= time_back)) || ((T_LeaveList.TimeLeave <= time_go) && (T_LeaveList.TimeBack >= time_back))) select T_LeaveList;

                            if (exist.Any())
                            {
                                bool flag = true;
                                foreach (qingjia_MVC.Models.T_LeaveList leaveList in exist.ToList())
                                {
                                    if (leaveList.StateBack == "0")
                                    {
                                        flag = false;
                                        break;
                                    }
                                }
                                if (flag)
                                {
                                    //插入数据库操作
                                    if (Insert_LeaveList(LV_NUM, ST_Num, LL_Type, time_go, time_back, leaveReason, leaveWay, backWay, address, holidayType, null, null, null) == 1)
                                    {
                                        result.result = "success";
                                        result.messages = "请假申请成功!";
                                    }
                                    else
                                    {
                                        result.result = "error";
                                        result.messages = "数据库提交失败，请重新尝试!";
                                    }
                                }
                                else
                                {
                                    result.result = "error";
                                    result.messages = "您已提交过此时间段的请假申请，请不要重复提交！";
                                }
                            }
                            else
                            {
                                //插入数据库操作
                                if (Insert_LeaveList(LV_NUM, ST_Num, LL_Type, time_go, time_back, leaveReason, leaveWay, backWay, address, holidayType, null, null, null) == 1)
                                {
                                    result.result = "success";
                                    result.messages = "请假申请成功!";
                                }
                                else
                                {
                                    result.result = "error";
                                    result.messages = "数据库提交失败，请重新尝试!";
                                }
                            }
                        }
                        else
                        {
                            result.result = "error";
                            result.messages = "长期请假短期请假不能少于3天!";
                        }
                    }
                    else if (LL_Type == "节假日请假")
                    {
                        //生成请假单号
                        string LV_NUM = DateTime.Now.ToString("yyMMdd");
                        var exist = from T_LeaveList in db.T_LeaveList where (T_LeaveList.StudentID == ST_Num) && (((T_LeaveList.TimeLeave >= time_go) && (T_LeaveList.TimeLeave <= time_back)) || ((T_LeaveList.TimeBack >= time_go) && (T_LeaveList.TimeBack <= time_back)) || ((T_LeaveList.TimeLeave <= time_go) && (T_LeaveList.TimeBack >= time_back))) select T_LeaveList;

                        if (exist.Any())
                        {
                            bool flag = true;
                            foreach (qingjia_MVC.Models.T_LeaveList leaveList in exist.ToList())
                            {
                                if (leaveList.StateBack == "0")
                                {
                                    flag = false;
                                    break;
                                }
                            }
                            if (flag)
                            {
                                //插入数据库操作
                                if (Insert_LeaveList(LV_NUM, ST_Num, LL_Type, time_go, time_back, leaveReason, leaveWay, backWay, address, holidayType, null, null, null) == 1)
                                {
                                    result.result = "success";
                                    result.messages = "请假申请成功!";
                                }
                                else
                                {
                                    result.result = "error";
                                    result.messages = "数据库提交失败，请重新尝试!";
                                }
                            }
                            else
                            {
                                result.result = "error";
                                result.messages = "您已提交过此时间段的请假申请，请不要重复提交！";
                            }
                        }
                        else
                        {
                            //插入数据库操作
                            if (Insert_LeaveList(LV_NUM, ST_Num, LL_Type, time_go, time_back, leaveReason, leaveWay, backWay, address, holidayType, null, null, null) == 1)
                            {
                                result.result = "success";
                                result.messages = "请假申请成功!";
                            }
                            else
                            {
                                result.result = "error";
                                result.messages = "数据库提交失败，请重新尝试!";
                            }
                        }
                    }
                    else
                    {
                        //非离校请假类型
                    }
                }
                else
                {
                    result.result = "error";
                    result.messages = "请假开始时间不能小于结束时间!";
                }
                #endregion
            }
            catch
            {
                result.result = "error";
                result.messages = "请假失败，请检查参数格式是否符合要求！";
            }

            return result;
        }

        private ApiBaseResult leaveSpecial(LeaveSpecialModel data)
        {
            ApiBaseResult result = new ApiBaseResult();

            try
            {
                #region 请假操作
                string access_token = data.access_token;
                string ST_Num = access_token.Substring(0, access_token.IndexOf("_"));
                string LL_Type = data.leave_type + "(" + data.leave_child_type + ")";
                string leaveDate = data.leave_date;
                string leaveTime = data.leave_time;
                string backDate = data.back_date;
                string backTime = data.leave_time;
                string leaveReason = data.leave_reason;
                string leaveWay = null;
                string backWay = null;
                string address = null;
                string holidayType = null;

                if (data.leave_type == "晚点名请假")
                {
                    string LV_NUM = DateTime.Now.ToString("yyMMdd");//流水号生成
                    string studytime_sp = leaveDate + " " + leaveTime;
                    DateTime time_go = Convert.ToDateTime(studytime_sp);
                    DateTime time_back = time_go;
                    var exist = from T_LeaveList in db.T_LeaveList where ((T_LeaveList.StudentID == ST_Num) && (T_LeaveList.TimeLeave == time_go) && (T_LeaveList.TypeID == 2)) select T_LeaveList;

                    if (exist.Any())
                    {
                        bool flag = true;
                        foreach (qingjia_MVC.Models.T_LeaveList leaveList in exist.ToList())
                        {
                            if (leaveList.StateBack == "0")
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                        {
                            //插入数据库操作
                            if (Insert_LeaveList(LV_NUM, ST_Num, LL_Type, time_go, time_back, leaveReason, leaveWay, backWay, address, holidayType, null, null, null) == 1)
                            {
                                result.result = "success";
                                result.messages = "请假申请成功！";
                            }
                            else
                            {
                                result.result = "error";
                                result.messages = "数据库提交失败，请重新尝试!";
                            }
                        }
                        else
                        {
                            result.result = "error";
                            result.messages = "您已提交过此时间段的请假申请，请不要重复提交！";
                        }
                    }
                    else
                    {
                        //插入数据库操作
                        if (Insert_LeaveList(LV_NUM, ST_Num, LL_Type, time_go, time_back, leaveReason, leaveWay, backWay, address, holidayType, null, null, null) == 1)
                        {
                            result.result = "success";
                            result.messages = "请假申请成功！";
                        }
                        else
                        {
                            result.result = "error";
                            result.messages = "数据库提交失败，请重新尝试!";
                        }
                    }
                }
                else if (data.leave_type == "早晚自习请假")
                {
                    string LV_NUM = DateTime.Now.ToString("yyMMdd");//流水号生成
                    string studytime_sp = leaveDate + " " + leaveTime;
                    DateTime time_go = Convert.ToDateTime(studytime_sp);
                    DateTime time_back = time_go;
                    var exist = from T_LeaveList in db.T_LeaveList where ((T_LeaveList.StudentID == ST_Num) && (T_LeaveList.TimeLeave == time_go) && (T_LeaveList.TypeID == 2)) select T_LeaveList;

                    if (exist.Any())
                    {
                        bool flag = true;
                        foreach (qingjia_MVC.Models.T_LeaveList leaveList in exist.ToList())
                        {
                            if (leaveList.StateBack == "0")
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                        {
                            //插入数据库操作
                            if (Insert_LeaveList(LV_NUM, ST_Num, LL_Type, time_go, time_back, leaveReason, leaveWay, backWay, address, holidayType, null, null, null) == 1)
                            {
                                result.result = "success";
                                result.messages = "请假申请成功！";
                            }
                            else
                            {
                                result.result = "error";
                                result.messages = "数据库提交失败，请重新尝试!";
                            }
                        }
                        else
                        {
                            result.result = "error";
                            result.messages = "您已提交过此时间段的请假申请，请不要重复提交！";
                        }
                    }
                    else
                    {
                        //插入数据库操作
                        if (Insert_LeaveList(LV_NUM, ST_Num, LL_Type, time_go, time_back, leaveReason, leaveWay, backWay, address, holidayType, null, null, null) == 1)
                        {
                            result.result = "success";
                            result.messages = "请假申请成功！";
                        }
                        else
                        {
                            result.result = "error";
                            result.messages = "数据库提交失败，请重新尝试!";
                        }
                    }
                }
                #endregion
            }
            catch
            {
                result.result = "error";
                result.messages = "请假失败，请检查参数格式是否符合要求！";
            }

            return result;
        }

        private ApiBaseResult leaveClass(LeaveClassModel data)
        {
            ApiBaseResult result = new ApiBaseResult();

            try
            {
                #region 请假操作
                string access_token = data.access_token;
                string ST_Num = access_token.Substring(0, access_token.IndexOf("_"));
                string LL_Type = "上课请假备案(" + data.leave_child_type + ")";
                string leaveDate = data.leave_date;
                string leaveTime = "";
                string backDate = data.leave_date;
                string backTime = "";
                string leaveReason = data.leave_reason;
                string lesson = data.lesson;

                if (lesson == "1")
                {
                    leaveTime = "08:00";
                    backTime = "09:40";
                }
                if (lesson == "2")
                {
                    leaveTime = "10:10";
                    backTime = "11:50";
                }
                if (lesson == "3")
                {
                    leaveTime = "14:00";
                    backTime = "15:40";
                }
                if (lesson == "4")
                {
                    leaveTime = "16:00";
                    backTime = "17:40";
                }
                if (lesson == "5")
                {
                    leaveTime = "18:30";
                    backTime = "21:00";
                }
                string teacher = data.teacher_name;

                string LV_NUM = DateTime.Now.ToString("yyMMdd");//流水号生成
                //DateTime time_go = Convert.ToDateTime(leaveDate + " " + leaveTime + ":00");
                //DateTime time_back = Convert.ToDateTime(backDate + " " + backTime + ":00");
                DateTime time_go = Convert.ToDateTime(leaveDate + " " + leaveTime);
                DateTime time_back = Convert.ToDateTime(backDate + " " + backTime);

                var exist = from T_LeaveList in db.T_LeaveList where ((T_LeaveList.StudentID == ST_Num) && (T_LeaveList.Lesson == lesson) && (T_LeaveList.TypeID == 3) && (T_LeaveList.TimeLeave == time_go)) select T_LeaveList;

                if (exist.Any())
                {
                    bool flag = true;
                    foreach (qingjia_MVC.Models.T_LeaveList leaveList in exist.ToList())
                    {
                        if (leaveList.StateBack == "0")
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        //插入数据库操作
                        if (Insert_LeaveList(LV_NUM, ST_Num, LL_Type, time_go, time_back, leaveReason, null, null, null, null, null, lesson, teacher) == 1)
                        {
                            result.result = "success";
                            result.messages = "请假申请成功！";
                        }
                        else
                        {
                            result.result = "error";
                            result.messages = "数据库提交失败，请重新尝试!";
                        }
                    }
                    else
                    {
                        result.result = "error";
                        result.messages = "您已提交过此时间段的请假申请，请不要重复提交！";
                    }
                }
                else
                {
                    //插入数据库操作
                    if (Insert_LeaveList(LV_NUM, ST_Num, LL_Type, time_go, time_back, leaveReason, null, null, null, null, null, lesson, teacher) == 1)
                    {
                        result.result = "success";
                        result.messages = "请假申请成功";
                    }
                    else
                    {
                        result.result = "error";
                        result.messages = "数据库提交失败，请重新尝试!";
                    }
                }
                #endregion
            }
            catch
            {
                result.result = "error";
                result.messages = "请假失败，请检查参数格式";
            }

            return result;
        }

        #endregion

        #region 插入T_LeaveList表

        /// <summary>
        /// 插入请假表
        /// </summary>
        /// <param name="LV_Num">请假单号</param>
        /// <param name="ST_Num">学号</param>
        /// <param name="LL_Type">请假类型名称</param>
        /// <param name="TimeLeave">请假时间</param>
        /// <param name="TimeBack">返校时间</param>
        /// <param name="LeaveReason">请假原因</param>
        /// <param name="leaveWay">离校方式</param>
        /// <param name="backWay">返校方式</param>
        /// <param name="leaveAddress">离校地址</param>
        /// <param name="holidayType">假期去向</param>
        /// <param name="notes">请假驳回理由</param>
        /// <param name="lesson">课程</param>
        /// <param name="teacher">老师</param>
        /// <returns>插入数据库结果</returns>
        private int Insert_LeaveList(string LV_Num, string ST_Num, string LL_Type, DateTime TimeLeave, DateTime TimeBack, string LeaveReason, string leaveWay, string backWay, string leaveAddress, string holidayType, string notes, string lesson, string teacher)
        {
            string endString = "0001";
            var leavelist = from T_LeaveList in db.T_LeaveList where (T_LeaveList.ID.StartsWith(LV_Num)) orderby T_LeaveList.ID descending select T_LeaveList.ID;
            if (leavelist.Any())
            {
                string leaveNumTop = leavelist.First().ToString().Trim();
                int end = Convert.ToInt32(leaveNumTop.Substring(6, 4));
                end++;
                endString = end.ToString("0000");//按照此格式Tostring
            }
            //请假单号
            LV_Num += endString;
            //提交时间
            DateTime nowTime = DateTime.Now;
            //请假类型编号
            var type = from T_Type in db.T_Type where (T_Type.Name == LL_Type) select T_Type;

            int TypeID = (int)((T_Type)type.ToList().First()).FatherID;
            int TypeChildID = (int)((T_Type)type.ToList().First()).ID;

            T_LeaveList LL = new T_LeaveList();
            LL.ID = LV_Num;
            LL.StudentID = ST_Num;
            LL.Reason = LeaveReason;
            LL.SubmitTime = nowTime;
            LL.StateLeave = "0";
            LL.StateBack = "0";
            LL.Notes = notes;
            LL.TypeID = TypeID;
            LL.TypeChildID = TypeChildID;
            LL.TimeLeave = TimeLeave;
            LL.TimeBack = TimeBack;
            LL.LeaveWay = leaveWay;
            LL.BackWay = backWay;
            LL.Address = leaveAddress;
            LL.Lesson = lesson;
            LL.Teacher = teacher;
            db.T_LeaveList.Add(LL);
            try
            {
                return db.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                return 0;
            }
        }

        #endregion

        #region 生成请假条

        #endregion

        #endregion
    }
}
