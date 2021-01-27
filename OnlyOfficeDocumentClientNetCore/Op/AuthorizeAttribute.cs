using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using OnlyOfficeDocumentClientNetCore.Common;
using OnlyOfficeDocumentClientNetCore.Model;
using System;
using System.Linq;
using System.Security.Claims;

namespace OnlyOfficeDocumentClientNetCore.Op
{
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public string AccessKey { get; set; }

        public virtual void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if (filterContext == null)
                throw new ArgumentNullException(nameof(filterContext));

            var result = filterContext.HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authStr);
            if (!result || string.IsNullOrEmpty(authStr.ToString()))
            {
                filterContext.Result = new UnauthorizedResult();
            }
            else
            {
                var jwtuserModel =  JwtHelper.DerializeJWT(authStr.ToString());

                if (jwtuserModel != null &&   jwtuserModel.LoginDateTime.AddSeconds(Convert.ToInt32( jwtuserModel.ExpDate))<DateTime.Now )
                {
                    var identity = new CustomIdentity(jwtuserModel.Uid.ToString());

                    var principal = new ClaimsPrincipal(identity);

                    filterContext.HttpContext.User = principal;
                }
                else
                {
                    filterContext.Result = new UnauthorizedResult();
                }
            }
        }
    }

    /// <summary>
    /// documentsever 调用本服务的时候验证，只有验证通过，才可以执行下载文件，callback等操作
    /// </summary>
    public class AuthorizeDocumentServerAttribute : Attribute, IAuthorizationFilter
    {
        public string AccessKey { get; set; }

        public virtual void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if (filterContext == null)
                throw new ArgumentNullException(nameof(filterContext));

    


            if (!ConfigOp.GetwhiteIp().Contains(filterContext.HttpContext.Connection.RemoteIpAddress.ToString()))
            { filterContext.Result = new UnauthorizedResult(); }
            else
            {
                var queryString = filterContext.HttpContext.Request.Query["sign"];
                if ( !string.IsNullOrEmpty(queryString.ToString()))
                {
                    string signstr = queryString.ToString();

                    if (string.IsNullOrEmpty(signstr))
                    {
                        filterContext.Result = new UnauthorizedResult();
                    }
                    else
                    {
                        try
                        {
                            Model.Sign sg = Newtonsoft.Json.JsonConvert.DeserializeObject<Model.Sign>(Security.Decrypt(signstr));
                            if (sg != null && !string.IsNullOrEmpty(sg.username) && sg.dt.AddSeconds(10) < DateTime.Now)
                            {
                                filterContext.Result = new UnauthorizedResult();
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("sign格式错误", ex);
                        }
                    }
                }
                else
                {
                    filterContext.Result = new UnauthorizedResult();
                }
            }
        }
    }
}