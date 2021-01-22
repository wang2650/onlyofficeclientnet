using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlyOfficeDocumentClientNetCore.Model
{

    [SqlSugar.SugarTable("fileinfomation")]
    public class FileInfomation
    {
        /// <summary>
        /// 主键id
        /// </summary>
        [SqlSugar.SugarColumn(IsPrimaryKey = true)]
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

        public DateTime createtime { get; set; } = DateTime.Now;


        public DateTime updatetime { get; set; } = DateTime.Now;


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
