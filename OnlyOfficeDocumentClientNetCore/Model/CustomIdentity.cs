using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace OnlyOfficeDocumentClientNetCore.Model
{
    public class CustomIdentity : IIdentity
    {
        #region 用户属性(可自定义更多信息)

        public CustomIdentity(string userName)
        {
            _userName = userName;
        }


        private string _userName;//用户账号
        public string UserName

        {

            get { return _userName; }

        }

        /// <summary>
        /// 
        /// </summary>
        public string AuthenticationType => "JWT";

        /// <summary>
        /// 是否验证
        /// </summary>

        public bool IsAuthenticated

        {

            get { return true; }

        }

        /// <summary>
        /// 返回用户
        /// </summary>

        public string Name

        {

            get { return _userName; }

        }

        #endregion




    }
}
