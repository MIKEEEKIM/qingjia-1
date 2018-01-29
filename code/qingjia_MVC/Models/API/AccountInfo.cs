using System.Collections.Generic;

namespace qingjia_MVC.Models.API
{
    public class AccountInfo
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string userID { get; set; }

        /// <summary>
        /// 账户名
        /// </summary>
        public string userName { get; set; }

        /// <summary>
        /// 用户角色ID
        /// </summary>
        public string userRoleID { get; set; }

        /// <summary>
        /// 用户角色姓名
        /// </summary>
        public string userRoleName { get; set; }

        /// <summary>
        /// 用户所属组织ID
        /// </summary>
        public string Grade { get; set; }

        /// <summary>
        /// 授权令牌
        /// </summary>
        public string access_token { get; set; }

        /// <summary>
        /// 操作权限列表
        /// </summary>
        public List<string> permissionList { set; get; }
    }
}