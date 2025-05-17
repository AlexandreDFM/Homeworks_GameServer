//Helper class
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace encryptionHelper
{
    public class AesEncryptionHelper
    {
        private static readonly string Key = "1234567890123456"; 
        // 16 byte encryption key
        private static readonly string IV = "1234567890123456";  
        // 16 byte initialization vector

        public static string Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(Key);
                aes.IV = Encoding.UTF8.GetBytes(IV);

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream
                               (ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string Decrypt(string cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(Key);
                aes.IV = Encoding.UTF8.GetBytes(IV);

                using (var ms = new MemoryStream
                           (Convert.FromBase64String(cipherText)))
                {
                    using (var cs = new CryptoStream
                               (ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (var sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
