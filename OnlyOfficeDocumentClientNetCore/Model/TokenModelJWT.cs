using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlyOfficeDocumentClientNetCore.Model
{
    /// <summary>
    /// 令牌
    /// </summary>
    public class TokenModelJWT
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long Uid { get; set; }


        /// <summary>
        /// 过期时间
        /// </summary>
        public int? ExpDate { get; set; }


        public DateTime LoginDateTime { get; set; }
    }
}
