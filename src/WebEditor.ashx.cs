/*
 *
 * (c) Copyright Ascensio System SIA 2020
 *
 * The MIT License (MIT)
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
*/

using ASC.Api.DocumentConverter;
using OnlineEditorsExample.DbOp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace OnlineEditorsExample
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class WebEditor : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            string userinfo = string.Empty;
            string Authorization = string.Empty;

            if (context.Request.Form.Count>0 &&  !string.IsNullOrEmpty(context.Request.Form["userinfo"]))
            {
                userinfo = context.Request.Form["userinfo"];
            }

            if (context.Request.Headers.Count > 0 && !string.IsNullOrEmpty(context.Request.Headers["Authorization"]))
            {
                Authorization = context.Request.Headers["Authorization"];
            }

            string userId = string.Empty;
            string userName = string.Empty;


            switch (context.Request["type"])
            {
                case "upload":
                    Upload(context,userId,userName);
                    break;
                case "convert":
                    Convert(context, userId, userName);
                    break;
                case "track":
                    Track(context);
                    break;
                case "remove":
                    Remove(context, userId, userName);
                    break;
            }
        }

        private static void Upload(HttpContext context,string userId,string userName)
        {
        
            context.Response.ContentType = "text/plain";
           
            try
            {
                if (context.Request.Files.Count == 0)
                {
                    OpResult result = new OpResult();
                    result.Code = 1;
                    result.ErrorMessage = "没有上传文件";


                    context.Response.Write( Newtonsoft.Json.JsonConvert.SerializeObject(result));
                }
                else
                {

                    context.Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(DefaultConfigTools.DoUpload(context , userId, userName) ));
                }


            }
            catch (Exception e)
            {
                OpResult result = new OpResult();
                result.Code = 1;
                result.ErrorMessage = e.Message;
                context.Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(result));
            }
        }

        private static void Convert(HttpContext context,string userId,string userName)
        {
            context.Response.ContentType = "text/plain";
            try
            {
                context.Response.Write(DefaultConfigTools.DoConvert(context,userId, userName));
            }
            catch (Exception e)
            {
                context.Response.Write("{ \"error\": \"" + e.Message + "\"}");
            }
        }

        private enum TrackerStatus
        {
            NotFound = 0,
            Editing = 1,
            MustSave = 2,
            Corrupted = 3,
            Closed = 4,
        }

        private static void Track(HttpContext context)
        {
            var fileid = context.Request["fileid"];
            var path = string.Empty;
            var fileName = string.Empty;
            FileInfomation fileInfomation = DbClient.GetFileInfomation(fileid);
            if (fileInfomation!=null)
            {
                path = fileInfomation.filepath;
                fileName = fileInfomation.newfilename;
            }
           

            string body;
            try
            {
                using (var receiveStream = context.Request.InputStream)
                using (var readStream = new StreamReader(receiveStream))
                {
                    body = readStream.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                throw new HttpException((int) HttpStatusCode.BadRequest, e.Message);
            }

            var jss = new JavaScriptSerializer();
            if (string.IsNullOrEmpty(body)) return;
            var fileData = jss.Deserialize<Dictionary<string, object>>(body);

            if (JwtManager.Enabled)
            {
                if (fileData.ContainsKey("token"))
                {
                    fileData = jss.Deserialize<Dictionary<string, object>>(JwtManager.Decode(fileData["token"].ToString()));
                }
                else if (context.Request.Headers.AllKeys.Contains("Authorization", StringComparer.InvariantCultureIgnoreCase))
                {
                    var headerToken = context.Request.Headers.Get("Authorization").Substring("Bearer ".Length);
                    fileData = (Dictionary<string, object>)jss.Deserialize<Dictionary<string, object>>(JwtManager.Decode(headerToken))["payload"];
                }
                else
                {
                    throw new Exception("Expected JWT");
                }
            }

            var status = (TrackerStatus) (int) fileData["status"];
            LogOp.WriteLogs("onlyofficelog", "", "status:" + fileData["status"].ToString());

            LogOp.WriteLogs("onlyofficelog", "", "fileData:" + Newtonsoft.Json.JsonConvert.SerializeObject( fileData));
            switch (status)
            {
                case TrackerStatus.MustSave:
                case TrackerStatus.Corrupted:
                    LogOp.WriteLogs("onlyofficelog", "", "进入了:");
                    var downloadUri = (string) fileData["url"];
              
                    var curExt = Path.GetExtension(fileName);
                    var downloadExt = Path.GetExtension(downloadUri) ?? "";
                    if (!downloadExt.Equals(curExt, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var key = ServiceConverter.GenerateRevisionId(downloadUri);

                        try
                        {
                            string newFileUri;
                            ServiceConverter.GetConvertedUri(downloadUri, downloadExt, curExt, key, false, out newFileUri);
                            downloadUri = newFileUri;
                        }
                        catch (Exception ex)
                        {
                            LogOp.WriteLogs("onlyofficelog", "", "出错了:" +ex.Message);
                            fileName = DefaultConfigTools.GetCorrectName(Path.GetFileNameWithoutExtension(fileName) + downloadExt, path);
                        }
                    }

                    // hack. http://ubuntuforums.org/showthread.php?t=1841740
                    if (DefaultConfigTools.IsMono)
                    {
                        ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;
                    }

                    var saved = 1;
                    try
                    {

                        LogOp.WriteLogs("onlyofficelog", "", "下载保存1:" );
                        var storagePath = DefaultConfigTools.StoragePath(fileName);
                        var histDir = DefaultConfigTools.HistoryDir(storagePath);
                        var versionDir = DefaultConfigTools.VersionDir(histDir, DefaultConfigTools.GetFileVersion(histDir) + 1);

                        if (!Directory.Exists(versionDir)) Directory.CreateDirectory(versionDir);

                        File.Copy(storagePath, Path.Combine(versionDir, "prev" + curExt));
                        LogOp.WriteLogs("onlyofficelog", "", "下载保存2:");
                        DownloadToFile(downloadUri, DefaultConfigTools.StoragePath(fileName));
                        DownloadToFile((string)fileData["changesurl"], Path.Combine(versionDir, "diff.zip"));
                        LogOp.WriteLogs("onlyofficelog", "", "下载保存3:");
                        var hist = fileData.ContainsKey("changeshistory") ? (string)fileData["changeshistory"] : null;
                        if (string.IsNullOrEmpty(hist) && fileData.ContainsKey("history"))
                        {
                            hist = jss.Serialize(fileData["history"]);
                        }

                        if (!string.IsNullOrEmpty(hist))
                        {
                            File.WriteAllText(Path.Combine(versionDir, "changes.json"), hist);
                        }

                        File.WriteAllText(Path.Combine(versionDir, "key.txt"), (string)fileData["key"]);
                    }
                    catch (Exception ex)
                    {
                        LogOp.WriteLogs("onlyofficelog", "", "出错了last:" + ex.Message);
                        saved = 0;
                    }

                    break;
            }
            context.Response.Write("{\"error\":0}");
        }

        private static void Remove(HttpContext context, string userId, string userName)
        {
            context.Response.ContentType = "text/plain";
            try
            {
                var fileName = context.Request["fileName"];
                var path = DefaultConfigTools.StoragePath(fileName);
                var histDir = DefaultConfigTools.HistoryDir(path);

                if (File.Exists(path)) File.Delete(path);
                if (Directory.Exists(histDir)) Directory.Delete(histDir, true);

                context.Response.Write("{ \"success\": true }");
            }
            catch (Exception e)
            {
                context.Response.Write("{ \"error\": \"" + e.Message + "\"}");
            }
        }

        private static void DownloadToFile(string url, string path)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentException("url");
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("path");

            var req = (HttpWebRequest)WebRequest.Create(url);
            using (var stream = req.GetResponse().GetResponseStream())
            {
                if (stream == null) throw new Exception("stream is null");
                const int bufferSize = 4096;

                using (var fs = File.Open(path, FileMode.Create))
                {
                    var buffer = new byte[bufferSize];
                    int readed;
                    while ((readed = stream.Read(buffer, 0, bufferSize)) != 0)
                    {
                        fs.Write(buffer, 0, readed);
                    }
                }
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}