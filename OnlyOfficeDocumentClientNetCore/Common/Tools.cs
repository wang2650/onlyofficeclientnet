using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using OnlyOfficeDocumentClientNetCore.Model;
using OnlyOfficeDocumentClientNetCore.Common;
namespace OnlyOfficeDocumentClientNetCore.Common
{
    public class Tools
    {

        public static string VirtualPath = string.Empty;

        public static string Secret = string.Empty;

        public static long FileSizeMax = 2048000;
        public static string host = "";
        private static string UrlPreloadScripts = string.Empty;
        public static UriBuilder Host(HttpRequest request)
        {
            {
                var uri = new UriBuilder(request.Host.ToUriComponent()) { Query = "" };
                var requestHost = request.Headers["host"];
                if (!string.IsNullOrEmpty(requestHost))
                    uri = new UriBuilder(uri.Scheme + "://" + requestHost);

                return uri;
            }
        }

        private static bool? _ismono;
        public static bool IsMono
        {
            get { return _ismono.HasValue ? _ismono.Value : (_ismono = (bool?)(Type.GetType("Mono.Runtime") != null)).Value; }
        }
        /// <summary>
        /// 默认5m
        /// </summary>
        public static long MaxFileSize
        {
            get
            {
          
                return FileSizeMax > 0 ? FileSizeMax : 5 * 1024 * 1024;
            }
        }

        public static List<string> FileExts
        {
            get { return ViewedExts.Concat(EditedExts).Concat(ConvertExts).ToList(); }
        }

        private static List<string> ViewedExts
        {
            get { return (Common.Appsettings.app(new string[] { "OnlyOffice", "ViewedDocs" }) ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList(); }
        }

        public static List<string> EditedExts
        {
            get { return (Common.Appsettings.app(new string[] { "OnlyOffice", "EditedDocs" }) ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList(); }
        }

        public static List<string> ConvertExts
        {
            get { return (Common.Appsettings.app(new string[] { "OnlyOffice", "ConvertdDocs" }) ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList(); }
        }

        public static string StoragePath(string fileid)
        {
            Model.FileInfomation fileInfomation = new Model.FileInfomation() ;
           // Model.FileInfomation fileInfomation = DbClient.GetFileInfomation(fileid);
            string fileName = fileid;
            string filePath = string.Empty;
            if (fileInfomation != null)
            {
                fileName = fileInfomation.newfilename;
                filePath = fileInfomation.filepath;

            }
            else
            {
                filePath = DateTime.UtcNow.ToString("yyyyMMdd");
            }

            var directory = VirtualPath + filePath + "\\";
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
        
         return host+"getfile.ashxfileid=" + id + "&sign=" + Security.Encrypt(Newtonsoft.Json.JsonConvert.SerializeObject(sg), "wang2650");
         
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




        public static string GetUrlPreloadScripts() {

            return string.IsNullOrEmpty(UrlPreloadScripts) ? Common.Appsettings.app(new string[] { "OnlyOffice", "OnlyOfficeServerIp" }) + Common.Appsettings.app(new string[] { "OnlyOffice", "preloaderUrl" }) : UrlPreloadScripts;

        }

        public static string CallbackUrl(string fileid)
        {

       return    host + "webeditor.ashx?type=track&fileid=" + System.Web.HttpUtility.UrlEncode(fileid);
       

        }




    }
}