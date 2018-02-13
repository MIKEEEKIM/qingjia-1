using System;
using System.Security.Cryptography;
using System.Text;

namespace qingjia_MVC.Common
{
    public static class PsdEncryption
    {
        public static string Encryption(string password)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            //将要加密的字符串转换为字节数组 两次加密
            byte[] palindata = Encoding.Default.GetBytes(password);
            palindata = md5.ComputeHash(palindata);

            //将字符串加密后也转换为字符数组
            byte[] encryptdata = md5.ComputeHash(palindata);

            //将加密后的字节数组转换为加密字符串
            return Convert.ToBase64String(encryptdata);
        }
    }
}