using qingjia_MVC.Common;
using qingjia_MVC.Models;
using qingjia_MVC.Models.API;
using qingjia_MVC.Models.API.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Http;

namespace qingjia_MVC.Controllers.API
{
    public class BaseApiController : ApiController
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        public BaseApiController()
        {
            #region 系统沉睡0.5秒
            //测试状态 沉睡0.5秒
            if (System.Configuration.ConfigurationManager.AppSettings["IsTest"].ToString() == "1")
            {
                //线程 暂停0.5秒
                System.Threading.Thread.Sleep(500);
            }
            #endregion

            #region 访问日志
            //OutputLog.WriteLog("访问API接口 " + HttpContext.Current.Request.Path);
            #endregion

            #region 测试状态模拟登陆
            AccountInfo accountInfo = new AccountInfo();
            accountInfo.access_token = "11";
            accountInfo.userID = "1214001";
            accountInfo.userName = "王健铭";
            accountInfo.userRoleID = "3";
            accountInfo.userRoleName = "辅导员";
            accountInfo.Grade = "2014";
            accountInfo.permissionList = null;

            HttpRuntime.Cache.Insert(accountInfo.access_token, accountInfo, null, DateTime.MaxValue, TimeSpan.FromMinutes(Convert.ToInt32(ConfigurationManager.AppSettings["CacheSpanTime"].ToString())));
            #endregion
        }

        /// <summary>
        ///  API 返回结果
        /// </summary>
        public ApiResult result;

        /// <summary>
        ///  实例化数据库连接
        /// </summary>
        public static Entities db = new Entities();

        #region 返回ApiResult 方法
        /// <summary>
        /// 出现未知程序错误、联系管理员
        /// </summary>
        /// <returns></returns>
        public static ApiResult SystemError()
        {
            ApiResult result = new ApiResult();
            result.status = "error";
            result.messages = "操作失败、出现未知程序错误、请联系管理员！";
            result.fieldErrors = null;
            return result;
        }

        /// <summary>
        /// 令牌过期是返回错误
        /// </summary>
        /// <returns></returns>
        public static ApiResult Error()
        {
            ApiResult result = new ApiResult();
            result.status = "error";
            result.messages = "令牌已过期，请重新登录！";
            result.fieldErrors = null;
            return result;
        }

        /// <summary>
        ///  发生错误 返回错误信息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ApiResult Error(string message)
        {
            ApiResult result = new ApiResult();
            result.status = "error";
            result.messages = message;
            return result;
        }

        /// <summary>
        ///  发生错误 返回错误信息及错误字段
        /// </summary>
        /// <param name="message"></param>
        /// <param name="fieldErrors"></param>
        /// <returns></returns>
        public static ApiResult Error(string message, Dictionary<string, string> fieldErrors)
        {
            ApiResult result = new ApiResult();
            result.status = "error";
            result.messages = message;
            result.fieldErrors = fieldErrors;
            return result;
        }

        /// <summary>
        ///  操作成功 返回信息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ApiResult Success(string message)
        {
            ApiResult result = new ApiResult();
            result.status = "success";
            result.messages = message;
            return result;
        }

        /// <summary>
        ///  操作成功 返回信息及数据
        /// </summary>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ApiResult Success(string message, object data)
        {
            ApiResult result = new ApiResult();
            result.status = "success";
            result.messages = message;
            result.data = data;
            return result;
        }
        #endregion

        #region Access_Token 相关方法
        //此模块包含：令牌验证等相关操作

