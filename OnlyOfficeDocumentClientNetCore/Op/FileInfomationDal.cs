using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OnlyOfficeDocumentClientNetCore.Model;
namespace OnlyOfficeDocumentClientNetCore.Op
{
    public class FileInfomationDal
    {

        public static FileInfomation GetOne(string fileId)
        {
           return DbClient.GetInstance().Queryable<FileInfomation>().First(it => it.id == fileId);
        }


        public static bool Insert(FileInfomation model)
        {
          return  DbClient.GetInstance().Insertable(model).ExecuteCommand()>0;
        }




        public static bool Update(FileInfomation model)
        {
            return DbClient.GetInstance().Updateable(model).ExecuteCommand() > 0;
        }

        public static bool Delete(string fileId)
        {
            return DbClient.GetInstance().Deleteable<FileInfomation>(it=>it.id==fileId).ExecuteCommand() > 0;
        }



    }
}
