using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace SyncCloud.Helpers
{
    public static class Helper
    {
        public static bool ValidateSignature(string content, string signature)
        {
            string appSecret = ConfigurationManager.AppSettings["Dropbox_ClientSecret"];
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appSecret)))
            {
                if (!VerifySha256Hash(hmac, content, signature))
                    return false;
            }
            return true;
        }

        public static string GetSha256Hash(HMACSHA256 sha256Hash, string input)
        {
            byte[] data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            var stringBuilder = new StringBuilder();
            foreach (byte t in data)
            {
                stringBuilder.Append(t.ToString("x2"));
            }
            return stringBuilder.ToString();
        }

        public static bool VerifySha256Hash(HMACSHA256 sha256Hash, string input, string hash)
        {
            string hashOfInput = GetSha256Hash(sha256Hash, input);
            if (String.Compare(hashOfInput, hash, StringComparison.OrdinalIgnoreCase) == 0)
                return true;
            return false;
        }
    }
}