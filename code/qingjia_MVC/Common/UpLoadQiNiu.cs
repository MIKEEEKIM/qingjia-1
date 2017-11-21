using Qiniu.IO;
using Qiniu.IO.Model;
using Qiniu.RS;
using Qiniu.RS.Model;
using Qiniu.Util;
using System;
using System.IO;

namespace qingjia_MVC.Common
{
    public class UpLoadQiNiu
    {
        private static string _AccessKey = System.Configuration.ConfigurationManager.AppSettings["QiNiuAccessKey"].ToString().Trim();

        private static string _SecretKey = System.Configuration.ConfigurationManager.AppSettings["QiNiuSecretKey"].ToString().Trim();

        private static string _bucket = System.Configuration.ConfigurationManager.AppSettings["QiNiuBucket"].ToString().Trim();

        private static string _QiNiuUrl = System.Configuration.ConfigurationManager.AppSettings["QiNiuUrl"].ToString().Trim();

        /// <summary>
        /// 上传（来自网络回复的）数据流
        /// </summary>
        public static string UploadStream(FileStream _stream, string _saveKey)
        {
            _saveKey += ".jpg"; //保存格式
            string result_string = _QiNiuUrl + _saveKey;
            
            if (Stat(_saveKey))
            {
                return result_string;
            }

            // 生成(上传)凭证时需要使用此Mac
            // 这个示例单独使用了一个Settings类，其中包含AccessKey和SecretKey
            // 实际应用中，请自行设置您的AccessKey和SecretKey
            Mac mac = new Mac(_AccessKey, _SecretKey);
            string bucket = _bucket;
            string saveKey = _saveKey;

            // 上传策略，参见 
            // https://developer.qiniu.com/kodo/manual/put-policy
            PutPolicy putPolicy = new PutPolicy();

            // 如果需要设置为"覆盖"上传(如果云端已有同名文件则覆盖)，请使用 SCOPE = "BUCKET:KEY"
            putPolicy.Scope = bucket + ":" + saveKey;
            //putPolicy.Scope = bucket;


            // 上传策略有效期(对应于生成的凭证的有效期)          
            putPolicy.SetExpires(3600);


            // 上传到云端多少天后自动删除该文件，如果不设置（即保持默认默认）则不删除
            putPolicy.DeleteAfterDays = 1;


            // 生成上传凭证，参见
            // https://developer.qiniu.com/kodo/manual/upload-token
            string jstr = putPolicy.ToJsonString();
            string token = Auth.CreateUploadToken(mac, jstr);
            try
            {
                string url = "http://img.ivsky.com/img/tupian/pre/201610/09/beifang_shanlin_xuejing-001.jpg";
                var wReq = System.Net.WebRequest.Create(url) as System.Net.HttpWebRequest;
                var resp = wReq.GetResponse() as System.Net.HttpWebResponse;
                //using (var stream = resp.GetResponseStream())
                using (var stream = _stream)
                {
                    // 请不要使用UploadManager的UploadStream方法，因为此流不支持查找(无法获取Stream.Length)
                    // 请使用FormUploader或者ResumableUploader的UploadStream方法
                    FormUploader fu = new FormUploader();
                    Qiniu.Http.HttpResult result = fu.UploadStream(stream, saveKey, token);

                    //代表上传成功
                    if (result.Code.ToString().Trim() == "200")
                    {
                        return result_string;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                return result_string;
            }
        }

        private static bool Stat(string LV_NUM)
        {
            // 这个示例单独使用了一个Settings类，其中包含AccessKey和SecretKey
            // 实际应用中，请自行设置您的AccessKey和SecretKey
            Mac mac = new Mac(_AccessKey, _SecretKey);
            string bucket = _bucket;
            string key = LV_NUM;
            BucketManager bm = new BucketManager(mac);
            StatResult result = bm.Stat(bucket, key);
            if (result.Code.ToString().Trim() == "200")
            {
                //200 状态码代表 已存在该图片
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}