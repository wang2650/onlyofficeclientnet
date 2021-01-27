using Microsoft.IdentityModel.Tokens;
using OnlyOfficeDocumentClientNetCore.Model;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnlyOfficeDocumentClientNetCore.Common
{
    public class JwtHelper
    {
        private static string iss = "";
        private static string aud = "";
        private static string secret = "";
        private static SigningCredentials creds = null;

        public JwtHelper()
        {
            if (string.IsNullOrEmpty(iss))
            {
                iss = Appsettings.app(new string[] { "Audience", "Issuer" });
            }
            if (string.IsNullOrEmpty(aud))
            {
                aud = Appsettings.app(new string[] { "Audience", "Audience" });
            }
            if (string.IsNullOrEmpty(secret))
            {
                secret = Appsettings.app(new string[] { "Audience", "Secret" });
            }

            if (creds == null)
            {     //秘钥 (SymmetricSecurityKey 对安全性的要求，密钥的长度太短会报出异常)
                SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
                creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            }
        }

        /// <summary>
        /// 颁发JWT字符串
        /// </summary>
        /// <param name="tokenModel"></param>
        /// <returns></returns>
        public static string SerializeJWT(TokenModelJWT tokenModel)
        {
            DateTime dateTime = DateTime.UtcNow;

            DateTime expDt = DateTime.Now.AddHours(9);//默认过期时间8小时

            List<Claim> claims = new List<Claim>
                {
                    //下边为Claim的默认配置
                new Claim(JwtRegisteredClaimNames.Jti, tokenModel.Uid.ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, $"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}"),
                new Claim(JwtRegisteredClaimNames.Nbf,$"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}") ,
                //这个就是过期时间，目前是过期8小时，可自定义，注意JWT有自己的缓冲过期时间
                new Claim (JwtRegisteredClaimNames.Exp,$"{new DateTimeOffset(expDt).ToUnixTimeSeconds()}"),
               new Claim(JwtRegisteredClaimNames.Iss,iss),
                new Claim(JwtRegisteredClaimNames.Aud,aud),
               };

            JwtSecurityToken jwt = new JwtSecurityToken(
                issuer: iss,
                expires: expDt,
                claims: claims,
                signingCredentials: creds);

            JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();
            string encodedJwt = jwtHandler.WriteToken(jwt);

            return Security.Encrypt( encodedJwt);
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="jwtStr"></param>
        /// <returns></returns>
        public static TokenModelJWT DerializeJWT(string jwtStr)
        {


            JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = jwtHandler.ReadJwtToken(Security.Decrypt(    jwtStr));

            object role = new object(); ;
            try
            {
                jwtToken.Payload.TryGetValue(ClaimTypes.Role, out role);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            TokenModelJWT tm = new TokenModelJWT
            {
                ExpDate = jwtToken.Payload.Exp,
                Uid = Convert.ToInt32(jwtToken.Id)
            };
            return tm;
        }
    }
}