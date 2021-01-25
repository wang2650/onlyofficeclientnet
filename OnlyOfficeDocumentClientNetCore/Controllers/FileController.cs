using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using OnlyOfficeDocumentClientNetCore.Model;
using System.IO;
using OnlyOfficeDocumentClientNetCore.Op;
using OnlyOfficeDocumentClientNetCore.Common;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Mvc;
namespace OnlyOfficeDocumentClientNetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class  FileController : Controller
    {


        [HttpGet]
        [Route("DisplayPageConfig")]
        public JsonResult DisplayPageConfig()
        {


            OpResult result = new OpResult();
            string fileId = "53259a15dbb44deda722dd96dc774d87";
            string userId = "1";
            string userName = "songyan";
            bool canEdit = true;
            bool canDownLoad = true;
            string sign = "sign";
            OnlyOfficeDocumentClientNetCore.Common.Tools.ConfigHost(this.Request);

            var cfg=  ConfigOp.GetDisplayPageConfig(fileId,userId,userName,canEdit,canDownLoad,sign);
            result= cfg;




            return Json(result);


        }
        [HttpPost]
        [Route("CallbackUrl")]
        public JsonResult CallbackUrl()
        {
            OpResult result = new OpResult();
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
        public async Task<IActionResult> DownloadFileByFileId(string  fileId)
        {
            OpResult result = new OpResult();

            var rs=  ConfigOp.GetOne(fileId);
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

        [HttpPost]
        [Route("UploadHeadImage")]
        public async Task<JsonResult> UploadHeadImage()
        {
            OpResult result = new OpResult();

            var boundary = this.Request.GetMultipartBoundary();
            string targetDirectory = "wwwroot\\uploadfiles";
            //检查相应目录
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }
            var result2 = this.Request.Headers.TryGetValue("wxq", out StringValues authStr);
            var filePath = string.Empty;
            var reader = new Microsoft.AspNetCore.WebUtilities.MultipartReader(boundary, this.Request.Body);
            try
            {
                CancellationToken cancellationToken = new System.Threading.CancellationToken();
                var section = await reader.ReadNextSectionAsync(cancellationToken);

                while (section != null)
                {
                    ContentDispositionHeaderValue header = section.GetContentDispositionHeader();

                    if (header.FileName.HasValue || header.FileNameStar.HasValue)
                    {
                        var fileSection = section.AsFileSection();

                        var fileName = fileSection.FileName;
                        var mimeType = fileSection.Section.ContentType;
                        filePath = Path.Combine(targetDirectory, fileName);

                        using (var writeStream = System.IO.File.Create(filePath))
                        {
                            const int bufferSize = 1024;
                            await fileSection.FileStream.CopyToAsync(writeStream, bufferSize, cancellationToken);
                        }
                    }
                    //else
                    //{
                    //  取formdata中的信息
                    //    var formDataSection = section.AsFormDataSection();
                    //    var name = formDataSection.Name;
                    //    var value = await formDataSection.GetValueAsync();
                    //    uploadSectionInfo.Dicts.Add(new KV(name, value));

                    //}
                    section = await reader.ReadNextSectionAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                //return (false, "用户取消操作", null);
            }

            return Json(result);
        }



        //public async Task<IActionResult> Download(string id)
        //{
        //    string filename = "a.txt";
        //    string filePath = "/downlfile/" + filename;
        //    if (!System.IO.File.Exists(filePath))
        //    {
        //        ResponseResult result = new ResponseResult();
        //        result.Code = ResponseResultMessageDefine.OpLost;
        //        result.Errors.Add(ResponseResultMessageDefine.NoExistFile);

        //        return Ok(result);
        //    }

        //    else
        //    {
        //        var memoryStream = new MemoryStream();
        //        using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 65536, FileOptions.Asynchronous | FileOptions.SequentialScan))
        //        {
        //            await stream.CopyToAsync(memoryStream);
        //        }
        //        var ext = "." + filename.Split('.')[1];
        //        memoryStream.Seek(0, SeekOrigin.Begin);
        //        new FileExtensionContentTypeProvider().Mappings.TryGetValue(ext, out var contenttype);
        //        return new FileStreamResult(memoryStream, contenttype);
        //    }


        //}


    }
}
