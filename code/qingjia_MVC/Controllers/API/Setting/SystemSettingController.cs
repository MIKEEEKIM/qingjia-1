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
                //系统级接口 需要判断调用接口的角色类型
                AccountInfo accountInfo = GetAccountInfo(access_token);
                IQueryable<vw_TeacherLeaveType> leaveList = db.vw_TeacherLeaveType.Where(c => c.IsDelete == 0);
                SelectCondition conditionsModel = new SelectCondition();
                ConditionModel condition = CreatCondition("TeacherID", GetTeacherID(accountInfo));
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

        #region 设置班级负责人 -- 班长
        /// <summary>
        /// 获取班级信息
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpGet, Route("getclassinfo")]
        public ApiResult GetClassInfo(string access_token)
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
                if (accountInfo.userRoleID != "3")
                {
                    return Error("此接口仅限辅导员使用");
                }

                var classInfoList = db.vw_Class.Where(c => c.TeacherID == accountInfo.userID).OrderBy(c => c.ST_Name).ToList();

                return Success("获取成功！", classInfoList);
            }
            catch
            {
                return SystemError();
            }
            #endregion
        }

        /// <summary>
        /// 设置班级信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("setclassinfo")]
        public ApiResult SetClassInfo([FromBody] SetClassInfo model)
        {
            #region 令牌验证
            result = Check(model.access_token);
            if (result != null)
            {
                return result;
            }
            if (model.classInfo == null)
            {
                return Error("参数错误");
            }
            #endregion

            #region 逻辑操作
            try
            {
                AccountInfo accountInfo = GetAccountInfo(model.access_token);
                if (accountInfo.userRoleID != "3")
                {
                    return Error("此接口仅限辅导员使用");
                }
                foreach (var item in model.classInfo)
                {
                    T_Class classModel = db.T_Class.Where(c => c.ID == item.classID.ToString().Trim() && c.TeacherID == accountInfo.userID).ToList().First();
                    if (classModel != null)
                    {
                        T_Student studentModel = db.T_Student.Where(c => c.ID == item.monitorID.ToString().Trim() && c.ClassName == classModel.ClassName).ToList().First();
                        if (db.T_Student.Where(c => c.ID == item.monitorID.ToString().Trim() && c.ClassName == classModel.ClassName) != null)
                        {
                            classModel.MonitorID = item.monitorID;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                int count = db.SaveChanges();
                return Success("成功修改" + count + "条记录");
            }
            catch
            {
                return SystemError();
            }
            #endregion
        }
        #endregion

        #region 晚点名设置
        [HttpGet, Route("getweeklymeetinginfo")]
        public ApiResult GetWeeklyMeetingInfo(string access_token)
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
                if (accountInfo.userRoleID != "3")
                {
                    return Error("此接口仅限辅导员使用");
                }
                var list = db.vw_ClassBatch.Where(c => c.TeacherID == accountInfo.userID && c.Grade == accountInfo.Grade).OrderBy(c => c.Batch).ToList();

                return Success("获取数据成功", list);
            }
            catch
            {
                return SystemError();
            }
            #endregion
        }

        [HttpPost, Route("setweeklymeetinginfo")]
        public ApiResult SetWeeklyMeetingInfo([FromBody]SetWeeklyMeetingInfo model)
        {
            #region 令牌验证
            result = Check(model.access_token);
            if (result != null)
            {
                return result;
            }
            if (model.batchInfo == null)
            {
                return Error("参数错误");
            }
            #endregion

            #region 逻辑操作
            try
            {
                AccountInfo accountInfo = GetAccountInfo(model.access_token);
                if (accountInfo.userRoleID != "3")
                {
                    return Error("此接口仅限辅导员使用");
                }
                foreach (var item in model.batchInfo)
                {
                    T_Batch batchMode = db.T_Batch.Where(c => c.Batch == item.batchID && c.TeacherID == accountInfo.userID).ToList().First();
                    if (batchMode != null)
                    {
                        string _time = item.date + " " + item.time;
                        DateTime time = Convert.ToDateTime(_time);
                        batchMode.Datetime = time;
                        batchMode.Location = item.location;
                        foreach (var _item in item.classID)
                        {
                            T_Class classModel = db.T_Class.Where(c => c.ID == _item && c.TeacherID == accountInfo.userID && c.Grade == accountInfo.Grade).ToList().First();
                            if (classModel != null)
                            {
                                classModel.Batch = batchMode.ID;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                    else
                    {
                        batchMode = new T_Batch();
                        batchMode.ID = Guid.NewGuid();
                        batchMode.Batch = item.batchID;
                        batchMode.TeacherID = accountInfo.userID;
                        batchMode.Location = item.location;
                        string _time = item.date + " " + item.time;
                        DateTime time = Convert.ToDateTime(_time);
                        batchMode.Datetime = time;

                        db.T_Batch.Add(batchMode);

                        foreach (var _item in item.classID)
                        {
                            T_Class classModel = db.T_Class.Where(c => c.ID == _item && c.TeacherID == accountInfo.userID && c.Grade == accountInfo.Grade).ToList().First();
                            if (classModel != null)
                            {
                                classModel.Batch = batchMode.ID;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                }
                db.SaveChanges();
                return Success("保存成功");
            }
            catch
            {
                return SystemError();
            }
            #endregion
        }
        #endregion

        #region 节假日设置
        /// <summary>
        /// 获取节假日信息 辅导员
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpGet, Route("getholidayinfo")]
        public ApiResult GetHolidayInfo(string access_token)
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
                if (accountInfo.userRoleID != "3")
                {
                    return Error("此接口仅限辅导员使用");
                }
                IQueryable<T_Holiday> holidayModelList = db.T_Holiday.Where(c => c.TeacherID == accountInfo.userID).OrderByDescending(q => q.SubmitTime);
                if (holidayModelList.Any())
                {
                    DataList dtSource = new DataList();
                    List<HolidayInfo> data = new List<HolidayInfo>();
                    foreach (var item in holidayModelList)
                    {
                        HolidayInfo model = new HolidayInfo
                        {
                            ID = item.ID.ToString().Trim(),
                            Name = item.Name.ToString().Trim(),
                            StartTime = ((DateTime)item.StartTime).ToString("yyyy/MM/dd HH:mm:ss"),
                            EndTime = ((DateTime)item.EndTime).ToString("yyyy/MM/dd HH:mm:ss"),
                            SubmitTime = ((DateTime)item.SubmitTime).ToString("yyyy/MM/dd HH:mm:ss"),
                            DeadLine = ((DateTime)item.DeadLine).ToString("yyyy/MM/dd HH:mm:ss"),
                            TeacherID = accountInfo.userID,
                            TeacherName = accountInfo.userName,
                            AutoAudit = item.AutoAudit.ToString().Trim() == "1" ? "自动" : "手动",
                            IsDelete = item.IsDelete.ToString().Trim()
                        };
                        data.Add(model);
                    }
                    dtSource.total = holidayModelList.ToList().Count;
                    dtSource.list = data;
                    return Success("获取成功", dtSource);
                }
                else
                {
                    return Success("没有节假日记录！");
                }
            }
            catch (Exception ex)
            {
                return SystemError(ex);
            }
            #endregion
        }

        /// <summary>
        /// 获取某一节假日信息
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="holidayID"></param>
        /// <returns></returns>
        [HttpGet, Route("getholidayinfo")]
        public ApiResult GetHolidayInfo(string access_token, string holidayID)
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
                if (accountInfo != null)
                {
                    T_Holiday holiday = db.T_Holiday.Find(holidayID);
                    if (holiday != null && holiday.TeacherID == accountInfo.userID)
                    {
                        IQueryable<vw_New_LeaveList> _LL = db.vw_New_LeaveList.Where(q => q.LeaveType.ToString().Trim() == "4" && q.Teacher.ToString().Trim() == accountInfo.userID && ((q.LeaveTime <= holiday.StartTime && q.BackTime >= holiday.StartTime) || (q.LeaveTime <= holiday.EndTime && q.BackTime >= holiday.EndTime) || (q.LeaveTime >= holiday.StartTime && q.BackTime <= holiday.EndTime)));

                        SelectCondition conditionsModel = new SelectCondition();
                        conditionsModel.sortField = "SubmitTime";
                        conditionsModel.sortDirection = "DESC";
                        DataList dtSource = GetList(conditionsModel, _LL);
                        dtSource.list = TransformLL((List<vw_New_LeaveList>)dtSource.list);
                        return Success("获取成功", dtSource);
                    }
                    else
                    {
                        return Error("您的列表中不存在此条记录！");
                    }
                }
                else
                {
                    return Error();
                }
            }
            catch (Exception ex)
            {
                return SystemError(ex);
            }
            #endregion
        }

        /// <summary>
        /// 删除（结束）节假日信息
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="holidayID"></param>
        /// <returns></returns>
        [HttpGet, Route("deleteholidayinfo")]
        public ApiResult DeleteHolidayInfo(string access_token, string holidayID)
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
                //删除某次节假日信息、所有待销假全部转为已销假
                AccountInfo accountInfo = GetAccountInfo(access_token);
                if (accountInfo != null)
                {
                    T_Holiday holiday = db.T_Holiday.Find(holidayID);
                    if (holiday != null && holiday.TeacherID == accountInfo.userID)
                    {
                        int a = 0;//未销假请假记录

                        //列表中删除 此条记录
                        holiday.IsDelete = 1;
                        //清空所有请假
                        IQueryable<T_New_LeaveList> _LL = db.T_New_LeaveList.Where(q => q.LeaveType.ToString().Trim() == "4" && q.Teacher.ToString().Trim() == accountInfo.userID && ((q.LeaveTime <= holiday.StartTime && q.BackTime >= holiday.StartTime) || (q.LeaveTime <= holiday.EndTime && q.BackTime >= holiday.EndTime) || (q.LeaveTime >= holiday.StartTime && q.BackTime <= holiday.EndTime)) && q.StateBack.ToString().Trim() == "0");
                        if (_LL.Any())
                        {
                            a = _LL.Count();
                            foreach (var item in _LL)
                            {
                                T_New_LeaveList _ll_model = db.T_New_LeaveList.Find(item.ID);
                                _ll_model.StateBack = "1";
                            }
                            db.SaveChanges();
                            return Success("删除成功！在此期间的" + a + "条节假日请假记录已销假！");
                        }
                        else
                        {
                            return Success("删除成功！");
                        }
                    }
                    else
                    {
                        return Error("您的列表中不存在此条记录！");
                    }
                }
                else
                {
                    return Error();
                }
            }
            catch (Exception ex)
            {
                return SystemError(ex);
            }
            #endregion
        }

        /// <summary>
        /// 设置节假日信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("setholidayinfo")]
        public ApiResult SetHolidayInfo([FromBody]SetHolidayInfo model)
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
                AccountInfo accountInfo = GetAccountInfo(model.access_token);
                if (accountInfo.userRoleID != "3")
                {
                    return Error("此接口仅限辅导员使用");
                }
                if (model.autoaudit != 0 && model.autoaudit != 1)
                {
                    return Error("autoaudit参数值错误。");
                }

                string startTime = model.startdate + " " + model.starttime;
                string endTime = model.enddate + " " + model.endtime;
                string deadLine = model.deadlinedate + " " + model.deadlinetime;
                DateTime _startTime = Convert.ToDateTime(startTime);
                DateTime _endTime = Convert.ToDateTime(endTime);
                DateTime _deadLine = Convert.ToDateTime(deadLine);

                //检查是否与之前的节假日信息时间上有重叠
                IQueryable<T_Holiday> _holidayInfo = db.T_Holiday.Where(q => q.TeacherID == accountInfo.userID && ((_startTime >= q.StartTime && _startTime <= q.EndTime) || (_endTime >= q.StartTime && _endTime <= q.EndTime) || (_startTime <= q.StartTime && _endTime >= q.EndTime)));
                if (_holidayInfo.Any())
                {
                    return Error("节假日时间与其他节假日记录有重叠，请重新输入！");
                }

                IQueryable<T_Holiday> holidayInfo = db.T_Holiday.Where(q => q.TeacherID == accountInfo.userID && q.IsDelete == 0);
                if (holidayInfo.Any())
                {
                    foreach (var item in holidayInfo)
                    {
                        T_Holiday _model = db.T_Holiday.Find(item.ID);
                        _model.IsDelete = 1;
                    }
                    db.SaveChanges();
                }
                T_Holiday holidayModel = new T_Holiday();
                holidayModel.Name = model.name.ToString().Trim();
                holidayModel.StartTime = _startTime;
                holidayModel.EndTime = _endTime;
                holidayModel.DeadLine = _deadLine;
                holidayModel.SubmitTime = DateTime.Now;
                holidayModel.AutoAudit = model.autoaudit.ToString().Trim();
                holidayModel.TeacherID = accountInfo.userID;
                holidayModel.IsDelete = 0;
                db.T_Holiday.Add(holidayModel);
                db.SaveChanges();
                return Success("保存成功");
            }
            catch (Exception ex)
            {
                return SystemError(ex);
            }
            #endregion
        }

        /// <summary>
        /// 修改节假日信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("modifyholidayinfo")]
        public ApiResult ModifyHolidayInfo([FromBody]ModifyHolidayInfo model)
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
                AccountInfo accountInfo = GetAccountInfo(model.access_token);
                if (accountInfo.userRoleID != "3")
                {
                    return Error("此接口仅限辅导员使用");
                }
                if (model.autoaudit != 0 && model.autoaudit != 1)
                {
                    return Error("autoaudit参数值错误。");
                }

                string startTime = model.startdate + " " + model.starttime;
                string endTime = model.enddate + " " + model.endtime;
                string deadLine = model.deadlinedate + " " + model.deadlinetime;
                DateTime _startTime = Convert.ToDateTime(startTime);
                DateTime _endTime = Convert.ToDateTime(endTime);
                DateTime _deadLine = Convert.ToDateTime(deadLine);

                //检查是否与之前的节假日信息时间上有重叠
                IQueryable<T_Holiday> _holidayInfo = db.T_Holiday.Where(q => q.TeacherID == accountInfo.userID && ((_startTime >= q.StartTime && _startTime <= q.EndTime) || (_endTime >= q.StartTime && _endTime <= q.EndTime) || (_startTime <= q.StartTime && _endTime >= q.EndTime)) && q.IsDelete == 1);
                if (_holidayInfo.Any())
                {
                    return Error("节假日时间与其他节假日记录有重叠，修改失败！");
                }

                T_Holiday holidayModel = db.T_Holiday.Find(model.holidayID);
                if (holidayModel != null)
                {
                    if (holidayModel.IsDelete == 0)
                    {
                        holidayModel.Name = model.name.ToString().Trim();
                        holidayModel.StartTime = _startTime;
                        holidayModel.EndTime = _endTime;
                        holidayModel.DeadLine = _deadLine;
                        holidayModel.SubmitTime = DateTime.Now;
                        holidayModel.AutoAudit = model.autoaudit.ToString().Trim();
                        holidayModel.TeacherID = accountInfo.userID;
                        holidayModel.IsDelete = 0;
                        db.SaveChanges();
                        return Success("修改成功");
                    }
                    else
                    {
                        return Error("已结束的节假日信息不能修改！");
                    }
                }
                else
                {
                    return Error("此记录不存在！");
                }
            }
            catch (Exception ex)
            {
                return SystemError(ex);
            }
            #endregion
        }
        #endregion

        #region 清理数据库数据 请假记录 转换
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