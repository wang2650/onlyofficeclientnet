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

        public static bool FileIsExist(string fileId)
        {
            return DbClient.GetInstance().Queryable<FileInfomation>().Where(it => it.id == fileId).Any();
        }

        public static bool Insert(FileInfomation model)
        {
          return  DbClient.GetInstance().Insertable(model).ExecuteCommand()>0;
        }




        public static bool Update(FileInfomation model)
        {
            return DbClient.GetInstance().Updateable(model).ExecuteCommand() > 0;
        }
        /// <summary>
        /// 修改状态 默认为删除
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public static bool UpdateState(string fileId,int userId,DateTime dt, int fileState=2)
        {
            return DbClient.GetInstance().Updateable<FileInfomation>().SetColumns(f => f.filestate == fileState).SetColumns(f=>f.updatetime==dt).SetColumns(f=>f.updateuserid==userId.ToString()).Where(f => f.id == fileId.Trim()).ExecuteCommand() > 0;
        }
        public static bool Delete(string fileId)
        {
            return DbClient.GetInstance().Deleteable<FileInfomation>(it=>it.id==fileId).ExecuteCommand() > 0;
        }



    }
}
