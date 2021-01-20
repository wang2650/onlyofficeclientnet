
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PetaPoco;
namespace OnlineEditorsExample.DbOp
{
    [PetaPoco.TableName("fileinfomation")]
    [PetaPoco.PrimaryKey("id", AutoIncrement = false)]
    public class FileInfomation
    {
        /// <summary>
        /// 主键id
        /// </summary>

        public string id { get; set; }
        /// <summary>
        /// 
        /// </summary>

        public string oldfilename { get; set; } = "";


        /// <summary>
        /// 
        /// </summary>

        public string newfilename { get; set; } = "";
        /// <summary>
        /// 创建时间
        /// </summary>

        public DateTime createtime { get; set; } = DateTime.UtcNow;


        public DateTime updatetime { get; set; } = DateTime.UtcNow;


        public string createuserid { get; set; } = "";

        public string createusername { get; set; } = "";

        public string updateuserid { get; set; }

        public string updateusername { get; set; } = "";

        public string filepath { get; set; } = "";
        /// <summary>
        /// 0 正常  1 修改过  2 删除
        /// </summary>

        public int filestate { get; set; } = 0;

        public int appid { get; set; } = 1;

    }

}




