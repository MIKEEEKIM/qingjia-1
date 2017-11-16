﻿using System.Collections.Generic;

namespace qingjia_MVC.Common
{
    public class ApiBaseResult
    {
        /// <summary>
        /// 返回结果success或error
        /// </summary>
        public string result { get; set; }
        /// <summary>
        /// 失败原因
        /// </summary>
        public string messages { get; set; }//public List<string> messages { get; set; }
        /// <summary>
        /// 字段格式错误信息
        /// </summary>
        public Dictionary<string, string> fieldErrors { get; set; }
        /// <summary>
        /// 错误码
        /// </summary>
        public string errors { get; set; }//public List<string> errors { get; set; }        
        /// <summary>
        /// 返回结果
        /// </summary>
        public object data { get; set; }
    }
}