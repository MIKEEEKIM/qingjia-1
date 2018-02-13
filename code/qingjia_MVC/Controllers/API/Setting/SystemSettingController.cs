using qingjia_MVC.Common;
using qingjia_MVC.Models;
using qingjia_MVC.Models.API;
using qingjia_MVC.Models.API.Setting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Z.EntityFramework.Plus;


namespace qingjia_MVC.Controllers.API.Setting
{
    [RoutePrefix("api/system")]
    public class SystemSettingController : BaseApiController
    {
        //此模块包含 请假类型设置、

        #region 请假类型设置
        /// <summary>
        /// 获取 请假类型设置情况
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpGet, Route("getleavetype")]
        public ApiResult GetLeaveType(string access_token)
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
                IQueryable<vw_TeacherLeaveType> leaveList = db.vw_TeacherLeaveType;
                SelectCondition conditionsModel = new SelectCondition();
                ConditionModel condition = CreatCondition("TeacherID", accountInfo.userID);
                conditionsModel.conditions.Add(condition);
                return Success("获取成功", GetList(conditionsModel, leaveList));
            }
            catch
            {
                return SystemError();
            }
            #endregion
        }

        /// <summary>
        /// 获取所有请假类型
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpGet, Route("leavetype")]
        public ApiResult LeaveType(string access_token)
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
                IQueryable<T_LeaveType> leaveList = db.T_LeaveType;
                SelectCondition conditionsModel = new SelectCondition();
                conditionsModel.conditions = null;
                return Success("获取成功", GetList(conditionsModel, leaveList));
            }
            catch
            {
                return SystemError();
            }
            #endregion
        }

        /// <summary>
        /// 设置所使用的请假类型
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("setleavetype")]
        public ApiResult SetLeaveType([FromBody] SetLeaveType model)
        {
            #region 令牌验证
            result = Check(model.access_token);
            if (result != null)
            {
                return result;
            }
            #endregion

            #region 逻辑操作
            try
            {
                if (model.leaveTypeIdList == null || model.enableMessage == null)
                {
                    return Error("leaveTypeIdList或enableMessage参数错误");
                }
                if (model.leaveTypeIdList.Count() != model.enableMessage.Count())
                {
                    return Error("leaveTypeIdList和enableMessage参数应为一一对应关系");
                }
                AccountInfo accountInfo = GetAccountInfo(model.access_token);

                //批量修改 T_TeacherLeaveType.Enable 字段
                db.T_TeacherLeaveType.Where(q => q.TeacherID == accountInfo.userID).Update(q => new T_TeacherLeaveType() { IsDelete = 1 });
                List<T_TeacherLeaveType> list = new List<T_TeacherLeaveType>();
                int i = 0;
                foreach (string _item in model.leaveTypeIdList)
                {
                    int itemID = Convert.ToInt32(model.leaveTypeIdList[i].ToString().Trim());
                    int itemEnableMessage = Convert.ToInt32(model.enableMessage[i].ToString().Trim());
                    if (db.T_LeaveType.Find(itemID) != null)
                    {
                        list.Add(new T_TeacherLeaveType() { LeaveTypeID = itemID, TeacherID = accountInfo.userID, EnableMessage = itemEnableMessage, IsDelete = 0 });
                    }
                    i++;
                }
                db.T_TeacherLeaveType.AddRange(list);
                db.SaveChanges();
                return Success("修改成功！");
            }
            catch (ArgumentException ex)
            {
                System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString());
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return SystemError();
            }
            #endregion
        }
        #endregion

        #region 请假记录 转换
        [HttpGet, Route("changeleavelist")]
        public ApiResult ChangeLeaveList()
        {
            System.Diagnostics.Debug.WriteLine("开始转换数据");
            string message = "";
            int i = 0;
            var oldList = from T_LeaveList in db.T_LeaveList orderby T_LeaveList.SubmitTime descending select T_LeaveList;
            if (oldList.Any())
            {
                message += "开始时间：" + DateTime.Now.ToString() + "       ";
                foreach (T_LeaveList listModel in oldList)
                {
                    T_New_LeaveList newModel = new T_New_LeaveList();
                    newModel.ID = listModel.ID;
                    newModel.StudentID = listModel.StudentID;
                    newModel.Reason = listModel.Reason;
                    newModel.SubmitTime = listModel.SubmitTime;
                    newModel.StateLeave = listModel.StateLeave;
                    newModel.StateBack = listModel.StateBack;
                    newModel.RejectReason = listModel.Notes;
                    if (listModel.TypeChildID == 4)
                    {
                        //短期请假
                        newModel.LeaveType = "1";
                        newModel.LeaveTypeChildrenID = null;
                    }
                    else if (listModel.TypeChildID == 5)
                    {
                        //长期请假
                        newModel.LeaveType = "2";
                        newModel.LeaveTypeChildrenID = null;
                    }
                    else if (listModel.TypeChildID == 6)
                    {
                        //节假日请假
                        newModel.LeaveType = "4";
                        newModel.LeaveTypeChildrenID = null;
                    }
                    else if (listModel.TypeID == 2 && listModel.TypeChildID == 7)
                    {
                        //晚点名请假
                        newModel.LeaveType = "5";
                        newModel.LeaveTypeChildrenID = "1";
                    }
                    else if (listModel.TypeID == 2 && listModel.TypeChildID == 8)
                    {
                        //晚点名请假
                        newModel.LeaveType = "5";
                        newModel.LeaveTypeChildrenID = "2";
                    }
                    else if (listModel.TypeID == 2 && listModel.TypeChildID == 9)
                    {
                        //晚点名请假
                        newModel.LeaveType = "5";
                        newModel.LeaveTypeChildrenID = "3";
                    }
                    else if (listModel.TypeID == 2 && listModel.TypeChildID == 10)
                    {
                        //早晚自习
                        newModel.LeaveType = "6";
                        newModel.LeaveTypeChildrenID = "1";
                    }
                    else if (listModel.TypeID == 2 && listModel.TypeChildID == 11)
                    {
                        //早晚自习
                        newModel.LeaveType = "6";
                        newModel.LeaveTypeChildrenID = "2";
                    }
                    else if (listModel.TypeID == 2 && listModel.TypeChildID == 12)
                    {
                        //早晚自习
                        newModel.LeaveType = "6";
                        newModel.LeaveTypeChildrenID = "3";
                    }
                    else if (listModel.TypeChildID == 13)
                    {
                        //上课请假
                        newModel.LeaveType = "7";
                        newModel.LeaveTypeChildrenID = "1";
                    }
                    else if (listModel.TypeChildID == 14)
                    {
                        //上课请假
                        newModel.LeaveType = "7";
                        newModel.LeaveTypeChildrenID = "2";
                    }
                    else if (listModel.TypeChildID == 15)
                    {
                        //上课请假
                        newModel.LeaveType = "7";
                        newModel.LeaveTypeChildrenID = "3";
                    }
                    else
                    {
                        //错误
                        newModel.LeaveType = null;
                        newModel.LeaveTypeChildrenID = null;
                    }

                    newModel.LeaveTime = listModel.TimeLeave;
                    newModel.BackTime = listModel.TimeBack;
                    newModel.LeaveWay = listModel.LeaveWay;
                    newModel.BackWay = listModel.BackWay;
                    newModel.Address = listModel.Address;
                    newModel.Lesson = listModel.Lesson;
                    newModel.Teacher = listModel.Teacher;
                    string teacherID = listModel.AuditTeacherID;
                    T_Teacher teacherModel = db.T_Teacher.Find(teacherID);
                    if (teacherModel != null)
                    {
                        newModel.AuditTeacher = teacherModel.Name;
                    }
                    newModel.PrintTimes = listModel.PrintTimes.ToString();
                    newModel.IsDelete = 0;

                    db.T_New_LeaveList.Add(newModel);
                    i++;
                    System.Diagnostics.Debug.WriteLine("添加第" + i + "条数据");
                }
                int count = db.SaveChanges();
                message += "结束时间：" + DateTime.Now.ToString() + "       ";
                message += "转换了：" + i + "条数据";
                System.Diagnostics.Debug.WriteLine(message);
            }
            return Success(message);
        }

        [HttpGet, Route("psdencryption")]
        public ApiResult PassWordEncryption()
        {
            int i = 0;
            System.Diagnostics.Debug.WriteLine("开始时间：" + DateTime.Now.ToString());
            try
            {
                var psdList = from T_Account in db.T_Account orderby T_Account.ID ascending select T_Account.ID;
                foreach (var item in psdList)
                {
                    T_Account accountModel = db.T_Account.Where(c => c.ID == item).ToList().First();
                    string _psd = PsdEncryption.Encryption(accountModel.Psd);
                    accountModel.Psd = _psd;
                    i++;
                    System.Diagnostics.Debug.WriteLine("添加第" + i + "条数据");
                }
                System.Diagnostics.Debug.WriteLine("开始保存修改的" + i + "条数据");
                db.SaveChanges();
                System.Diagnostics.Debug.WriteLine("结束时间：" + DateTime.Now.ToString());
                return Success("");
            }
            catch
            {
                return SystemError();
            }
        }
        #endregion
    }
}