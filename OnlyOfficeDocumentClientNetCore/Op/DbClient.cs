using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SqlSugar;
namespace OnlyOfficeDocumentClientNetCore.Op
{
    public class DbClient
    {

        private static string connstring = "";
        public static SqlSugarClient GetInstance()
        {
            if (string.IsNullOrEmpty(connstring))
            {
                connstring = Common.Appsettings.app(new string[] { "ConnectionStrings", "postgresql" });
            }


            //创建数据库对象
            SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = connstring,//连接符字串
                DbType = DbType.PostgreSQL,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute//从特性读取主键自增信息
            });

            ////添加Sql打印事件，开发中可以删掉这个代码
            //db.Aop.OnLogExecuting = (sql, pars) =>
            //{
            //    Console.WriteLine(sql);
            //};
            return db;
        }

    }
}
