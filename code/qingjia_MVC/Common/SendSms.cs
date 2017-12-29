using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.Core.Profile;
using Aliyun.Acs.Dysmsapi.Model.V20170525;
using qingjia_MVC.Models;
using System;
using System.Configuration;

namespace qingjia_MVC.Common
{
    public class SendSms
    {
        //产品名称:云通信短信API产品,开发者无需替换
        const string product = "Dysmsapi";
        //产品域名,开发者无需替换
        const string domain = "dysmsapi.aliyuncs.com";

        // TODO 此处需要替换成开发者自己的AK(在阿里云访问控制台寻找)
        const string accessKeyId = "LTAI7W5SRT92SGZD";
        const string accessKeySecret = "F7Gv1zZvwHYHLbkSIXnn1Dx9HUIi0K";

        //实例化数据模型
        private static imaw_qingjiaEntities db = new imaw_qingjiaEntities();

        //密码验证：SMS_60140885
        //请假失败：SMS_27620081
        //离校请假成功：SMS_27325377
        //请假成功：SMS_107115105

        public static SendSmsResponse sendSms(MessageModel model)
        {
            #region 参数验证
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
                    return null;
                }
            }

            if (model.LV_Num == null || model.ST_Name == null || model.ST_Tel == null || model.MessageType == null)
            {
                return null;
            }
            #endregion

            IClientProfile profile = DefaultProfile.GetProfile("cn-hangzhou", accessKeyId, accessKeySecret);
            DefaultProfile.AddEndpoint("cn-hangzhou", "cn-hangzhou", product, domain);
            IAcsClient acsClient = new DefaultAcsClient(profile);
            SendSmsRequest request = new SendSmsRequest();
            SendSmsResponse response = null;
            try
            {
                //必填:待发送手机号。支持以逗号分隔的形式进行批量调用，批量上限为1000个手机号码,批量调用相对于单条调用及时性稍有延迟,验证码类型的短信推荐使用单条调用的方式
                request.PhoneNumbers = model.ST_Tel;
                //必填:短信签名-可在短信控制台中找到
                request.SignName = "请假系统";

                //必填:短信模板-可在短信控制台中找到
                //request.TemplateCode = "";
                //可选:模板中的变量替换JSON串,如模板内容为"亲爱的${name},您的验证码为${code}"时,此处的值为
                //request.TemplateParam = "";
                if (model.MessageType == "go" && model.picurl != "")
                {
                    //需要打印请假条  附带请假系统域名地址（短信模板尚未申请成功）
                    request.TemplateCode = "SMS_110095016";
                    request.TemplateParam = "{\"name\":\"" + model.ST_Name + "\",\"lvnum\":\"" + model.LV_Num + "\",\"picurl\":\"" + model.picurl + "\"}";
                    //{\"  \":\"  \",\"  \":\"  \",\"  \":\"  \"}
                }
                else if (model.MessageType == "go" && model.picurl == "")
                {
                    //请假成功模板 无需打印假条
                    request.TemplateCode = "SMS_107115105";
                    request.TemplateParam = "{\"name\":\"" + model.ST_Name + "\",\"lvnum\":\"" + model.LV_Num + "\"}";
                }
                else if (model.MessageType == "back")
                {
                    //销假成功模板
                    request.TemplateCode = "SMS_27495348";
                    request.TemplateParam = "{\"name\":\"" + model.ST_Name + "\",\"lvnum\":\"" + model.LV_Num + "\"}";
                }
                else if (model.MessageType == "failed")
                {
                    //驳回请假模板
                    request.TemplateCode = "SMS_27620081";
                    request.TemplateParam = "{\"name\":\"" + model.ST_Name + "\",\"lvnum\":\"" + model.LV_Num + "\"}";
                }
                else if (model.MessageType == "FindPsd")
                {
                    //短信验证找回密码
                    request.TemplateCode = "SMS_60140885";
                    request.TemplateParam = "{\"name\":\"" + model.ST_Name + "\",\"lvnum\":\"" + model.LV_Num + "\"}";
                }
                else
                {
                    return null;
                }

                //可选:outId为提供给业务方扩展字段,最终在短信回执消息中将此值带回给调用者
                request.OutId = "qingjia";
                //请求失败这里会抛ClientException异常
                response = acsClient.GetAcsResponse(request);

                //保存 发送记录
                SaveMessageList(model.ST_Num, model.LV_Num, model.ST_Tel, model.MessageType);
            }
            catch (ServerException e)
            {
                Console.WriteLine(e.ErrorCode);
            }
            catch (ClientException e)
            {
                Console.WriteLine(e.ErrorCode);
            }
            return response;
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
}