        /// <summary>
        /// 令牌验证
        /// 验证是否存在该令牌
        /// 验证该令牌是否具有该操作权限
        /// </summary>
        /// <param name="access_token">授权令牌</param>
        /// <returns></returns>
        public static ApiResult Check(string access_token)
        {
            #region 测试代码--测试401错误，默认返回ApiResult
            //判断是否处于测试状态，1代表测试状态，其他代表非测试状态
            if (ConfigurationManager.AppSettings["IsTest401"].ToString() == "1")
            {
                //强制返回401错误
                ForceHttpStatusCodeResult.SetForceHttpUnauthorizedHeader();

                ApiResult result_401 = new ApiResult();
                result_401.status = "error";
                result_401.messages = "发生401错误（处于测试模式）";

                return result_401;
            }
            #endregion

            #region 测试代码--跳过令牌验证，返回null
            //判断是否处于测试状态，1代表测试状态，其他代表非测试状态
            if (System.Configuration.ConfigurationManager.AppSettings["IsTest"].ToString() == "1")
            {
                //返回null  代表验证通过
                return null;
            }
            #endregion

            #region 令牌验证代码
            //获取访问路径
            string path = HttpContext.Current.Request.Path.ToString().Trim();

            ApiResult result = new ApiResult();
            try
            {
                AccountInfo userInfo = (AccountInfo)HttpRuntime.Cache.Get(access_token);
                if (HttpRuntime.Cache.Get(access_token) != null)
                {
                    List<string> permissionList = userInfo.permissionList;
                    if (permissionList.IndexOf(path) == -1)
                    {
                        Dictionary<string, string> rolePermission = (Dictionary<string, string>)HttpRuntime.Cache.Get("rolePermission");
                        throw new Exception("该账户缺少权限，接口名称：" + rolePermission[path].ToString());
                    }
                    else
                    {
                        //返回null 代表令牌验证成功
                        return null;
                    }
                }
                else
                {
                    throw new Exception("AccessToken错误，尚未授权或授权已过期");
                }
            }
            catch (Exception e)
            {
                //强制返回401错误
                ForceHttpStatusCodeResult.SetForceHttpUnauthorizedHeader();
                result.status = "error";
                result.messages = e.Message;
                return result;
            }
            #endregion
        }

        /// <summary>
        /// 验证是否为本人操作
        /// 令牌验证
        /// 验证是否存在该令牌
        /// 验证该令牌是否具有该操作权限
        /// </summary>
        /// <param name="access_token">授权令牌</param>
        /// <param name="AccountID">操作人ID</param>
        /// <returns></returns>
        public static ApiResult Check(string access_token, string AccountID)
        {
            #region 测试代码--测试401错误，默认返回ApiResult
            //判断是否处于测试状态，1代表测试状态，其他代表非测试状态
            if (System.Configuration.ConfigurationManager.AppSettings["IsTest401"].ToString() == "1")
            {
                //强制返回401错误
                ForceHttpStatusCodeResult.SetForceHttpUnauthorizedHeader();

                ApiResult result_401 = new ApiResult();
                result_401.status = "error";
                result_401.messages = "发生401错误（处于测试模式）";

                return result_401;
            }
            #endregion

            #region 测试代码
            //判断是否处于测试状态，1代表测试状态，其他代表非测试状态
            if (System.Configuration.ConfigurationManager.AppSettings["IsTest"].ToString() == "1")
            {
                //返回null  代表验证通过
                return null;
            }
            #endregion

            #region 令牌验证代码
            //获取访问路径
            string path = HttpContext.Current.Request.Path.ToString().Trim();

            ApiResult result = new ApiResult();
            try
            {
                AccountInfo userInfo = (AccountInfo)HttpRuntime.Cache.Get(access_token);
                if (userInfo != null)
                {
                    List<string> permissionList = userInfo.permissionList;
                    if (permissionList.IndexOf(path) == -1)
                    {
                        Dictionary<string, string> rolePermission = (Dictionary<string, string>)HttpRuntime.Cache.Get("rolePermission");
                        throw new Exception("该账户缺少权限，权限名称：" + rolePermission[path].ToString());
                    }
                    else
                    {
                        if (AccountID == userInfo.userID)
                        {
                            return null;
                        }
                        else
                        {
                            throw new Exception("AccessToken错误，此接口不能修改他人账户");
                        }
                    }
                }
                else
                {
                    throw new Exception("AccessToken错误，尚未授权或授权已过期");
                }
            }
            catch (Exception e)
            {
                //强制返回401错误
                ForceHttpStatusCodeResult.SetForceHttpUnauthorizedHeader();
                result.status = "error";
                result.messages = e.Message;
                return result;
            }
            #endregion
        }

        /// <summary>
        /// 注销AccessToken
        /// </summary>
        /// <param name="access_token">授权令牌</param>
        /// <returns></returns>
        public static ApiResult Clear(string access_token)
        {
            ApiResult result = new ApiResult();
            if (HttpRuntime.Cache.Remove(access_token) != null)
            {
                result.status = "success";
                result.messages = "注销成功";
            }
            else
            {
                result.status = "error";
                result.messages = "令牌无效";
            }
            return result;
        }

        /// <summary>
        /// 获取令牌帐号信息
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        public static AccountInfo GetAccountInfo(string access_token)
        {
            #region 测试代码
            //判断是否处于测试状态，1代表测试状态，其他代表非测试状态
            if (System.Configuration.ConfigurationManager.AppSettings["IsTest"].ToString() == "1")
            {
                //返回测试信息  代表验证通过
                AccountInfo _userInfo = (AccountInfo)HttpRuntime.Cache.Get(access_token);
                return _userInfo;
            }
            #endregion

            #region 根据令牌获取用户信息
            AccountInfo userInfo = new AccountInfo();
            userInfo = (AccountInfo)HttpRuntime.Cache.Get(access_token);
            if (HttpRuntime.Cache.Get(access_token) != null)
            {
                return userInfo;
            }
            else
            {
                return null;
            }
            #endregion
        }
        #endregion

