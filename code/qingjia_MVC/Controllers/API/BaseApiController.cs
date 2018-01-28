using qingjia_MVC.Common;
using qingjia_MVC.Models;
using qingjia_MVC.Models.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
            OutputLog.WriteLog("访问API接口 " + RequestContext.Url);
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
        #endregion

        #region 数据模型转换

        #endregion
    }
}