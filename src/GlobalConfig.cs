using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace OnlineEditorsExample
{
    public class GlobalConfig
    {
   
        private static List<string> OnlyOfficeServerIp;

        private static int CurrentIpIndex = 0;
        private static List<string> GetInstance()
        {
            if (OnlyOfficeServerIp==null)
            {

                OnlyOfficeServerIp = WebConfigurationManager.AppSettings["OnlyOfficeServerIp"].Split(',').ToList();
     

            }
            return OnlyOfficeServerIp;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="opType">获取服务器地址的方法</param>
        /// <returns></returns>
        public static string GetOnlyOfficeServerIp(int opType=1)
        {
            GetInstance();
            string result = string.Empty;
            if (OnlyOfficeServerIp != null&& OnlyOfficeServerIp.Count > 0)
            {
                switch (opType)
                {
                    case 1: //轮询
                        if (CurrentIpIndex== (OnlyOfficeServerIp.Count-1))
                        {
                            CurrentIpIndex = 0;
                        }
                        else
                        {
                            CurrentIpIndex = CurrentIpIndex+1;
                        }

                        result = OnlyOfficeServerIp[CurrentIpIndex];


                        break;
                    case 2:  //随机、
                        if (OnlyOfficeServerIp.Count>0)
                        {
                            result = OnlyOfficeServerIp[RandomNext() % OnlyOfficeServerIp.Count];

                        }


                        break;


                    default:
                        break;

                }



            }





            return result;


        }



        private static Random random = new Random();

        private static int RandomNext()
        {
            lock (random)
            {
                return random.Next();
            }
        }
    }
}