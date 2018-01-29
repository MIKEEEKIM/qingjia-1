using qingjia_MVC.Common;
using qingjia_MVC.Models;
using qingjia_MVC.Models.API;
using System;
using System.Collections.Generic;
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
            OutputLog.WriteLog("访问API接口 " + HttpContext.Current.Request.Path);
            #endregion
        }

        /// <summary>
        ///  API 返回结果
        /// </summary>
        public ApiResult result;

        /// <summary>
        ///  实例化数据库连接
        /// </summary>
        public imaw_qingjiaEntities db = new imaw_qingjiaEntities();

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
        public static List<T> GetList<T>(SelectCondition model, IQueryable<T> returnList)
        {
            if (model.conditions != null)
            {
                foreach (var item in model.conditions)
                {
                    returnList = returnList.Where(GetConditionExpression<T>(item));
                }
            }
            return GetList(returnList, model.page, model.limit, model.sortDirection, model.sortField);
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
        #endregion

        #region 数据模型转换

        #endregion
    }
}