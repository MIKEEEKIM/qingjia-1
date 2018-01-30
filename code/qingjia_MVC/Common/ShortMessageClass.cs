using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.Core.Profile;
using Aliyun.Acs.Sms.Model.V20160927;
using qingjia_MVC.Models;
using System;
using System.Configuration;

namespace qingjia_MVC.Common
{
    public class ShortMessageClass
    {
        //实例化数据模型
        private static imaw_qingjiaEntities db = new imaw_qingjiaEntities();

        //密码验证：SMS_60140885
        //请假失败：SMS_27620081
        //离校请假成功：SMS_27325377
        //请假成功：SMS_107115105

        /// <summary>
        /// 发送通知短信
        /// </summary>
        /// <param name="ST_Name">学生姓名</param>
        /// <param name="LV_Num">请假单号</param>
        /// <param name="ST_Tel">电话号码</param>
        /// <param name="MessageType">短信类型</param>
        /// <returns></returns>
        public static bool SendShortMessage(MessageModel model)
        {
            if (ConfigurationManager.AppSettings["ShortMessageService"].ToString().Trim() != "1")
            {
                if (ConfigurationManager.AppSettings["ShortMessageService"].ToString().Trim() == "-1")
                {
                    //测试人员手机号
                    model.ST_Tel = ConfigurationManager.AppSettings["Tel"].ToString().Trim();
                }
                else
                {
                    //非1 非-1 代表关闭服务
                    return false;
                }
            }

            if (model.LV_Num == null || model.ST_Name == null || model.ST_Tel == null || model.MessageType == null)
            {
                return false;
            }


            //AccessKey 和 AccessKeyCode
            IClientProfile profile = DefaultProfile.GetProfile("cn-hangzhou", "LTAI7W5SRT92SGZD", "F7Gv1zZvwHYHLbkSIXnn1Dx9HUIi0K");
             IAcsClient client = new DefaultAcsClient(profile);
            SingleSendSmsRequest request = new SingleSendSmsRequest();
            try
            {
                //短信签名  【请假系统】
                request.SignName = "请假系统";

                if (model.MessageType == "go" && model.picurl != "")
                {
                    //需要打印请假条  附带请假系统域名地址（短信模板尚未申请成功）
                    request.TemplateCode = "SMS_110095016";
                    request.ParamString = "{\"name\":\"" + model.ST_Name + "\",\"lvnum\":\"" + model.LV_Num + "\",\"picurl\":\"" + model.picurl + "\"}";
                    //{\"  \":\"  \",\"  \":\"  \",\"  \":\"  \"}
                }
                else if (model.MessageType == "go" && model.picurl == "")
                {
                    //请假成功模板 无需打印假条
                    request.TemplateCode = "SMS_107115105";
                    request.ParamString = "{\"name\":\"" + model.ST_Name + "\",\"lvnum\":\"" + model.LV_Num + "\"}";
                }
                else if (model.MessageType == "back")
                {
                    //销假成功模板
                    request.TemplateCode = "SMS_27495348";
                    request.ParamString = "{\"name\":\"" + model.ST_Name + "\",\"lvnum\":\"" + model.LV_Num + "\"}";
                }
                else if (model.MessageType == "failed")
                {
                    //驳回请假模板
                    request.TemplateCode = "SMS_27620081";
                    request.ParamString = "{\"name\":\"" + model.ST_Name + "\",\"lvnum\":\"" + model.LV_Num + "\"}";
                }
                else if (model.MessageType == "FindPsd")
                {
                    //短信验证找回密码
                    request.TemplateCode = "SMS_60140885";
                    request.ParamString = "{\"name\":\"" + model.ST_Name + "\",\"lvnum\":\"" + model.LV_Num + "\"}";
                }
                else
                {
                    return false;
                }

                request.RecNum = model.ST_Tel;

                SingleSendSmsResponse httpResponse = client.GetAcsResponse(request);

                SaveMessageList(model.ST_Num, model.LV_Num, model.ST_Tel, model.MessageType);

                return true;
            }
            catch (ServerException e)
            {
                return false;
            }
            catch (ClientException e)
            {
                return false;
            }
        }

        /// <summary>
        /// 将发送的短信内容保存至数据库
        /// </summary>
        /// <param name="ST_NUM"></param>
        /// <param name="LV_Num"></param>
        /// <param name="ST_Tel"></param>
        /// <param name="MessageType"></param>
        /// <returns></returns>
        private static bool SaveMessageList(string ST_Num, string LV_Num, string ST_Tel, string MessageType)
        {
            T_SendList LL = new T_SendList();
            LL.LV_Num = LV_Num;
            LL.ST_Num = ST_Num;
            LL.MessageType = MessageType;
            LL.ST_Tel = ST_Tel;
            LL.timeString = DateTime.Now;
            db.T_SendList.Add(LL);
            try
            {
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// 短信模板
    /// </summary>
    //public class MessageModel
    //{
    //    //学生姓名
    //    public string ST_Name { get; set; }

    //    //学生学号
    //    public string ST_Num { get; set; }

    //    //请假单号
    //    public string LV_Num { get; set; }

    //    //电话号码
    //    public string ST_Tel { get; set; }

    //    //链接网址
    //    public string picurl { get; set; }

    //    //短信类型  go/back/failed
    //    public string MessageType { get; set; }
    //}
}