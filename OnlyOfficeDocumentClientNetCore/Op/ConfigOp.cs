using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OnlyOfficeDocumentClientNetCore.Common;
using OnlyOfficeDocumentClientNetCore.Model;
using OnlyOfficeDocumentClientNetCore.Op;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace OnlyOfficeDocumentClientNetCore.Op
{
    public class ConfigOp
    {
        private static List<string> whiteIp { get; set; } 
        public  static List<string> GetwhiteIp()
        {

            if (whiteIp==null|| whiteIp.Count==0)
            {
                string ips = Common.Appsettings.app(new string[] { "OnlyOffice", "whiteIp" });
                if (string.IsNullOrEmpty(ips))
                {
                    whiteIp = new List<string>();
                }
                else
                {
                    whiteIp = ips.Split(',').ToList();
                }



            }


            return whiteIp;
        }

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
        public static OpResult GetDisplayPageConfig(string fileId,string  userId,string userName,bool canEdit,bool canDownload,string sign)
        {
            OpResult result = new OpResult();

            string str = "{\"type\": \"desktop\",\"documentType\": \"text\",\"document\": {	\"title\": \"说明.docx\",\"url\": \""+Tools.host+ "/getfile.ashx?fileid="+ fileId + "&sign="+sign+"\",	\"fileType\": \"docx\",	\"key\": \"1159266715\",	\"info\": {\"owner\": \"\",	\"uploaded\": \"2021-01-20 00:00:00\"	},\"permissions\": {	\"comment\": true,\"download\": false,\"edit\": true,	\"modifyFilter\": true,\"modifyContentControl\": true,	\"review\": true}},\"editorConfig\": {\"actionLink\": null,\"mode\": \"edit\",\"lang\": \"zh\",\"callbackUrl\": \"" + Tools.host + "/webeditor.ashx?type=track&fileid=53259a15dbb44deda722dd96dc774d87.docx\",\"user\": {	\"id\": \"123\",\"name\": \"yyy\"	},\"embedded\": {	\"saveUrl\": \"" + Tools.host + "/getfile.ashx?fileid=53259a15dbb44deda722dd96dc774d87.docx&sign=9D0BADAC80A902F151EC03EED736877505CEFA6536DEEC490749F3DDF6CCD708A69F7C4BFD688038A67581B2BCA886F4C8096B1CCCEADED0ECBAD25EF3EC6BA77C0118D8C5C90119C3EFA042059B6C0D9D46F71FDF9E072073DEE942270BB9266D32ADEAC3831CF7287E5B650CB91B73\",	\"embedUrl\": \"" + Tools.host + "/getfile.ashx?fileid=53259a15dbb44deda722dd96dc774d87.docx&sign=9D0BADAC80A902F151EC03EED736877505CEFA6536DEEC49606BCCFFB638CAD963AC929739789176561983F700FD392C845966D26EA74F32981FB89AD0E187756C1A80759118CDF6500F52B5637630FE3684842776C532AF6D35ABB335543C965544CCCC3530F1208CB5A40E2A522FCC\",	\"shareUrl\": \"" + Tools.host + "/getfile.ashx?fileid=53259a15dbb44deda722dd96dc774d87.docx&sign=9D0BADAC80A902F151EC03EED736877505CEFA6536DEEC49606BCCFFB638CAD963AC929739789176561983F700FD392C845966D26EA74F32981FB89AD0E187756C1A80759118CDF6500F52B5637630FE3684842776C532AF6D35ABB335543C965544CCCC3530F1208CB5A40E2A522FCC\",	\"toolbarDocked\": \"top\"},\"customization\": {	\"about\": true,	\"feedback\": true,\"goback\": {	\"url\": \"" + Tools.host + "/default.aspx\"}}} }";

            FileInfomation fileInfomation = FileInfomationDal.GetOne(fileId);
            string fileName = string.Empty;
            if (fileInfomation==null)
            {
                result.error = 1;
                result.ErrorMessage = "无此文件";
                return result;

            }
            else
            {
                fileName = fileInfomation.newfilename;
            }
            var ext = Path.GetExtension(fileName);



            FileConfig fcg = new FileConfig();
            fcg.documentType = Tools.DocumentType(fileName);
            fcg.Type = "desktop";
            fcg.document.title = fileInfomation.oldfilename;
            fcg.document.url = Tools.FileUri(fileName);
            fcg.document.fileType = ext.Trim('.');
            fcg.document.key = Key(fileName);
            fcg.document.info.owner = fileInfomation.createusername;
            fcg.document.info.uploaded = fileInfomation.createtime.ToString("yyyy-MM-dd HH:mm:ss");
            fcg.document.permissions.comment = canEdit;
            fcg.document.permissions.download = canDownload;
            fcg.document.permissions.edit = canEdit;
            fcg.document.permissions.modifyContentControl = canEdit;
            fcg.document.permissions.modifyFilter = canEdit;
            fcg.document.permissions.review = canEdit;
            fcg.editorConfig.callbackUrl =Tools.CallbackUrl(fileName);
            fcg.editorConfig.actionLink = null;
            fcg.editorConfig.mode = canEdit ? "edit" : "view";
            fcg.editorConfig.lang = "en";
            fcg.editorConfig.user.id = userId;
            fcg.editorConfig.user.name = userName;
            fcg.editorConfig.embedded.toolbarDocked = "top";
            fcg.editorConfig.embedded.saveUrl = Tools.FileUri(fileName);
            fcg.editorConfig.embedded.embedUrl = Tools.FileUri(fileName);
            fcg.editorConfig.embedded.shareUrl = Tools.FileUri(fileName);
            fcg.editorConfig.customization.about = true;
            fcg.editorConfig.customization.feedback = true;
            fcg.editorConfig.customization.goback.Add("url", Tools.host + Tools.DefaultPage);
            fcg.token = JwtManager.Encode(fcg);
            //fcg.token=
            result.Result = fcg;




            //var a = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(str);

            //string b = Common.JwtManager.Encode(a);




            return result;
        }





    }
}