        #region 数据库 查询 逻辑 (此处不包含业务逻辑)
        /// <summary>
        /// 排序 分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="returnList"></param>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <param name="sortDirection"></param>
        /// <param name="sortField"></param>
        /// <returns></returns>
        private static List<T> GetList<T>(IQueryable<T> returnList, int page, int limit, string sortDirection, string sortField)
        {
            if (sortDirection != null && sortField != null && (sortDirection == "ASC" || sortDirection == "DESC"))
            {
                if (typeof(T).GetProperty(sortField) != null)
                {
                    #region 动态排序

                    var property = typeof(T).GetProperty(sortField);
                    var parameter = Expression.Parameter(typeof(T), "p");
                    var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                    var orderByExp = Expression.Lambda(propertyAccess, parameter);

                    if (sortDirection == "ASC")//正序
                    {
                        MethodCallExpression resultExp = Expression.Call(typeof(Queryable), "OrderBy", new Type[] { typeof(T), property.PropertyType }, returnList.Expression, Expression.Quote(orderByExp));
                        returnList = (IOrderedQueryable<T>)returnList.Provider.CreateQuery<T>(resultExp);
                    }
                    if (sortDirection == "DESC")//倒序
                    {
                        MethodCallExpression resultExp = Expression.Call(typeof(Queryable), "OrderByDescending", new Type[] { typeof(T), property.PropertyType }, returnList.Expression, Expression.Quote(orderByExp));
                        returnList = (IOrderedQueryable<T>)returnList.Provider.CreateQuery<T>(resultExp);
                    }
                    #endregion
                }
            }

            if (page > 0 && limit > 0)
            {
                return returnList.Skip((page - 1) * limit).Take(limit).ToList();
            }
            else
            {
                return returnList.ToList();
            }
        }

        //构建表达式树
        private static Expression<Func<T, bool>> GetConditionExpression<T>(ConditionModel model)
        {
            ParameterExpression left = Expression.Parameter(typeof(T), "c");
            Expression expression = Expression.Constant(false);
            if (typeof(T).GetProperty(model.fieldName) != null)
            {
                string fieldName = model.fieldName;
                foreach (var optionName in model.fieldValues)
                {
                    Expression right = Expression.Call(Expression.Property(left, typeof(T).GetProperty(fieldName)), typeof(string).GetMethod("Contains", new Type[] { typeof(string) }), Expression.Constant(optionName.item.ToString().Trim()));
                    expression = Expression.Or(right, expression);
                }
            }
            Expression<Func<T, bool>> finalExpression = Expression.Lambda<Func<T, bool>>(expression, new ParameterExpression[] { left });
            return finalExpression;
        }

        /// <summary>
        /// 查询 数据库 支持多条件、排序、分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="returnList"></param>
        /// <returns></returns>
        public static DataList GetList<T>(SelectCondition model, IQueryable<T> returnList)
        {
            DataList result = new DataList();
            if (model.conditions != null)
            {
                foreach (var item in model.conditions)
                {
                    returnList = returnList.Where(GetConditionExpression<T>(item));
                }
                result.total = returnList.Count();
            }
            result.list = GetList(returnList, model.page, model.pageSize, model.sortDirection, model.sortField);
            return result;
        }

        /// <summary>
        /// 拼装查询条件 单值检索
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        public static ConditionModel CreatCondition(string fieldName, string fieldValue)
        {
            if (fieldName == "" || fieldName == null || fieldValue == "" || fieldValue == null)
            {
                return null;
            }
            ConditionModel model = new ConditionModel();
            model.fieldName = fieldName;
            FieldValue valueItem = new FieldValue();
            valueItem.item = fieldValue.Trim();
            model.fieldValues.Add(valueItem);
            return model;
        }

        /// <summary>
        /// 拼装查询条件 多值检索
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        public static ConditionModel CreatCondition(string fieldName, List<string> fieldValue)
        {
            if (fieldName == "" || fieldName == null || fieldValue == null || fieldName.Count() == 0)
            {
                return null;
            }
            ConditionModel model = new ConditionModel();
            model.fieldName = fieldName;
            foreach (string item in fieldValue)
            {
                FieldValue valueItem = new FieldValue();
                valueItem.item = item.Trim();
                model.fieldValues.Add(valueItem);
            }
            return model;
        }

