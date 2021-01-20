
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

using Npgsql;
using PetaPoco;

namespace OnlineEditorsExample.DbOp
{
    public class DbClient
    {

        private static string sqlConn = "SqlConnStr";

        public static FileInfomation GetFileInfomation(string Id)
        {

            Database db = new PetaPoco.Database(sqlConn);
            if (Id.IndexOf(".")>0)
            {
                Id = Id.Substring(0, Id.IndexOf("."));
            }
          

            var lt=  db.Query<FileInfomation>(" select *  from fileinfomation where id='"+Id+"'").ToList();

            if (lt != null && lt.Count>0)
            {
                return lt[0];
            }
            else {
                return null;
                }

     
        }

        public static bool InsertFileInfomation(FileInfomation insertObj)
        {
            bool result = false;
            try
            {


                Database db = new PetaPoco.Database(sqlConn);

                result =db.Execute("INSERT INTO   public.fileinfomation  (id , oldfilename   , newfilename  , createtime , updatetime  , createuserid  , createusername  , updateuserid  , updateusername  , filestate , appid  , filepath) VALUES  (" +
                "'"+insertObj.id+ "', '"+insertObj.oldfilename+ "', '" + insertObj.newfilename+ "' , '" + insertObj.createtime+ "'   , " +
                "'" + insertObj.updatetime+ "' , '" + insertObj.createuserid+ "' , '" + insertObj.createusername+ "'  , '" + insertObj.updateuserid+ "'  , '" + insertObj.updateusername+ "' , '" + insertObj.filestate+ "' ,'" + insertObj.appid+ "','" + insertObj.filepath+ "')")>0;
            }
            catch(Exception ex)
            {
                var a = ex.Message;
            }

            return result;
        }

        public static bool DeleteFileInfomatin(string Id)
        {
            Database db = new PetaPoco.Database(sqlConn);
            return db.Delete<FileInfomation>(Id ) > 0;
        }


        public static bool UpdateFileInfomatin(FileInfomation infomation)
        {
            Database db = new PetaPoco.Database(sqlConn);

            return db.Update(infomation)>0;
        }



    }
}