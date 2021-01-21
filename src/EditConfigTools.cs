using ASC.Api.DocumentConverter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using OnlineEditorsExample.DbOp;
namespace OnlineEditorsExample
{
    public class EditConfigTools
    {


       // public  string FileName;

        public  string FileUri (string fileId)
        {

           return DefaultConfigTools.FileUri(fileId); 
        }

        public string Key (string fileName)
        {
         
                return ServiceConverter.GenerateRevisionId(DefaultConfigTools.CurUserHostAddress(null)
                                                           + "/" + Path.GetFileName(FileUri(fileName))
                                                           + "/" + File.GetLastWriteTime(DefaultConfigTools.StoragePath(fileName)).GetHashCode());
            
        }

        public static  string DocServiceApiUri
        {
            get { 
                
                return 
                  GlobalConfig.GetOnlyOfficeServerIp()+ WebConfigurationManager.AppSettings["files.docservice.url.api"] ?? string.Empty;
            
            }
        }

        public  string DocConfig { get; private set; }
        public string History { get; private set; }
        public string HistoryData { get; private set; }

        public static string CallbackUrl (string fileid)
        {
           
                var callbackUrl = DefaultConfigTools.Host;
                callbackUrl.Path =
                    HttpRuntime.AppDomainAppVirtualPath
                    + (HttpRuntime.AppDomainAppVirtualPath.EndsWith("/") ? "" : "/")
                    + "webeditor.ashx";
                callbackUrl.Query = "type=track"
                                    + "&fileid=" + HttpUtility.UrlEncode(fileid)
                                   ;
                return callbackUrl.ToString();
            
        }


        public void GetHistory(out Dictionary<string, object> history, out Dictionary<string, object> historyData,string fileid)
        {
            var jss = new JavaScriptSerializer();
            var histDir = DefaultConfigTools.HistoryDir(DefaultConfigTools.StoragePath(fileid));

            history = null;
            historyData = null;

            if (DefaultConfigTools.GetFileVersion(histDir) > 0)
            {
                var currentVersion = DefaultConfigTools.GetFileVersion(histDir);
                var hist = new List<Dictionary<string, object>>();
                var histData = new Dictionary<string, object>();

                for (var i = 0; i <= currentVersion; i++)
                {
                    var obj = new Dictionary<string, object>();
                    var dataObj = new Dictionary<string, object>();
                    var verDir = DefaultConfigTools.VersionDir(histDir, i + 1);

                    var key = i == currentVersion ? Key(fileid) : File.ReadAllText(Path.Combine(verDir, "key.txt"));

                    obj.Add("key", key);
                    obj.Add("version", i);

                    if (i == 0)
                    {
                        var infoPath = Path.Combine(histDir, "createdInfo.json");

                        if (File.Exists(infoPath))
                        {
                            var info = jss.Deserialize<Dictionary<string, object>>(File.ReadAllText(infoPath));
                            obj.Add("created", info["created"]);
                            obj.Add("user", new Dictionary<string, object>() {
                                { "id", info["id"] },
                                { "name", info["name"] },
                            });
                        }
                    }

                    dataObj.Add("key", key);
                    dataObj.Add("url", i == currentVersion ? FileUri(fileid) : MakePublicUrl(Directory.GetFiles(verDir, "prev.*")[0]));
                    dataObj.Add("version", i);
                    if (i > 0)
                    {
                        var changes = jss.Deserialize<Dictionary<string, object>>(File.ReadAllText(Path.Combine(DefaultConfigTools.VersionDir(histDir, i), "changes.json")));
                        var change = ((Dictionary<string, object>)((ArrayList)changes["changes"])[0]);

                        obj.Add("changes", changes["changes"]);
                        obj.Add("serverVersion", changes["serverVersion"]);
                        obj.Add("created", change["created"]);
                        obj.Add("user", change["user"]);

                        var prev = (Dictionary<string, object>)histData[(i - 1).ToString()];
                        dataObj.Add("previous", new Dictionary<string, object>() {
                            { "key", prev["key"] },
                            { "url", prev["url"] },
                        });
                        dataObj.Add("changesUrl", MakePublicUrl(Path.Combine(DefaultConfigTools.VersionDir(histDir, i), "diff.zip")));
                        //  dataObj.Add("token", JwtManager.Encode(dataObj));
                    }

                    hist.Add(obj);
                    // histData.Add("token", JwtManager.Encode(dataObj));
                    histData.Add(i.ToString(), dataObj);
                }

                history = new Dictionary<string, object>()
                {
                    { "currentVersion", currentVersion },
                    { "history", hist }
                };
                historyData = histData;
            }
        }

