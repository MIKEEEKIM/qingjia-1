using qingjia_MVC.Models;
using qingjia_MVC.Models.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace qingjia_MVC.Controllers.API.AddressList
{
    [RoutePrefix("api/addresslist")]
    public class AddressListController : BaseApiController
    {
        /// <summary>
        /// Get 获取学生通讯录
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet, Route("get")]
        public ApiResult Get(string access_token, int page, int pageSize)
        {
            #region 令牌验证
            result = Check(access_token);
            if (result != null)
            {
                return result;
            }
            #endregion

            #region 逻辑操作
            //判断角色类型 返回数据
            try
            {
                AccountInfo accountInfo = GetAccountInfo(access_token);
                IQueryable<vw_Student> studentList = db.vw_Student;
                SelectCondition conditionsModel = new SelectCondition();
                if (accountInfo.userRoleID == "1")
                {
                    //学生
                    vw_Student studentModel = db.vw_Student.Where(c => c.ST_Num == accountInfo.userID).ToList().First();
                    studentList = db.vw_Student.Where(c => c.ST_Grade.ToString().Trim() == accountInfo.Grade.ToString().Trim() && c.ST_Class.ToString().Trim() == studentModel.ST_Class.ToString().Trim());
                }
                else if (accountInfo.userRoleID == "2")
                {
                    //班级账号
                    vw_Class classModel = db.vw_Class.Where(c => c.ID == accountInfo.userID).ToList().First();
                    studentList = db.vw_Student.Where(c => c.ST_Grade.ToString().Trim() == accountInfo.Grade.ToString().Trim() && c.ST_Class.ToString().Trim() == classModel.ClassName.ToString().Trim());
                }
                else if (accountInfo.userRoleID == "3")
                {
                    //辅导员
                    studentList = db.vw_Student.Where(c => c.ST_Grade.ToString().Trim() == accountInfo.Grade.ToString().Trim() && c.ST_TeacherID.ToString().Trim() == accountInfo.userID.ToString().Trim());
                }

                studentList = studentList.OrderBy(c => c.ST_Class).ThenBy(c => c.ST_Sex).ThenBy(c => c.ST_Num);

                if (page != 0 && pageSize != 0)
                {
                    conditionsModel.page = page;
                    conditionsModel.pageSize = pageSize;
                }

                DataList dtSource = GetList(conditionsModel, studentList);
                dtSource.list = TransformStudentList((List<vw_Student>)dtSource.list, accountInfo.userRoleID);

                return Success("获取成功", dtSource);
            }
            catch (Exception ex)
            {
                return SystemError();
            }
            #endregion
        }

        /// <summary>
        /// Post 获取学生通讯录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("post")]
        public ApiResult Post([FromBody]SelectCondition model)
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
                IQueryable<vw_Student> studentList = db.vw_Student;
                if (accountInfo.userRoleID == "1")
                {
                    //学生
                    vw_Student studentModel = db.vw_Student.Where(c => c.ST_Num == accountInfo.userID).ToList().First();
                    studentList = db.vw_Student.Where(c => c.ST_Grade.ToString().Trim() == accountInfo.Grade.ToString().Trim() && c.ST_Class.ToString().Trim() == studentModel.ST_Class.ToString().Trim());
                }
                else if (accountInfo.userRoleID == "2")
                {
                    //班级账号
                    vw_Class classModel = db.vw_Class.Where(c => c.ID == accountInfo.userID).ToList().First();
                    studentList = db.vw_Student.Where(c => c.ST_Grade.ToString().Trim() == accountInfo.Grade.ToString().Trim() && c.ST_Class.ToString().Trim() == classModel.ClassName.ToString().Trim());
                }
                else if (accountInfo.userRoleID == "3")
                {
                    //辅导员
                    studentList = db.vw_Student.Where(c => c.ST_Grade.ToString().Trim() == accountInfo.Grade.ToString().Trim() && c.ST_TeacherID.ToString().Trim() == accountInfo.userID.ToString().Trim());
                }

                studentList = studentList.OrderBy(c => c.ST_Class).ThenBy(c => c.ST_Sex).ThenBy(c => c.ST_Num);

                DataList dtSource = GetList(model, studentList);
                dtSource.list = TransformStudentList((List<vw_Student>)dtSource.list, accountInfo.userRoleID);

                return Success("获取成功", dtSource);
            }
            catch
            {
                return SystemError();
            }
            #endregion
        }
    }
}
