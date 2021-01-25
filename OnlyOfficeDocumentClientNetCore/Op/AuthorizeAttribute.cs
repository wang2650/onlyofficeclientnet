using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using OnlyOfficeDocumentClientNetCore.Common;
using System;
using System.Linq;

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
                var queryString = filterContext.HttpContext.Request.Query["sign"];
                if (queryString.Count > 0 && string.IsNullOrEmpty(queryString.FirstOrDefault()))
                {
                    string signstr = queryString[0];
                }
                else
                {
                    filterContext.Result = new UnauthorizedResult();
                }
            }
            else
            {
                //var jwtuserModel = JwtHelper.DerializeJWT(authStr.ToString());

                //if (jwtuserModel != null && jwtuserModel.ExpDate > new DateTimeOffset(DateTime.Now.AddHours(1)).ToUnixTimeSeconds())
                //{
                //    var identity = new CustomIdentity(jwtuserModel.Uid.ToString());

                //    var principal = new ClaimsPrincipal(identity);

                //    filterContext.HttpContext.User = principal;
                //}
            }
        }
    }

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
                if (queryString.Count > 0 && string.IsNullOrEmpty(queryString.FirstOrDefault()))
                {
                    string signstr = queryString[0];

                    if (string.IsNullOrEmpty(signstr))
                    {
                        filterContext.Result = new UnauthorizedResult();
                    }
                    else
                    {
                        try
                        {
                            Model.Sign sg = Newtonsoft.Json.JsonConvert.DeserializeObject<Model.Sign>(Security.Decrypt( signstr));
                            if (sg != null && !string.IsNullOrEmpty(sg.username) && sg.dt.AddSeconds(10) < DateTime.Now)
                            {
                                filterContext.Result = new UnauthorizedResult();
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("sign格式错误",ex);
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