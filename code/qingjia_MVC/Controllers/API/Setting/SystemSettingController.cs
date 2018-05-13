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
                List<T_LeaveType> leaveList = db.T_LeaveType.OrderBy(q => q.ID).ToList();
                List<ReturnLeaveTypeList> data = new List<ReturnLeaveTypeList>();
                if (leaveList.Any())
                {
                    foreach (var item in leaveList)
                    {
                        ReturnLeaveTypeList _item = new ReturnLeaveTypeList
                        {
                            id = item.ID.ToString().Trim(),
                            name = item.Name.ToString().Trim(),
                            description = item.Description == null ? "" : item.Description.ToString().Trim()
                        };
                        data.Add(_item);
                    }
                }
                return Success("获取成功", data);
            }
            catch(Exception ex)
            {
                return SystemError(ex);
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
                    //int itemEnableMessage = Convert.ToInt32(model.enableMessage[i].ToString().Trim());
                    int itemEnableMessage = model.enableMessage[i] == "true" ? 1 : 0;
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

                //db = new Entities();
                IQueryable<vw_Class> classList = db.vw_Class.Where(c => c.TeacherID == accountInfo.userID).OrderBy(c => c.ClassName);
                if (classList.Any())
                {
                    return Success("获取成功！", classList);
                }
                else
                {
                    return Error("尚未绑定班级！");
                }
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
                //db.Dispose();
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
                if (model.deadline != "" && model.deadline != null)
                {
                    T_Deadline _deadLine = db.T_Deadline.Where(q => q.TeacherID.Trim() == accountInfo.userID && q.TypeID == 2).ToList().First();
                    DateTime dl = Convert.ToDateTime(model.deadline);
                    _deadLine.Time = dl;
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
                        batchMode.AutoUpdateTime = item.AutoUpdateTime.ToString();
                        batchMode.AutoUpdateTimeSpan = item.AutoUpdateTimeSpan;
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
                        batchMode = new T_Batch
                        {
                            ID = Guid.NewGuid(),
                            Batch = item.batchID,
                            TeacherID = accountInfo.userID,
                            Location = item.location,
                            AutoUpdateTime = item.AutoUpdateTime.ToString(),
                            AutoUpdateTimeSpan = item.AutoUpdateTimeSpan
                        };
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
                            AutoAudit = item.AutoAudit.ToString().Trim(),
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
                    int id = Convert.ToInt32(holidayID);
                    T_Holiday holiday = db.T_Holiday.Find(id);
                    if (holiday != null && holiday.TeacherID == accountInfo.userID)
                    {

                        IQueryable<IGrouping<string, vw_New_LeaveList>> _LL = db.vw_New_LeaveList.Where(q => q.LeaveType.ToString().Trim() == "4" && q.ST_TeacherID.ToString().Trim() == accountInfo.userID && ((q.LeaveTime <= holiday.StartTime && q.BackTime >= holiday.StartTime) || (q.LeaveTime <= holiday.EndTime && q.BackTime >= holiday.EndTime) || (q.LeaveTime <= holiday.StartTime && q.BackTime >= holiday.EndTime))).OrderByDescending(q => q.SubmitTime).GroupBy(q => q.ST_Class);

                        //待审核
                        int a = db.vw_New_LeaveList.Where(q => q.LeaveType.ToString().Trim() == "4" && q.ST_TeacherID.ToString().Trim() == accountInfo.userID && ((q.LeaveTime <= holiday.StartTime && q.BackTime >= holiday.StartTime) || (q.LeaveTime <= holiday.EndTime && q.BackTime >= holiday.EndTime) || (q.LeaveTime <= holiday.StartTime && q.BackTime >= holiday.EndTime))).Where(q => q.StateLeave.ToString().Trim() == "0" && q.StateBack.ToString().Trim() == "0").ToList().Count();

                        //待销假
                        int b = db.vw_New_LeaveList.Where(q => q.LeaveType.ToString().Trim() == "4" && q.ST_TeacherID.ToString().Trim() == accountInfo.userID && ((q.LeaveTime <= holiday.StartTime && q.BackTime >= holiday.StartTime) || (q.LeaveTime <= holiday.EndTime && q.BackTime >= holiday.EndTime) || (q.LeaveTime <= holiday.StartTime && q.BackTime >= holiday.EndTime))).Where(q => q.StateLeave.ToString().Trim() == "1" && q.StateBack.ToString().Trim() == "0").ToList().Count();

                        //已销假
                        int c = db.vw_New_LeaveList.Where(q => q.LeaveType.ToString().Trim() == "4" && q.ST_TeacherID.ToString().Trim() == accountInfo.userID && ((q.LeaveTime <= holiday.StartTime && q.BackTime >= holiday.StartTime) || (q.LeaveTime <= holiday.EndTime && q.BackTime >= holiday.EndTime) || (q.LeaveTime <= holiday.StartTime && q.BackTime >= holiday.EndTime))).Where(q => q.StateLeave.ToString().Trim() == "1" && q.StateBack.ToString().Trim() == "1").ToList().Count();

                        //已驳回
                        int d = db.vw_New_LeaveList.Where(q => q.LeaveType.ToString().Trim() == "4" && q.ST_TeacherID.ToString().Trim() == accountInfo.userID && ((q.LeaveTime <= holiday.StartTime && q.BackTime >= holiday.StartTime) || (q.LeaveTime <= holiday.EndTime && q.BackTime >= holiday.EndTime) || (q.LeaveTime <= holiday.StartTime && q.BackTime >= holiday.EndTime))).Where(q => q.StateLeave.ToString().Trim() == "2" && q.StateBack.ToString().Trim() == "1").ToList().Count();

                        HolidayLeaveListInfo data = new HolidayLeaveListInfo();
                        Dictionary<string, DataList> _ll_info_groupbyclass = new Dictionary<string, DataList>();
                        Dictionary<string, int> _ll_info_status = new Dictionary<string, int>();

                        _ll_info_status.Add("待审核", a);
                        _ll_info_status.Add("待销假", b);
                        _ll_info_status.Add("已销假", c);
                        _ll_info_status.Add("已驳回", d);

                        foreach (var item in _LL)
                        {
                            SelectCondition conditionsModel = new SelectCondition();
                            conditionsModel.sortField = "SubmitTime";
                            conditionsModel.sortDirection = "DESC";
                            DataList dtSource = GetList(conditionsModel, item.AsQueryable());
                            dtSource.list = TransformLL((List<vw_New_LeaveList>)dtSource.list);

                            _ll_info_groupbyclass.Add(item.Key, dtSource);
                        }

                        data.ll_info_groupbyclass = _ll_info_groupbyclass;
                        data.ll_info_status = _ll_info_status;
                        return Success("获取成功", data);
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

                    T_Holiday holiday = db.T_Holiday.Find(Convert.ToInt32(holidayID));
                    if (holiday != null && holiday.TeacherID == accountInfo.userID)
                    {
                        int a = 0;//未销假请假记录
                        int b = 0;//待审核请假记录

                        //列表中删除 此条记录
                        holiday.IsDelete = 1;
                        //清空所有请假
                        IQueryable<vw_New_LeaveList> _LL = db.vw_New_LeaveList.Where(q => q.LeaveType.ToString().Trim() == "4" && q.ST_TeacherID.ToString().Trim() == accountInfo.userID && ((q.LeaveTime <= holiday.StartTime && q.BackTime >= holiday.StartTime) || (q.LeaveTime <= holiday.EndTime && q.BackTime >= holiday.EndTime) || (q.LeaveTime <= holiday.StartTime && q.BackTime >= holiday.EndTime)) && q.StateBack.ToString().Trim() == "0");
                        IQueryable<vw_New_LeaveList> __LL = db.vw_New_LeaveList.Where(q => q.LeaveType.ToString().Trim() == "4" && q.ST_TeacherID.ToString().Trim() == accountInfo.userID && ((q.LeaveTime <= holiday.StartTime && q.BackTime >= holiday.StartTime) || (q.LeaveTime <= holiday.EndTime && q.BackTime >= holiday.EndTime) || (q.LeaveTime <= holiday.StartTime && q.BackTime >= holiday.EndTime)) && q.StateLeave.ToString().Trim() == "0");

                        if (_LL.Any() || __LL.Any())
                        {
                            if (_LL.Any())
                            {
                                a = _LL.Count();
                                foreach (var item in _LL)
                                {
                                    T_New_LeaveList _ll_model = db.T_New_LeaveList.Find(item.ID);
                                    _ll_model.StateBack = "1";
                                }
                            }
                            if (__LL.Any())
                            {
                                b = __LL.Count();
                                foreach (var item in __LL)
                                {
                                    T_New_LeaveList _ll_model = db.T_New_LeaveList.Find(item.ID);
                                    _ll_model.StateLeave = "1";
                                    _ll_model.StateBack = "1";
                                }
                            }
                            db.SaveChanges();
                            return Success("删除成功！在此期间的" + (a + b) + "条节假日请假记录已销假！");
                        }
                        else
                        {
                            db.SaveChanges();
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


                T_Holiday holidayModel = db.T_Holiday.Find(Convert.ToInt32(model.holidayID));
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

        #region 个人信息修改、密码修改、初始化学生密码
        /// <summary>
        /// 获取账户信息
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpGet, Route("userinfo")]
        public ApiResult GetUserInfo(string access_token)
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
                if (accountInfo.userRoleID == "1")//学生
                {
                    vw_Student model = db.vw_Student.Where(q => q.ST_Num.Trim() == accountInfo.userID).ToList().First();
                    if (model != null)
                    {
                        return Success("获取成功", model);
                    }
                    else
                    {
                        return Error("不包含此账号学生个人信息，请联系系统维护人员！");
                    }
                }
                else if (accountInfo.userRoleID == "3")//辅导员
                {
                    T_Teacher model = db.T_Teacher.Where(q => q.ID.Trim() == accountInfo.userID).ToList().First();
                    if (model != null)
                    {
                        TeacherInfo data = new TeacherInfo
                        {
                            teacherID = model.ID.ToString().Trim(),
                            teacherName = model.Name.ToString().Trim(),
                            grade = model.Grade.ToString().Trim(),
                            teacherTel = model.Tel.ToString().Trim(),
                            teacherEmail = model.Email.ToString().Trim()
                        };
                        return Success("获取成功", data);
                    }
                    else
                    {
                        return Error("不包含此账号辅导员个人信息，请联系系统维护人员！");
                    }
                }
                else
                {
                    return Error("班级账号不能修改个人信息！");
                }
            }
            catch (Exception ex)
            {
                return SystemError(ex);
            }
            #endregion
        }

        /// <summary>
        /// 学生修改个人信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("setstudentinfo")]
        public ApiResult SetUserInfo([FromBody]SetStudentInfo model)
        {
            #region 令牌验证
            result = Check(model.access_token, model.studentID);
            if (result != null)
            {
                return result;
            }
            #endregion

            #region 逻辑操作
            try
            {
                AccountInfo accountInfo = GetAccountInfo(model.access_token);
                T_Student studentModel = db.T_Student.Find(model.studentID);
                if (studentModel == null)
                {
                    return Error("数据库修改失败，请联系管理员！");
                }
                studentModel.Tel = model.tel.Trim();
                studentModel.Email = model.Email.Trim();
                studentModel.QQ = model.qq.Trim();
                studentModel.Room = model.dor.Trim();
                studentModel.Sex = model.sex.Trim();
                studentModel.ContactOne = model.contactRelation + "-" + model.contactName.Trim();
                studentModel.OneTel = model.contactTel.Trim();

                db.SaveChanges();
                return Success("修改成功！");
            }
            catch (Exception ex)
            {
                return SystemError(ex);
            }
            #endregion
        }

        /// <summary>
        /// 辅导员修改个人信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("setteacherinfo")]
        public ApiResult SetUserInfo([FromBody]SetTeacherInfo model)
        {
            #region 令牌验证
            result = Check(model.access_token, model.teacherID);
            if (result != null)
            {
                return result;
            }
            #endregion

            #region 逻辑操作
            try
            {
                AccountInfo accountInfo = GetAccountInfo(model.access_token);
                T_Teacher teacherModel = db.T_Teacher.Find(model.teacherID);
                if (teacherModel == null)
                {
                    return Error("数据库修改失败，请联系管理员！");
                }
                teacherModel.Name = model.teacherName;
                teacherModel.Tel = model.teacherTel.Trim();
                teacherModel.Email = model.teacherEmail.Trim();

                db.SaveChanges();
                return Success("修改成功！");
            }
            catch (Exception ex)
            {
                return SystemError(ex);
            }
            #endregion
        }

        /// <summary>
        /// 修改个人账户密码
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("password")]
        public ApiResult SetPassWord([FromBody]PassWord model)
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
                if (model == null)
                {
                    return Error("参数格式错误或缺少参数！");
                }
                if (model.old_psd == "" || model.old_psd == null || model.new_psd == "" || model.new_psd == null)
                {
                    return Error("参数格式错误或缺少参数！");
                }
                if (model.new_psd.Length < 6)
                {
                    return Error("新密码必须大于等于6位！");
                }

                AccountInfo accountInfo = GetAccountInfo(model.access_token);
                string encryptionString = PsdEncryption.Encryption(model.old_psd);
                if (db.T_Account.Where(c => c.ID == accountInfo.userID && c.Psd == encryptionString).Any())
                {
                    db.T_Account.Where(c => c.ID == accountInfo.userID).Update(c => new T_Account() { Psd = PsdEncryption.Encryption(model.new_psd) });
                    db.SaveChanges();

                    return Success("修改密码成功！");
                }
                else
                {
                    return Error("原密码错误！");
                }
            }
            catch (Exception ex)
            {
                return SystemError(ex);
            }
            #endregion
        }

        /// <summary>
        /// 初始化学生账号密码
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="studentID"></param>
        /// <returns></returns>
        [HttpGet, Route("initialpwd")]
        public ApiResult InitialPassWord(string access_token, string studentID)
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
                if (db.vw_Student.Where(q => q.ST_TeacherID.Trim() == accountInfo.userID && q.ST_Num.Trim() == studentID).ToList().Any())
                {
                    T_Account accountModel = db.T_Account.Find(studentID);
                    string _psd = studentID.Substring(studentID.Length - 6, 6);
                    string psd = PsdEncryption.Encryption(_psd);
                    accountModel.Psd = psd;
                    db.SaveChanges();

                    return Success("密码初始化成功！初始化密码为学号后六位，请尽快修改！");
                }
                else
                {
                    return Error("数据错误");
                }
            }
            catch (Exception ex)
            {
                return SystemError(ex);
            }
            #endregion
        }
        #endregion
    }
}