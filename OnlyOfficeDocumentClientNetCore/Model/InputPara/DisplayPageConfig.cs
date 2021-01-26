using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlyOfficeDocumentClientNetCore.Model.InputPara
{
    public class DisplayPageConfig
    {
        /// <summary>
        /// 是否可以编辑
        /// </summary>
        public bool canEdit { get; set; } = false;
        /// <summary>
        /// 是否可以下载
        /// </summary>
        public bool canDownLoad { get; set; } = true;
        /// <summary>
        /// 文件id
        /// </summary>
        public string fileId { get; set; } ;
    }
}
