using System;
using System.IO;
using System.Web;

namespace qingjia_MVC.Common
{
    public class OutputLog
    {
        public static string logPath = System.AppDomain.CurrentDomain.BaseDirectory;

        public static void WriteLog(string msg)
        {
            using (FileStream fs = new FileStream(logPath + @"\App_Data\log." + DateTime.Now.ToString("yyMM") + ".txt", FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.BaseStream.Seek(0, SeekOrigin.End);
                    sw.WriteLine("DateTime:" + DateTime.Now.ToString() + "      Url:" + HttpContext.Current.Request.Path + "    Message:{0}\n", msg, DateTime.Now);
                    sw.Flush();
                }
            }
        }
    }
}