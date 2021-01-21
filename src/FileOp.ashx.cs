using OnlineEditorsExample.DbOp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineEditorsExample
{
    /// <summary>
    /// Summary description for FileOp
    /// </summary>
    public class FileOp : IHttpHandler
    {
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
            string fileId = context.Request["fileId"];
            string userId = context.Request["userId"];
            string userName = context.Request["userName"];

            string authtoken = context.Request.Headers[""];

            if (string.IsNullOrEmpty(authtoken))
            {
                result.Code = 2;
                result.ErrorMessage = "没有身份信息token";


            }
            else
            {





                if (!string.IsNullOrEmpty(optype.ToLower()))
                {
                    switch (optype)
                    {
                        case "delete":
                            try
                            {


                                if (string.IsNullOrEmpty(fileId))
                                {
                                    result.Code = 1;
                                    result.ErrorMessage = "文件id参数没有设置";
                                }
                                else
                                {
                                    // DbClient.DeleteFileInfomatin(fileId);
                                    var model = DbClient.GetFileInfomation(fileId);
                                    model.filestate = 2;
                                    model.updateuserid = userId;
                                    model.updateusername = userName;
                                    model.updatetime = DateTime.UtcNow;
                                    DbClient.UpdateFileInfomatin(model);

                                }
                            }
                            catch (Exception ex)
                            {
                                result.Code = 1;
                                result.ErrorMessage = ex.Message;

                            }



                            break;
                        case "create":
                            try
                            {
                                if (string.IsNullOrEmpty(filetemplateId))
                                {
                                    result.Code = 1;
                                    result.ErrorMessage = "文件id参数没有设置";
                                }
                                else
                                {
                                    int templateId = 0;
                                    int.TryParse(filetemplateId, out templateId);
                                    result = DefaultConfigTools.DoUpload(templateId, userId, userName);




                                }
                            }
                            catch (Exception ex)
                            {
                                result.Code = 1;
                                result.ErrorMessage = ex.Message;
                            }
                            break;
                        case "fileexist":


                            try
                            {


                                if (string.IsNullOrEmpty(fileId))
                                {
                                    result.Code = 1;
                                    result.ErrorMessage = "文件id参数没有设置";
                                }
                                else
                                {
                                    // DbClient.DeleteFileInfomatin(fileId);
                                    var model = DbClient.GetFileInfomation(fileId);
                                    if (model==null)
                                    {
                                     
                                        result.Result = 1;// 无此文件

                                    }
                                    else
                                    {
                                        if (model.filestate==2)
                                        {
                                            result.Result = 2; //以上传状态
                                        }
                                        else
                                        {
                                            result.Result = 0; // 存在
                                        }
                                        
                                    }

                                }
                            }
                            catch (Exception ex)
                            {
                                result.Code = 1;
                                result.ErrorMessage = ex.Message;

                            }






                            break;

                        default:
                            break;

                    }
                }
            }



        




            context.Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(result));






        }

    }
}