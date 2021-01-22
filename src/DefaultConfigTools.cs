using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using ASC.Api.DocumentConverter;
using OnlineEditorsExample.DbOp;

namespace OnlineEditorsExample
{
    public class DefaultConfigTools
    {

        public static UriBuilder Host
        {
            get
            {
                var uri = new UriBuilder(HttpContext.Current.Request.Url) { Query = "" };
                var requestHost = HttpContext.Current.Request.Headers["Host"];
                if (!string.IsNullOrEmpty(requestHost))
                    uri = new UriBuilder(uri.Scheme + "://" + requestHost);

                return uri;
            }
        }


        public static string VirtualPath
        {
            get
            {
                return
                    HttpRuntime.AppDomainAppVirtualPath
                    + (HttpRuntime.AppDomainAppVirtualPath.EndsWith("/") ? "" : "/")
                    + WebConfigurationManager.AppSettings["storage-path"]
                    + CurUserHostAddress(null) + "/";
            }
        }

        private static bool? _ismono;
        public static bool IsMono
        {
            get { return _ismono.HasValue ? _ismono.Value : (_ismono = (bool?)(Type.GetType("Mono.Runtime") != null)).Value; }
        }

        public static string CurUserHostAddress(string userAddress)
        {
            return Regex.Replace(userAddress ?? HttpContext.Current.Request.UserHostAddress, "[^0-9a-zA-Z.=]", "_");
        }



        public static long MaxFileSize
        {
            get
            {
                long size;
                long.TryParse(WebConfigurationManager.AppSettings["filesize-max"], out size);
                return size > 0 ? size : 5 * 1024 * 1024;
            }
        }

        public static List<string> FileExts
        {
            get { return ViewedExts.Concat(EditedExts).Concat(ConvertExts).ToList(); }
        }