        public string MakePublicUrl(string fullPath)
        {
            var root = HttpRuntime.AppDomainAppPath + WebConfigurationManager.AppSettings["storage-path"];
            return DefaultConfigTools.Host + fullPath.Substring(root.Length).Replace(Path.DirectorySeparatorChar, '/');
        }

        public static void Try(string type, string sample, HttpRequest request)
        {
            string oldFIleName = string.Empty;
            string ext;
            switch (type)
            {
                case "document":
                    ext = ".docx";
                    break;
                case "spreadsheet":
                    ext = ".xlsx";
                    break;
                case "presentation":
                    ext = ".pptx";
                    break;
                default:
                    return;
            }
            var demoName = (string.IsNullOrEmpty(sample) ? "new" : "demo") + ext;
            string path = DateTime.UtcNow.ToString("yyyyMMdd");
            oldFIleName = DefaultConfigTools.GetCorrectName(demoName,path);

            var filePath = DefaultConfigTools.StoragePath(oldFIleName);
            File.Copy(HttpRuntime.AppDomainAppPath + "app_data/" + demoName, filePath);

            var histDir = DefaultConfigTools.HistoryDir(filePath);
            Directory.CreateDirectory(histDir);
            File.WriteAllText(Path.Combine(histDir, "createdInfo.json"), new JavaScriptSerializer().Serialize(new Dictionary<string, object> {
                { "created", DateTime.Now.ToString() },
                { "id", request.Cookies.GetOrDefault("uid", "1") },
                { "name", request.Cookies.GetOrDefault("uname", "eci") }
            }));
        }


