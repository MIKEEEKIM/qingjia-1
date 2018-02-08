using System.Collections.Generic;

namespace qingjia_MVC.Models.API
{
    public class SelectCondition
    {
        //授权令牌
        public string access_token { get; set; }

        //记录类型 0代表 同时查询荣誉信息记录和竞赛获奖记录 1代表查询荣誉记录 2代表查询竞赛获奖记录
        public string leaveTypeID { get; set; }

        //请假状态 0 代表全部请假 1 代表待审核请假 2 代表待销假请假 3 代表已销假 4 代表已驳回请假
        public string state { get; set; }

        //页码
        public int page { get; set; }

        //每页条数
        public int pageSize { get; set; }

        //排序方式 ASC 或 DESC
        public string sortDirection { get; set; }

        //排序字段
        public string sortField { get; set; }

        //查询条件集合
        public List<ConditionModel> conditions = new List<ConditionModel>();
    }

    //字段 查询条件
    public class ConditionModel
    {
        public string fieldName { get; set; }
        public List<FieldValue> fieldValues = new List<FieldValue>();
    }

    //字段 查询条件值
    public class FieldValue
    {
        public string item { get; set; }
    }
}