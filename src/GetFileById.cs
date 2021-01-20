﻿using OnlineEditorsExample.DbOp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;

namespace OnlineEditorsExample
{
    public class GetFileById : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            bool isValidate = true;



            string fileid = context.Request["fileid"];
            if (!string.IsNullOrEmpty(fileid))
            {
                FileInfomation fileInfomation = DbClient.GetFileInfomation(fileid);

                if (fileInfomation != null)
                {


                    string fileaname = fileInfomation.newfilename;
                    string path = fileInfomation.filepath;
                    string filepath = WebConfigurationManager.AppSettings["storage-path"] + path + "\\" + fileaname;

                    FileInfo file = new FileInfo(filepath);
                    FileStream myfileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                    byte[] filedata = new Byte[file.Length];
                    myfileStream.Read(filedata, 0, (int)(file.Length));
                    myfileStream.Close();
                    context.Response.Clear();
                    context.Response.ContentType = "application/msword";
                    context.Response.AddHeader("Content-Disposition", "attachment;filename=" + fileInfomation.oldfilename);
                    context.Response.Flush();
                    context.Response.BinaryWrite(filedata);
                    context.Response.End();

                }
            }



        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }


    }
}