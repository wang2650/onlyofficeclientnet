using System;
using System.Web;

namespace OnlineEditorsExample
{
    public class FileApi : IHttpHandler
    {
        /// <summary>
        /// You will need to configure this handler in the Web.config file of your 
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: https://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpHandler Members

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get
            {
                return false;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            OpResult result = new OpResult();

            context.Response.ContentType = "text/plain";

            string optype = context.Request["optype"];

            string filetemplateId = context.Request["filetemplateId"];


            context.Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(result));






        }

        #endregion
    }
}
