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
            _logger.LogDebug(1, "NLog injected into HomeController");
        }

        [HttpGet]
        [Route("DisplayPageConfig")]
        public JsonResult DisplayPageConfig([FromForm] Model.InputPara.DisplayPageConfig model)
        {
            OpResult result = new OpResult();

            string userId = "1";
            string userName = "songyan";

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

        [HttpGet]
        [Route("Error")]
        public JsonResult Error()
        {
            OpResult result = new OpResult();
            return Json(result);
        }

        [HttpGet]
        [Route("Edit")]
        public JsonResult Edit()
        {
            OpResult result = new OpResult();
            return Json(result);
        }

        [HttpGet]
        [Route("DownloadFileByFileId")]
        public async Task<IActionResult> DownloadFileByFileId(string fileId)
        {
            OpResult result = new OpResult();

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

        [HttpGet]
        [Route("GetFileByFileId")]
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

        [HttpGet]
        [Route("GetFileHistoryByFileId")]
        public JsonResult GetFileHistoryByFileId([FromQuery] int userId)
        {
            OpResult result = new OpResult();

            return Json(result);
        }

        [HttpGet]
        [Route("GetFileHistoryDataByFileId")]
        public JsonResult GetFileHistoryDataByFileId([FromQuery] int userId)
        {
            OpResult result = new OpResult();

            return Json(result);
        }

        [HttpGet]
        [Route("DeleteByFileId")]
        public JsonResult DeleteByFileId([FromQuery] int userId)
        {
            OpResult result = new OpResult();

            return Json(result);
        }

        private async Task<OpResult> SaveUploadFileAsync()
        {
            OpResult result = new OpResult();
            string id = Guid.NewGuid().ToString("N");
            string newFileName = string.Empty;
            string oldFileName = string.Empty;
            string filePath = DateTime.Now.ToString(format: "yyyy-MM-dd");

            #region 保存文件

            var boundary = this.Request.GetMultipartBoundary();
            string targetDirectory = Common.Tools.VirtualPath + filePath + "/";
            //检查相应目录
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }
            var result2 = this.Request.Headers.TryGetValue("Authorization", out StringValues authStr);

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

            string userId = "999";
            string userName = "wxq";
            try
            {
                ConfigOp.InsertFileInfomation(userId, userName, oldFileName, newFileName, id, filePath, 1);
            }
            catch (Exception ex)
            {
                result.error = 1;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        [HttpPost]
        [Route("UploadFile2")]
        public async Task<JsonResult> UploadHeadImage()
        {
            OpResult result = new OpResult();

            result = await SaveUploadFileAsync();

            return Json(result);
        }
    }
}