        public void PageLoad()
        {
            string fileName = string.Empty;
            var externalUrl = HttpContext.Current.Request["fileUrl"];
            var userId = HttpContext.Current.Request["userId"];
            var userName = HttpContext.Current.Request["userName"];
            string fileId = HttpContext.Current.Request["fileid"];

            bool canedit = false;

            string editstr = HttpContext.Current.Request["canedit"];
            if (!string.IsNullOrEmpty(editstr))
            {
                bool.TryParse(editstr,out canedit);
            }
          
            var canmodifyContentControl = true;
            bool candownload = false;

            var downloadstr = HttpContext.Current.Request["candownload"];
            if (!string.IsNullOrEmpty(downloadstr))
            {
                bool.TryParse(downloadstr, out candownload);
            }
            string created = string.Empty;
            string author = string.Empty;
            FileInfomation fileInfomation = DbClient.GetFileInfomation(fileId);
            var title = string.Empty;
            if (fileInfomation!=null)
            {
                title = fileInfomation.oldfilename;
                created = fileInfomation.createtime.ToString("yyyy-MM-dd HH:mm:ss");
                author = fileInfomation.createusername;
            }

            if (!string.IsNullOrEmpty(externalUrl))
            {
                fileName = DefaultConfigTools.DoUpload(externalUrl, HttpContext.Current.Request);
            }
            else
            {
                fileName = fileInfomation.newfilename;
            }
            title = string.IsNullOrEmpty(title) ? fileName : title;
            var type = HttpContext.Current.Request["type"];
            if (!string.IsNullOrEmpty(type))
            {
                Try(type, HttpContext.Current.Request["sample"], HttpContext.Current.Request);
                HttpContext.Current.Response.Redirect("doceditor.aspx?fileID=" + HttpUtility.UrlEncode(fileName)+ "&userid="+userId+ "&userName="+ userName);
            }

            var ext = Path.GetExtension(fileName);

            var editorsMode = HttpContext.Current.Request.GetOrDefault("editorsMode", "edit");

            var canEdit = DefaultConfigTools.EditedExts.Contains(ext);
            var mode = canEdit && editorsMode != "view" ? "edit" : "view";

            var jss = new JavaScriptSerializer();

            var actionLink = HttpContext.Current.Request.GetOrDefault("actionLink", null);
            var actionData = string.IsNullOrEmpty(actionLink) ? null : jss.DeserializeObject(actionLink);
           
            var config = new Dictionary<string, object>
                {
                    { "type",  HttpContext.Current.Request.GetOrDefault("editorsType", "desktop") },
                    { "documentType", DefaultConfigTools.DocumentType(fileName) },
                    {
                        "document", new Dictionary<string, object>
                            {
                                { "title", title },
                                { "url", FileUri(fileName) },
                                { "fileType", ext.Trim('.') },
                                { "key", Key(fileName) },
                                {
                                    "info", new Dictionary<string, object>
                                        {
                                            { "owner", author },
                                            { "uploaded",created }
                                        }
                                },
                                {
                                    "permissions", new Dictionary<string, object>
                                        {
                                            { "comment",canedit},
                                            { "download", candownload },
                                            { "edit", canedit },
                                            { "modifyFilter",canedit },
                                            { "modifyContentControl", canedit },
                                            { "review", canedit }
                                            //{ "comment", editorsMode != "view" && editorsMode != "fillForms" && editorsMode != "embedded" && editorsMode != "blockcontent"},
                                            //{ "download", true },
                                            //{ "edit", canEdit && (editorsMode == "edit" || editorsMode == "filter") || editorsMode == "blockcontent" },
                                            //{ "fillForms", editorsMode != "view" && editorsMode != "comment" && editorsMode != "embedded" && editorsMode != "blockcontent" },
                                            //{ "modifyFilter", editorsMode != "filter" },
                                            //{ "modifyContentControl", editorsMode != "blockcontent" },
                                            //{ "review", editorsMode == "edit" || editorsMode == "review" }
                                        }
                                }
                            }
                    },
                    {
                        "editorConfig", new Dictionary<string, object>
                            {
                                { "actionLink", actionData },
                                { "mode", mode },
                                { "lang",  HttpContext.Current.Request.Cookies.GetOrDefault("ulang", "en") },
                                { "callbackUrl", CallbackUrl(fileName) },
                                {
                                    "user", new Dictionary<string, object>
                                        {
                                            { "id",  userId},
                                            { "name",  userName }
                                        }
                                },
                                {
                                    "embedded", new Dictionary<string, object>
                                        {
                                            { "saveUrl", FileUri(fileName) },
                                            { "embedUrl", FileUri(fileName) },
                                            { "shareUrl", FileUri(fileName) },
                                            { "toolbarDocked", "top" }
                                        }
                                },
                                {
                                    "customization", new Dictionary<string, object>
                                        {
                                            { "about", true },
                                            { "feedback", true },
                                            {
                                                "goback", new Dictionary<string, object>
                                                    {
                                                        { "url", DefaultConfigTools.Host + "default.aspx" }
                                                    }
                                            }
                                        }
                                }
                            }
                    }
                };

            if (JwtManager.Enabled)
            {
                var token = JwtManager.Encode(config);
                config.Add("token", token);
            }
            DocConfig =Newtonsoft.Json.JsonConvert.SerializeObject(config);
           // DocConfig = jss.Serialize(config);

            try
            {
                Dictionary<string, object> hist;
                Dictionary<string, object> histData;
                GetHistory(out hist, out histData, fileName);
                if (hist != null && histData != null)
                {
                    History = jss.Serialize(hist);
                    HistoryData = jss.Serialize(histData);
                }
            }
            catch(Exception ex) {

                var error = ex.Message;
            
            }
        }



    }
}