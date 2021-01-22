using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OnlyOfficeDocumentClientNetCore.Common;
using OnlyOfficeDocumentClientNetCore.Model;
using OnlyOfficeDocumentClientNetCore.Op;
using System.IO;
namespace OnlyOfficeDocumentClientNetCore.Op
{
    public class ConfigOp
    {


        public static FileInfomation GetOne(string fileId)
        {
            return FileInfomationDal.GetOne(fileId);
        }

        public static string GenerateRevisionId(string expectedKey)
        {
            if (expectedKey.Length > 20) expectedKey = expectedKey.GetHashCode().ToString();
            var key = System.Text.RegularExpressions.Regex.Replace(expectedKey, "[^0-9-.a-zA-Z_=]", "_");
            return key.Substring(key.Length - Math.Min(key.Length, 20));
        }
        public static string Key(string fileName)
        {

            return GenerateRevisionId( "/" + Path.GetFileName(Tools.FileUri(fileName))
                                                       + "/" + File.GetLastWriteTime(Tools.StoragePath(fileName)).GetHashCode());

        }
        public OpResult GetDisplayPageConfig(string fileId,string  userId,string userName,bool canEdit,bool canDownload)
        {
            OpResult result = new OpResult();
            string str = "{\"type\": \"desktop\",\"documentType\": \"text\",\"document\": {	\"title\": \"说明.docx\",\"url\": \"http://192.168.26.22:8091/getfile.ashx?fileid=53259a15dbb44deda722dd96dc774d87.docx&sign=9D0BADAC80A902F151EC03EED736877505CEFA6536DEEC49A277241FD767CB40BC58C9AC01BDC8CA255501E7CBB124C7B9C6009320CEFAF2C082894583ED875F59C23489E080B2FE286945051D33C7AB2F64E5033475EB1EA98AD48776AFEB95B778EED7DC548D939476FFC7C7188773\",	\"fileType\": \"docx\",	\"key\": \"1159266715\",	\"info\": {\"owner\": \"\",	\"uploaded\": \"2021-01-20 00:00:00\"	},\"permissions\": {	\"comment\": true,\"download\": false,\"edit\": true,	\"modifyFilter\": true,\"modifyContentControl\": true,	\"review\": true}},\"editorConfig\": {\"actionLink\": null,\"mode\": \"edit\",\"lang\": \"zh\",\"callbackUrl\": \"http://192.168.26.22:8091/webeditor.ashx?type=track&fileid=53259a15dbb44deda722dd96dc774d87.docx\",\"user\": {	\"id\": \"123\",\"name\": \"yyy\"	},\"embedded\": {	\"saveUrl\": \"http://192.168.26.22:8091/getfile.ashx?fileid=53259a15dbb44deda722dd96dc774d87.docx&sign=9D0BADAC80A902F151EC03EED736877505CEFA6536DEEC490749F3DDF6CCD708A69F7C4BFD688038A67581B2BCA886F4C8096B1CCCEADED0ECBAD25EF3EC6BA77C0118D8C5C90119C3EFA042059B6C0D9D46F71FDF9E072073DEE942270BB9266D32ADEAC3831CF7287E5B650CB91B73\",	\"embedUrl\": \"http://192.168.26.22:8091/getfile.ashx?fileid=53259a15dbb44deda722dd96dc774d87.docx&sign=9D0BADAC80A902F151EC03EED736877505CEFA6536DEEC49606BCCFFB638CAD963AC929739789176561983F700FD392C845966D26EA74F32981FB89AD0E187756C1A80759118CDF6500F52B5637630FE3684842776C532AF6D35ABB335543C965544CCCC3530F1208CB5A40E2A522FCC\",	\"shareUrl\": \"http://192.168.26.22:8091/getfile.ashx?fileid=53259a15dbb44deda722dd96dc774d87.docx&sign=9D0BADAC80A902F151EC03EED736877505CEFA6536DEEC49606BCCFFB638CAD963AC929739789176561983F700FD392C845966D26EA74F32981FB89AD0E187756C1A80759118CDF6500F52B5637630FE3684842776C532AF6D35ABB335543C965544CCCC3530F1208CB5A40E2A522FCC\",	\"toolbarDocked\": \"top\"},\"customization\": {	\"about\": true,	\"feedback\": true,\"goback\": {	\"url\": \"http://192.168.26.22:8091/default.aspx\"}}} }";

            FileInfomation fileInfomation = FileInfomationDal.GetOne(fileId);
            string fileName = string.Empty;
            if (fileInfomation==null)
            {
                result.Code = 1;
                result.ErrorMessage = "无此文件";
                return result;

            }
            else
            {
                fileName = fileInfomation.newfilename;
            }
            var ext = Path.GetExtension(fileName);
      


            var config = new Dictionary<string, object>
                {
                    { "type",   "desktop" },
                    { "documentType", Tools.DocumentType(fileName) },
                    {
                        "document", new Dictionary<string, object>
                            {
                                { "title", fileInfomation.oldfilename },
                                { "url", Tools. FileUri(fileName) },
                                { "fileType", ext.Trim('.') },
                                { "key", Key(fileName) },
                                {
                                    "info", new Dictionary<string, object>
                                        {
                                            { "owner", fileInfomation.createusername },
                                            { "uploaded",fileInfomation.createtime }
                                        }
                                },
                                {
                                    "permissions", new Dictionary<string, object>
                                        {
                                            { "comment",canEdit},
                                            { "download", canDownload },
                                            { "edit", canEdit },
                                            { "modifyFilter",canEdit },
                                            { "modifyContentControl", canEdit },
                                            { "review", canEdit }
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
                                { "actionLink", null },
                                { "mode",canEdit ? "edit" : "view" },
                                { "lang",  "en" },//en 
                                { "callbackUrl", Tools.CallbackUrl(fileName) },
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
                                            { "saveUrl", Tools.FileUri(fileName) },
                                            { "embedUrl", Tools.FileUri(fileName) },
                                            { "shareUrl", Tools.FileUri(fileName) },
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
                                                        { "url", Tools.host + "default.aspx" } //失败返回的页面
                                                    }
                                            }
                                        }
                                }
                            }
                    }
                };






            //var a = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(str);

            //string b = Common.JwtManager.Encode(a);




            return result;
        }





    }
}
