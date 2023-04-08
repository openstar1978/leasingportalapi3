using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeasingPortalApi.Models
{
    public class Encryption
    {
        public static string Encrypt(string text)
        {
            byte[] inputBuffer = System.Text.Encoding.UTF8.GetBytes(text);
            System.Security.Cryptography.RijndaelManaged rj = new System.Security.Cryptography.RijndaelManaged();

            byte[] key = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 8, 0, 0, 0, 7, 5, 0, 0, 8, 4, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2 };
            byte[] iv = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 8, 0, 0, 0, 7, 5 };

            var enc = rj.CreateEncryptor(key, iv);

            byte[] outputBuffer = enc.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
            string encText = Convert.ToBase64String(outputBuffer);
            return encText;
        }

        public static string Decrypt(string encText)
        {
            System.Security.Cryptography.RijndaelManaged rj = new System.Security.Cryptography.RijndaelManaged();

            byte[] key = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 8, 0, 0, 0, 7, 5, 0, 0, 8, 4, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2 };
            byte[] iv = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 8, 0, 0, 0, 7, 5 };
            var dec = rj.CreateDecryptor(key, iv);
            byte[] encBuffer = Convert.FromBase64String(encText);
            byte[] decBuffer = dec.TransformFinalBlock(encBuffer, 0, encBuffer.Length);
            string originalText = System.Text.Encoding.UTF8.GetString(decBuffer);
            return originalText;
        }
    }
}