        /// <summary>
        /// 拼装查询条件 多值检索
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        public static ConditionModel CreatCondition(string fieldName, List<FieldValue> fieldValue)
        {
            if (fieldName == "" || fieldName == null || fieldValue == null || fieldName.Count() == 0)
            {
                return null;
            }
            ConditionModel model = new ConditionModel();
            model.fieldName = fieldName;
            model.fieldValues = fieldValue;
            return model;
        }

        public static vw_New_LeaveList GetLeaveListModel(string LL_ID)
        {
            var _result = from vw_New_LeaveList in db.vw_New_LeaveList where (vw_New_LeaveList.ID == LL_ID && vw_New_LeaveList.IsDelete == 0) select vw_New_LeaveList;
            if (_result.Any() && _result.ToList().Count() == 1)
            {
                return _result.ToList().First();
            }
            return null;
        }
        #endregion

        #region 数据模型转换
        /// <summary>
        /// 此处方法用于 数据库视图转换成 用户使用数据
        /// </summary>
        /// <param name="_model_list"></param>
        /// <returns></returns>
        public static List<LeaveListModel> TransformLL(List<vw_New_LeaveList> _model_list)
        {
            List<LeaveListModel> model_list = new List<LeaveListModel>();
            foreach (var item in _model_list)
            {
                LeaveListModel model = new LeaveListModel();

                #region
                model.LL_ID = item.ID;
                model.studentID = item.StudentID;
                model.reason = item.Reason;
                model.leaveState = "Error";
                model.leaveStateCode = "Error";
                if (item.StateLeave == "0" && item.StateBack == "0")
                {
                    model.leaveState = "待审核";
                    model.leaveStateCode = "1";
                }
                if (item.StateLeave == "1" && item.StateBack == "0")
                {
                    model.leaveState = "待销假";
                    model.leaveStateCode = "2";
                }
                if (item.StateLeave == "1" && item.StateBack == "1")
                {
                    model.leaveState = "已销假";
                    model.leaveStateCode = "3";
                }
                if (item.StateLeave == "2" && item.StateBack == "1")
                {
                    model.leaveState = "已驳回";
                    model.leaveStateCode = "4";
                }
                model.rejectReason = item.RejectReason;
                model.leaveTypeID = item.LeaveType.ToString();
                model.leaveTypeName = "Error";
                if (item.LeaveType == "1" || item.LeaveType == "2" || item.LeaveType == "3" || item.LeaveType == "4")
                {
                    model.leaveTypeName = item.LeaveTypeName;
                }
                if (item.LeaveType == "5" || item.LeaveType == "6" || item.LeaveType == "7")
                {
                    if (item.LeaveTypeChildrenID == "1")
                    {
                        model.leaveTypeName = item.LeaveTypeName + "（公假）";
                    }
                    if (item.LeaveTypeChildrenID == "2")
                    {
                        model.leaveTypeName = item.LeaveTypeName + "（事假）";
                    }
                    if (item.LeaveTypeChildrenID == "3")
                    {
                        model.leaveTypeName = item.LeaveTypeName + "（病假）";
                    }
                }
                model.leaveTime = item.LeaveTime.ToString();
                model.backTime = item.BackTime.ToString();
                model.leaveWay = item.LeaveWay;
                model.backWay = item.BackWay;
                model.address = item.Address;
                model.lesson = "Error";
                if (item.Lesson == "1")
                {
                    model.lesson = "第一大节";
                }
                if (item.Lesson == "2")
                {
                    model.lesson = "第二大节";
                }
                if (item.Lesson == "3")
                {
                    model.lesson = "第三大节";
                }
                if (item.Lesson == "4")
                {
                    model.lesson = "第四大节";
                }
                if (item.Lesson == "5")
                {
                    model.lesson = "第五大节";
                }
                model.teacher = item.Teacher;
                model.auditTeacher = item.AuditTeacher;
                model.ST_Name = item.ST_Name;
                model.ST_Tel = item.ST_Tel;
                model.ST_Class = item.ST_Class;
                model.ST_Grade = item.ST_Grade;
                model.ST_TeacherName = item.ST_Teacher;
                model.intershipCompany = item.IntershipCompany;
                model.intershipAddress = item.IntershipAddress;
                model.principalName = item.PrincipalName;
                model.principalTel = item.PrincipalTel;
                model.pic_one = item.Pic_One;
                model.pic_two = item.Pic_Two;
                model.pic_three = item.Pic_Three;
                #endregion

                model_list.Add(model);
            }
            return model_list;
        }
        #endregion
    }
}