        private static List<string> ViewedExts
        {
            get { return (WebConfigurationManager.AppSettings["files.docservice.viewed-docs"] ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList(); }
        }

        public static List<string> EditedExts
        {
            get { return (WebConfigurationManager.AppSettings["files.docservice.edited-docs"] ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList(); }
        }

        public static List<string> ConvertExts
        {
            get { return (WebConfigurationManager.AppSettings["files.docservice.convert-docs"] ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList(); }
        }




        public static string StoragePath( string fileid)
        {
            FileInfomation fileInfomation = DbClient.GetFileInfomation(fileid);
            string fileName = fileid;
            string filePath = string.Empty;
            if (fileInfomation!=null)
            {
                fileName = fileInfomation.newfilename;
                filePath = fileInfomation.filepath;

            }
            else
            {
                filePath = DateTime.UtcNow.ToString("yyyyMMdd");
            }

            var directory =  WebConfigurationManager.AppSettings["storage-path"] + filePath + "\\";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return directory + fileName;
        }

        public static string HistoryDir(string storagePath)
        {
            return storagePath += "-hist";
        }

        public static string VersionDir(string histPath, int version)
        {
            return Path.Combine(histPath, version.ToString());
        }

        public static string VersionDir(string fileName, string userAddress, int version)
        {
            return VersionDir(HistoryDir(StoragePath(fileName)), version);
        }

        public static int GetFileVersion(string historyPath)
        {
            if (!Directory.Exists(historyPath)) return 0;
            return Directory.EnumerateDirectories(historyPath).Count();
        }

        public static int GetFileVersion(string fileName, string userAddress)
        {
            return GetFileVersion(HistoryDir(StoragePath(fileName)));
        }

        public static string FileUri(string id)
        {
            Sign sg = new Sign();
            sg.dt = DateTime.UtcNow;
            sg.url = "fileid=" + id;
            var uri = DefaultConfigTools.Host;
            uri.Path = "getfile.ashx";
            uri.Query = "fileid=" + id + "&sign=" + Security.Encrypt(  Newtonsoft.Json.JsonConvert.SerializeObject(sg),"wang2650");
            return uri.ToString() ;
        }

        public static string DocumentType(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = string.Empty;
            }
            else
            {
                var ext = Path.GetExtension(fileName).ToLower();

                if (FileType.ExtsDocument.Contains(ext)) fileName = "text";
                if (FileType.ExtsSpreadsheet.Contains(ext)) fileName = "spreadsheet";
                if (FileType.ExtsPresentation.Contains(ext)) fileName = "presentation";

            }
            return fileName;



        }

        protected string UrlPreloadScripts =GlobalConfig.GetOnlyOfficeServerIp()+ WebConfigurationManager.AppSettings["files.docservice.url.preloader"];

        public static OpResult DoUpload(HttpContext context,string userId,string userName)
        {
            OpResult result = new OpResult();
            FileInfoResult fileInfoResult = new FileInfoResult();
            string oldFIleName = string.Empty;
            string path = DateTime.UtcNow.ToString("yyyyMMdd");

            var httpPostedFile = context.Request.Files[0];

            if (HttpContext.Current.Request.Browser.Browser.ToUpper() == "IE")
            {
                var files = httpPostedFile.FileName.Split(new char[] { '\\' });
                oldFIleName = files[files.Length - 1];
            }
            else
            {
                oldFIleName = httpPostedFile.FileName;
            }

       

            var curSize = httpPostedFile.ContentLength;
            if (DefaultConfigTools.MaxFileSize < curSize || curSize <= 0)
            {
                throw new Exception("File size is incorrect");
            }

            var curExt = (Path.GetExtension(oldFIleName) ?? "").ToLower();
            if (!DefaultConfigTools.FileExts.Contains(curExt))
            {
                throw new Exception("File type is not supported");
            }
            string guidString = Guid.NewGuid().ToString("N");
            string newFileName = guidString + oldFIleName.Substring(oldFIleName.IndexOf("."), oldFIleName.Length- oldFIleName.IndexOf("."));

              var savedFileName = DefaultConfigTools.StoragePath(newFileName);

            httpPostedFile.SaveAs(savedFileName);

            var histDir = DefaultConfigTools.HistoryDir(savedFileName);
            Directory.CreateDirectory(histDir);
            File.WriteAllText(Path.Combine(histDir, "createdInfo.json"), new JavaScriptSerializer().Serialize(new Dictionary<string, object> {
                { "created", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss:ffff") },
                { "id", userId },
                { "name", userName }
            }));
            fileInfoResult.FileId = guidString;
            fileInfoResult.CreateDt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss:ffff");
            fileInfoResult.UserId = userId;
            fileInfoResult.UserName = userName;
            result.Result = fileInfoResult;

            FileInfomation model = new FileInfomation();
            model.id = guidString;
            model.appid = 1;
            model.createtime = DateTime.UtcNow;
            model.createuserid = userId;
            model.createusername = userName;
            model.filepath = path ;
            model.filestate = 0;
            model.newfilename = newFileName;
            model.oldfilename = oldFIleName;
            model.updatetime= DateTime.UtcNow;
            model.updateuserid = userId;
            model.updateusername = userName;
            DbClient.InsertFileInfomation(model);




            return result;
        }

        public static OpResult DoUpload( int filetemplateId,string userId, string userName)
        {
            OpResult result = new OpResult();
            FileInfoResult fileInfoResult = new FileInfoResult();
       
            string path = DateTime.UtcNow.ToString("yyyyMMdd");

            string fileExtType = "";
            switch (filetemplateId)
            {
                case 1:
                    fileExtType = ".docx";
                    break;
                case 2:
                    fileExtType = ".xlsx";
                    break;
                case 3:
                    fileExtType = ".pptx";
                    break;

                default:
                    break;

            }
            

    
            string guidString = Guid.NewGuid().ToString("N");
            string newFileName = guidString + fileExtType;

            var savedFileName = DefaultConfigTools.StoragePath(newFileName);
            File.Copy(HttpRuntime.AppDomainAppPath + "app_data/" + filetemplateId+ fileExtType, savedFileName);
    

            var histDir = DefaultConfigTools.HistoryDir(savedFileName);
            Directory.CreateDirectory(histDir);
            File.WriteAllText(Path.Combine(histDir, "createdInfo.json"), new JavaScriptSerializer().Serialize(new Dictionary<string, object> {
                { "created", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss:ffff") },
                { "id", userId },
                { "name", userName }
            }));
            fileInfoResult.FileId = guidString;
            fileInfoResult.CreateDt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss:ffff");
            fileInfoResult.UserId = userId;
            fileInfoResult.UserName = userName;
            result.Result = fileInfoResult;

            FileInfomation model = new FileInfomation();
            model.id = guidString;
            model.appid = 1;
            model.createtime = DateTime.UtcNow;
            model.createuserid = userId;
            model.createusername = userName;
            model.filepath = path;
            model.filestate = 0;
            model.newfilename = newFileName;
            model.oldfilename = "新文件"+fileExtType;
            model.updatetime = DateTime.UtcNow;
            model.updateuserid = userId;
            model.updateusername = userName;
            DbClient.InsertFileInfomation(model);




            return result;
        }



        public static string DoUpload(string fileUri, HttpRequest request)
        {
            string oldFIleName = string.Empty;
            string path = DateTime.UtcNow.ToString("yyyyMMdd");
            oldFIleName = GetCorrectName(Path.GetFileName(fileUri), path);

            var curExt = (Path.GetExtension(oldFIleName) ?? "").ToLower();
            if (!DefaultConfigTools.FileExts.Contains(curExt))
            {
                throw new Exception("File type is not supported");
            }

            var req = (HttpWebRequest)WebRequest.Create(fileUri);

            try
            {
                // hack. http://ubuntuforums.org/showthread.php?t=1841740
                if (DefaultConfigTools.IsMono)
                {
                    ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;
                }

                using (var stream = req.GetResponse().GetResponseStream())
                {
                    if (stream == null) throw new Exception("stream is null");
                    const int bufferSize = 4096;

                    using (var fs = File.Open(DefaultConfigTools.StoragePath(oldFIleName), FileMode.Create))
                    {
                        var buffer = new byte[bufferSize];
                        int readed;
                        while ((readed = stream.Read(buffer, 0, bufferSize)) != 0)
                        {
                            fs.Write(buffer, 0, readed);
                        }
                    }
                }

                var histDir = DefaultConfigTools.HistoryDir(DefaultConfigTools.StoragePath(oldFIleName));
                Directory.CreateDirectory(histDir);
                File.WriteAllText(Path.Combine(histDir, "createdInfo.json"), new JavaScriptSerializer().Serialize(new Dictionary<string, object> {
                    { "created", DateTime.Now.ToString() },
                    { "id", request.Cookies.GetOrDefault("uid", "uid-1") },
                    { "name", request.Cookies.GetOrDefault("uname", "John Smith") }
                }));
            }
            catch (Exception)
            {

            }
            return oldFIleName;
        }

        public static string DoConvert(HttpContext context, string userId, string userName)
        {
            string oldFIleName = string.Empty;
            oldFIleName = context.Request["filename"];
      
            var extension = (Path.GetExtension(oldFIleName) ?? "").Trim('.');
            var internalExtension = FileType.GetInternalExtension(oldFIleName).Trim('.');
            string path = context.Request["path"];
            if (DefaultConfigTools.ConvertExts.Contains("." + extension)
                && !string.IsNullOrEmpty(internalExtension))
            {
                var key = ServiceConverter.GenerateRevisionId(DefaultConfigTools.FileUri(oldFIleName));

                string newFileUri;
                var result = ServiceConverter.GetConvertedUri(DefaultConfigTools.FileUri(oldFIleName), extension, internalExtension, key, true, out newFileUri);
                if (result != 100)
                {
                    return "{ \"step\" : \"" + result + "\", \"filename\" : \"" + oldFIleName + "\"}";
                }
               
                var fileName = GetCorrectName(Path.GetFileNameWithoutExtension(oldFIleName) + "." + internalExtension,path);

                var req = (HttpWebRequest)WebRequest.Create(newFileUri);

                // hack. http://ubuntuforums.org/showthread.php?t=1841740
                if (DefaultConfigTools.IsMono)
                {
                    ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;
                }

                using (var stream = req.GetResponse().GetResponseStream())
                {
                    if (stream == null) throw new Exception("Stream is null");
                    const int bufferSize = 4096;

                    using (var fs = File.Open(DefaultConfigTools.StoragePath(fileName), FileMode.Create))
                    {
                        var buffer = new byte[bufferSize];
                        int readed;
                        while ((readed = stream.Read(buffer, 0, bufferSize)) != 0)
                        {
                            fs.Write(buffer, 0, readed);
                        }
                    }
                }

                var storagePath = DefaultConfigTools.StoragePath(oldFIleName);
                var histDir = DefaultConfigTools.HistoryDir(storagePath);
                File.Delete(storagePath);
                if (Directory.Exists(histDir)) Directory.Delete(histDir, true);

                oldFIleName = fileName;
                histDir = DefaultConfigTools.HistoryDir(DefaultConfigTools.StoragePath(oldFIleName));
                Directory.CreateDirectory(histDir);
                File.WriteAllText(Path.Combine(histDir, "createdInfo.json"), new JavaScriptSerializer().Serialize(new Dictionary<string, object> {
                    { "created", DateTime.UtcNow.ToString() },
                    { "id", context.Request.Cookies.GetOrDefault("uid", userId) },
                    { "name", context.Request.Cookies.GetOrDefault("uname", userName) }
                }));
            }

            return "{ \"filename\" : \"" + oldFIleName + "\"}";
        }

        public static string GetCorrectName(string fileName, string path)
        {
            var baseName = Path.GetFileNameWithoutExtension(fileName);
            var ext = Path.GetExtension(fileName);
            var name = baseName + ext;

            for (var i = 1; File.Exists(DefaultConfigTools.StoragePath(name)); i++)
            {
                name = baseName + " (" + i + ")" + ext;
            }
            return name;
        }

        public static List<string> GetStoredFiles()
        {
            var directory = HttpRuntime.AppDomainAppPath + WebConfigurationManager.AppSettings["storage-path"] + DefaultConfigTools.CurUserHostAddress(null) + "\\";
            if (!Directory.Exists(directory)) return new List<string>();

            var directoryInfo = new DirectoryInfo(directory);

            var storedFiles = directoryInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly).Select(fileInfo => fileInfo.Name).ToList();
            return storedFiles;
        }



    }
}