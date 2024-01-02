using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using CS2010.Common;

namespace ScanVerifier
{
    public class clsAppConfig
    {

        public static string APFolder
        {
            get
            {
                return GetAppValue("APFolder");
            }
        }

        public static string JEFolder
        {
            get
            {
                return GetAppValue("JEFolder");
            }
        }

        public static string FileRepository
        {
            get
            {
                return GetAppValue("FileRepository");
            }
        }

        public static string AuditStatus
        {
            get
            {
                return GetAppValue("AuditStatus");
            }
        }

        public static string AuditFile
        {
            get
            {
                string s = GetAppValue("AuditFile");

                s = s.Replace("[date]", DateTime.Now.FormatDate());

                return GetAppValue("AuditFile");
            }
        }

        public static string EmailTo
        {
            get
            {
                return GetAppValue("EmailTo");
            }
        }

        public static string SMTPServer
        {
            get
            {
                return GetAppValue("SMTPServer");
            }
        }

        public static string SMTPPort
        {
            get
            {
                return GetAppValue("SMTPPort");
            }
        }

        private static string GetAppValue(string key)
        {
            string s;

            try
            {
                s = ConfigurationManager.AppSettings[key].ToString();
            }
            catch
            {
                return string.Empty;
            }
            return s;
        }


    }
}
