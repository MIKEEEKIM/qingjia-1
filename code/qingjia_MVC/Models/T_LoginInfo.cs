//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace qingjia_MVC.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class T_LoginInfo
    {
        public int ID { get; set; }
        public string userID { get; set; }
        public string userName { get; set; }
        public Nullable<int> roleID { get; set; }
        public string roleName { get; set; }
        public string grade { get; set; }
        public Nullable<int> loginPort { get; set; }
        public Nullable<System.DateTime> loginTime { get; set; }
    }
}
