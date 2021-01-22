using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlyOfficeDocumentClientNetCore.Model
{
    public class OpResult
    {   /// <summary>
        /// 返回值 0成功，其他失败
        /// </summary>
        public int Code { get; set; } = 0;

        public string ErrorMessage { get; set; } = string.Empty;

        public object Result { get; set; }
    }
}
