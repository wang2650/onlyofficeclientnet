using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using OnlyOfficeDocumentClientNetCore.Common;
using OnlyOfficeDocumentClientNetCore.Model;
using OnlyOfficeDocumentClientNetCore.Op;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnlyOfficeDocumentClientNetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : Controller
    {
        private readonly ILogger<FileController> _logger;

        public FileController(ILogger<FileController> logger)
        {
            _logger = logger;
        }

        #region document server调用的接口

        /// <summary>
        /// (使用的document server的认证)服务器回调 文件修改后，回调此页面。用来保存最新版本的文件。（原net例子有保存历史版本和修改记录，这里只保存最新的文件）
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("CallbackUrl")]
        public JsonResult CallbackUrl()
        {
            OpResult result = new OpResult();

            try
            {
                string body = string.Empty;
                try
                {
                    int bufferThreshold = 5120000;//500k  bufferThreshold 设置的是 Request.Body 最大缓存字节数（默认是30K） 超出这个阈值的字节会被写入磁盘
                    int bufferLimit = 10240000;//1m 设置的是 Request.Body 允许的最大字节数（默认值是null），超出这个限制，就会抛出异常 System.IO.IOException
                    Request.EnableBuffering(bufferThreshold, bufferLimit);
                    HttpContext.Request.Body.Position = 0;
                    if (this.Request.ContentLength > 0 && this.Request.Body != null && this.Request.Body.CanRead)
                    {
                        Encoding encoding = System.Text.UTF8Encoding.Default;
                        using (var buffer = new MemoryStream())
                        {
                            HttpContext.Request.Body.CopyTo(buffer);
                            buffer.Flush();
                            buffer.Position = 0;
                            body = buffer.GetReader().ReadToEnd();
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogInformation("e:" + e.Message);
                }
                if (string.IsNullOrEmpty(body))
                {
                    result.error = 1;
                    result.ErrorMessage = "request.body内容为空";
                    return Json(result);
                }

                var fileData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(body);

                if (JwtManager.Enabled)
                {
                    if (fileData.ContainsKey("token"))
                    {
                        fileData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(JwtManager.Decode(fileData["token"].ToString()));
                    }
                    else if (Request.Headers.ContainsKey("Authorization"))
                    {
                        Request.Headers.TryGetValue("Authorization", out StringValues authStr);

                        var headerToken = authStr.ToString().Substring("Bearer ".Length);

                        fileData = (Dictionary<string, object>)Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(JwtManager.Decode(headerToken))["payload"];
                    }
                    else
                    {
                        result.error = 1;
                        result.ErrorMessage = "Expected JWT空";
                        return Json(result);
                    }
                }

                var status = (TrackerStatus)Convert.ToInt32(fileData["status"]);
                switch (status)
                {
                    case TrackerStatus.MustSave:
                    case TrackerStatus.Corrupted:
                        var fileid = this.Request.Query["fileid"].ToString();
                        var path = string.Empty;
                        var fileName = string.Empty;
                        if (!string.IsNullOrEmpty(fileid))
                        {
                            if (fileid.Contains("."))
                            {
                                fileid = fileid.Substring(0, fileid.IndexOf("."));
                            }
                            FileInfomation fileInfomation = ConfigOp.GetOne(fileid);
                            if (fileInfomation != null)
                            {
                                path = fileInfomation.filepath;
                                fileName = fileInfomation.newfilename;
                            }
                        }

                        var downloadUri = (string)fileData["url"];
                        var req = (HttpWebRequest)WebRequest.Create(downloadUri);
                        using (var stream = req.GetResponse().GetResponseStream())
                        {
                            if (stream == null) throw new Exception("stream is null");
                            const int bufferSize = 4096;

                            using (var fs = System.IO.File.Open(Path.Combine(Common.Tools.VirtualPath + path + "/", fileName), FileMode.Create))
                            {
                                var buffer = new byte[bufferSize];
                                int readed;
                                while ((readed = stream.Read(buffer, 0, bufferSize)) != 0)
                                {
                                    fs.Write(buffer, 0, readed);
                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                result.error = 1;
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex.Message);
            }
            return Json(result);
        }

        /// <summary>
        /// 不需要认证 错误页，当document服务器发生错误和脚本错误的时候，回跳转到此页面。
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Error")]
        public JsonResult Error()
        {
            OpResult result = new OpResult();
            result.error = 1;
            result.ErrorMessage = "请确认参数是否错误，并保证文档服务器运行正常";
            return Json(result);
        }

        /// <summary>
        /// 获取文件 document 服务器通过此接口获取要显示编辑的文件
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetFileByFileId")]
        [OnlyOfficeDocumentClientNetCore.Op.AuthorizeDocumentServer]
        public async Task<IActionResult> GetFileByFileId(string fileId)
        {
            OpResult result = new OpResult();

            if (fileId.Contains("."))
            {
                fileId = fileId.Substring(0, fileId.IndexOf("."));
            }

            var rs = ConfigOp.GetOne(fileId);
            string directoryPath = rs.filepath;
            string filePath = Tools.VirtualPath + "/" + directoryPath + "/" + rs.newfilename;
            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 65536, FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                await stream.CopyToAsync(memoryStream);
            }
            memoryStream.Seek(0, SeekOrigin.Begin);

            return File(memoryStream, "application/octet-stream", rs.oldfilename);
        }

        #endregion document server调用的接口

        #region 客户端 调用的接口

        /// <summary>
        /// 获取文档编辑页面的js config配置
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        [OnlyOfficeDocumentClientNetCore.Op.Authorize]
        [Route("DisplayPageConfig")]
        public JsonResult DisplayPageConfig([FromQuery] Model.InputPara.DisplayPageConfig model)
        {
            OpResult result = new OpResult();

            string userId = this.Request.HttpContext.User.Identity.Name;
            string userName = "";

            try
            {
                OnlyOfficeDocumentClientNetCore.Common.Tools.ConfigHost(this.Request);

                result = ConfigOp.GetDisplayPageConfig(model.fileId, userId, userName, model.canEdit, model.canDownLoad);
            }
            catch (Exception ex)
            {
                result.error = 1;
                result.ErrorMessage = ex.Message;
            }
            return Json(result);
        }

        [OnlyOfficeDocumentClientNetCore.Op.Authorize]
        [HttpGet]
        [Route("FileIsExist")]
        public JsonResult FileIsExist(string fileId)
        {
            OpResult result = new OpResult();
            try
            {
                result.error = 0;
                result.Result = ConfigOp.FileIsExist(fileId);
            }
            catch (Exception ex)
            {
                result.error = 1;
                result.ErrorMessage = "数据错误";
            }

            return Json(result);
        }

        [OnlyOfficeDocumentClientNetCore.Op.Authorize]
        [HttpGet]
        [Route("DownloadFileByFileId")]
        public async Task<IActionResult> DownloadFileByFileId()
        {
            OpResult result = new OpResult();
            string fileId = this.Request.Query["fileId"].ToString();
            try
            {
                var rs = ConfigOp.GetOne(fileId);

                if (rs != null && rs.filestate != 2)
                {
                    string directoryPath = rs.filepath;
                    string filePath = Tools.VirtualPath + "/" + directoryPath + "/" + rs.newfilename;
                    var memoryStream = new MemoryStream();
                    using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 65536, FileOptions.Asynchronous | FileOptions.SequentialScan))
                    {
                        await stream.CopyToAsync(memoryStream);
                    }
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    return File(memoryStream, "application/octet-stream", rs.oldfilename);
                }
                else
                {
                    return new JsonResult(new { error = 1, ErrorMessage = "文件已被删除" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = 1, ErrorMessage = ex.Message });
            }
        }

        [HttpGet]
        [OnlyOfficeDocumentClientNetCore.Op.Authorize]
        [Route("GetFileHistoryByFileId")]
        public JsonResult GetFileHistoryByFileId()
        {
            OpResult result = new OpResult();

            return Json(result);
        }

        [HttpGet]
        [OnlyOfficeDocumentClientNetCore.Op.Authorize]
        [Route("GetFileHistoryDataByFileId")]
        public JsonResult GetFileHistoryDataByFileId()
        {
            OpResult result = new OpResult();

            return Json(result);
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DeleteByFileId")]
        [OnlyOfficeDocumentClientNetCore.Op.Authorize]
        public JsonResult DeleteByFileId([FromQuery] string fileId)
        {
            OpResult result = new OpResult();

            result.error = ConfigOp.DeleteForState(fileId, Convert.ToInt32(this.Request.HttpContext.User.Identity.Name), DateTime.Now) ? 0 : 1;
            result.ErrorMessage = result.error == 0 ? "" : "删除失败";

            return Json(result);
        }

        #region 上传文件

        private async Task<OpResult> SaveUploadFileAsync()
        {
            OpResult result = new OpResult();

            string userId = this.Request.HttpContext.User.Identity.Name;

            string id = Guid.NewGuid().ToString("N");
            string newFileName = string.Empty;
            string oldFileName = string.Empty;
            string filePath = DateTime.Now.ToString(format: "yyyyMMdd");

            #region 保存文件

            var boundary = this.Request.GetMultipartBoundary();
            string targetDirectory = Common.Tools.VirtualPath + filePath + "/";
            //检查相应目录
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            var reader = new Microsoft.AspNetCore.WebUtilities.MultipartReader(boundary, this.Request.Body);
            try
            {
                CancellationToken cancellationToken = new System.Threading.CancellationToken();
                var section = await reader.ReadNextSectionAsync(cancellationToken);
                Dictionary<string, string> dt = new Dictionary<string, string>();
                while (section != null)
                {
                    ContentDispositionHeaderValue header = section.GetContentDispositionHeader();

                    if (header.FileName.HasValue || header.FileNameStar.HasValue)
                    {
                        var fileSection = section.AsFileSection();

                        oldFileName = fileSection.FileName;
                        newFileName = id + oldFileName.Substring(oldFileName.LastIndexOf('.'), oldFileName.Length - oldFileName.LastIndexOf('.'));
                        var mimeType = fileSection.Section.ContentType;

                        using (var writeStream = System.IO.File.Create(Path.Combine(targetDirectory, newFileName)))
                        {
                            const int bufferSize = 1024;
                            await fileSection.FileStream.CopyToAsync(writeStream, bufferSize, cancellationToken);
                        }
                    }
                    else
                    {
                        //取formdata中的信息
                        var formDataSection = section.AsFormDataSection();
                        var name = formDataSection.Name;
                        var value = await formDataSection.GetValueAsync();
                        dt.Add(name, value);
                    }
                    section = await reader.ReadNextSectionAsync(cancellationToken);
                }
                result.Result = newFileName;
            }
            catch (OperationCanceledException ex)
            {
                result.error = 1;
                result.ErrorMessage = "上传失败：" + ex.Message;

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                //return (false, "用户取消操作", null);
            }

            #endregion 保存文件

            try
            {
                ConfigOp.InsertFileInfomation(userId, "", oldFileName, newFileName, id, filePath, 1);
            }
            catch (Exception ex)
            {
                result.error = 1;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [OnlyOfficeDocumentClientNetCore.Op.Authorize]
        [Route("UploadFile")]
        public async Task<JsonResult> UploadFile()
        {
            OpResult result = new OpResult();

            result = await SaveUploadFileAsync();

            return Json(result);
        }

        #endregion 上传文件

        [OnlyOfficeDocumentClientNetCore.Op.Authorize]
        [HttpPost]
        [Route("CreateFile")]
        public async Task<JsonResult> CreateFile(int templateId)
        {
            OpResult result = new OpResult();
            string userId = this.Request.HttpContext.User.Identity.Name;

            string id = Guid.NewGuid().ToString("N");
            string filePath = DateTime.Now.ToString(format: "yyyyMMdd");
            if (templateId > 0 && templateId < 4)
            {
                string extName = "";

                switch (templateId)
                {
                    case 1:
                        extName = ".docx";
                        break;

                    case 2:
                        extName = ".xlsx";
                        break;

                    case 3:
                        extName = ".pptx";
                        break;

                    default:
                        break;
                }

                System.IO.File.Copy(Path.Combine(Common.Tools.VirtualPath + "template" + "/", templateId + extName), Path.Combine(Common.Tools.VirtualPath + filePath + "/", id + extName));
                result.error = ConfigOp.InsertFileInfomation(userId, "", "新文件", id + extName, id, filePath) ? 0 : 1;

                result.Result = new { FileId = id };
            }
            else
            {
                result.error = 1;
                result.ErrorMessage = "无此模板";
            }

            return Json(result);
        }

        #endregion 客户端 调用的接口

        [HttpGet]
        [Route("test")]
        public JsonResult test()
        {
            OpResult result = new OpResult();
            TokenModelJWT model = new TokenModelJWT();
            model.ExpDate = 3600;
            model.Uid = 34534;
            model.LoginDateTime = DateTime.Now;
            result.Result = JwtHelper.SerializeJWT(model);

            return Json(result);
        }
    }
}