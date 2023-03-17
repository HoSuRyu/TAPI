using System;
using System.Security.Cryptography;
using System.Text;

namespace TAPI_Interviewer.Helpers
{
    public static class TripleDESCryptoService
    {
        private static string key = "HRC&&^^#$%_^&*()";

        public static string Encrypt(string toEncrypt)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            keyArray = UTF8Encoding.UTF8.GetBytes(key);
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();

            //return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            ZBase32Encoder enc = new ZBase32Encoder();
            return enc.Encode(resultArray);

        }

        public static string Decrypt(string cipherString)
        {
            if (string.IsNullOrEmpty(cipherString) == true)
                return "";

            byte[] keyArray;
            //byte[] toEncryptArray = Convert.FromBase64String(cipherString);

            ZBase32Encoder enc = new ZBase32Encoder();
            byte[] toEncryptArray = enc.Decode(cipherString);

            keyArray = UTF8Encoding.UTF8.GetBytes(key);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();

            return UTF8Encoding.UTF8.GetString(resultArray);
        }
    }
}