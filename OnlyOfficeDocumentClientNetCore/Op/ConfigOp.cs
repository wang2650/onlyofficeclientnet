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

        public static bool InsertFileInfomation(string userId,string userName,string OldfileName,string  newFileName, string Id,  string filePath, int appId=1)
        {
            FileInfomation fileInfomation = new FileInfomation();
            fileInfomation.appid = appId;
            fileInfomation.createtime = DateTime.Now;
            fileInfomation.createuserid = userId;
            fileInfomation.createusername = userName;
            fileInfomation.filepath = filePath;
            fileInfomation.filestate = 0;
            fileInfomation.id = Id;
            fileInfomation.newfilename = newFileName;
            fileInfomation.oldfilename = OldfileName;
            fileInfomation.updatetime = fileInfomation.createtime;
            fileInfomation.updateuserid = userId;
            fileInfomation.updateusername = userName;

         return   FileInfomationDal.Insert(fileInfomation);


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
        public static OpResult GetDisplayPageConfig(string fileId,string  userId,string userName,bool canEdit,bool canDownload)
        {
            OpResult result = new OpResult();
            
   
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

            Sign sg = new Sign();
            sg.username = userName;
            sg.userid = userId;
            sg.dt = DateTime.Now;

            FileConfig fcg = new FileConfig();
            fcg.documentType = Tools.DocumentType(fileName);
            fcg.Type = "desktop";
            fcg.document.title = fileInfomation.oldfilename;
            fcg.document.url = Tools.FileUri(fileName, sg);
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
            fcg.editorConfig.embedded.saveUrl = Tools.FileUri(fileName, sg);
            fcg.editorConfig.embedded.embedUrl = Tools.FileUri(fileName, sg);
            fcg.editorConfig.embedded.shareUrl = Tools.FileUri(fileName, sg);
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

        public static bool FileIsExist(string fileId)
        {

            return FileInfomationDal.FileIsExist(fileId);
        }


        /// <summary>
        /// 删除  数据库记录状态改变 
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public static bool DeleteForState(string fileId,int userId,DateTime dt)
        {

            return FileInfomationDal.UpdateState(fileId,userId,dt);
        }




        public static bool Delete(string fileId)
        {
            bool result = false;
            var rs = GetOne(fileId);

            result = FileInfomationDal.Delete(fileId);
            if (result)
            {
                try
                {
                    string filePath = Path.Combine(Common.Tools.VirtualPath + rs.filepath + "/", rs.newfilename);
                    File.Delete(filePath);
                    result= !File.Exists(filePath);
                   
                }
                catch (Exception ex)
                {
                    
                }
            


            }

            return result;
        }


        


}
}
