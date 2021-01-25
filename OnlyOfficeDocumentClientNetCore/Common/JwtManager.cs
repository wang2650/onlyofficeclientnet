using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Security;
namespace OnlyOfficeDocumentClientNetCore.Common
{
    public static class JwtManager
    {
        private static readonly string Secret;
        public static readonly bool Enabled;

 
        static Microsoft.Extensions.Configuration.IConfiguration Configuration { get; set; }
        static JwtManager()
        {
          
             Secret = Common.Appsettings.app(new string[] { "OnlyOffice", "secret" });
             Enabled = !string.IsNullOrEmpty(Secret);
            
        }

        public static string Encode(IDictionary<string, object> payload)
        {
            var header = new Dictionary<string, object>
                {
                    { "alg", "HS256" },
                    { "typ", "JWT" }
                };

            var encHeader = Base64UrlEncode(Newtonsoft.Json.JsonConvert.SerializeObject(header));
            var encPayload = Base64UrlEncode(Newtonsoft.Json.JsonConvert.SerializeObject(payload));
            var hashSum = Base64UrlEncode(CalculateHash(encHeader, encPayload));

            return string.Format("{0}.{1}.{2}", encHeader, encPayload, hashSum);
        }
        public static string Encode(object payload)
        {
            var header = new Dictionary<string, object>
                {
                    { "alg", "HS256" },
                    { "typ", "JWT" }
                };

            var encHeader = Base64UrlEncode(Newtonsoft.Json.JsonConvert.SerializeObject(header));
            var encPayload = Base64UrlEncode(Newtonsoft.Json.JsonConvert.SerializeObject(payload));
            var hashSum = Base64UrlEncode(CalculateHash(encHeader, encPayload));

            return string.Format("{0}.{1}.{2}", encHeader, encPayload, hashSum);
        }
        public static string Decode(string token)
        {
            if (!Enabled || string.IsNullOrEmpty(token)) return "";

            var split = token.Split('.');
            if (split.Length != 3) return "";

            var hashSum = Base64UrlEncode(CalculateHash(split[0], split[1]));
            if (hashSum != split[2]) return "";
            return Base64UrlDecode(split[1]);
        }

        private static byte[] CalculateHash(string encHeader, string encPayload)
        {
            using (var hasher = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(Secret)))
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(string.Format("{0}.{1}", encHeader, encPayload));
                return hasher.ComputeHash(bytes);
            }
        }

        private static string Base64UrlEncode(string str)
        {
            return Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(str));
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                          .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static string Base64UrlDecode(string payload)
        {
            var b64 = payload.Replace('_', '/').Replace('-', '+');
            switch (b64.Length % 4)
            {
                case 2:
                    b64 += "==";
                    break;
                case 3:
                    b64 += "=";
                    break;
            }
            var bytes = Convert.FromBase64String(b64);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
