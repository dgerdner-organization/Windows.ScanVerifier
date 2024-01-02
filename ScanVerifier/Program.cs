using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CS2010.Common;
using System.Configuration;

namespace ScanVerifier
{
    class Program
    {
        static StringBuilder emailMsg = new StringBuilder();

        static void Main(string[] args)
        {
            Console.WriteLine("Scan Verification Report - Version: {0}", typeof(Program).Assembly.GetName().Version.ToString());

            clsVerificationResults vr = new clsVerificationResults();

            try
            {
                if (!ConnectToDatabase("SCANP", "scan_owner", "r1dg3m0n", "SCAN"))
                {
                    vr.connect_to_database = false;
                }
                else
                {
                    vr.connect_to_database = true;
                    clsScanVerify sv = new clsScanVerify();
                    sv.Verify(vr);
                }

                using (clsVerificationEmail ve = new clsVerificationEmail())
                {
                    ve.SendEmail(vr);
                }

            }
            catch (Exception ex)
            {
                msgWrite("Error: " + ex.Message);
                ClsErrorHandler.LogException(ex);
            }
        }

        public static void msgWrite(string msg)
        {
            Console.WriteLine(msg + " " + DateTime.Now.ToString());
            Audit(msg, true);
        }

        public static void Audit(string msg)
        {
            Audit(msg, false);
        }

        public static void Audit(string msg, bool banner)
        {

            emailMsg.AppendLine(string.Format("{0}\t{1}", DateTime.Now.ToString(), msg));

            string audit_file = clsAppConfig.AuditFile.Replace("[date]",
                string.Format("{0}.{1}.{2}", DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString(), DateTime.Now.Day.ToString()));

            StreamWriter sw = File.AppendText(audit_file);

            sw.WriteLine(
                (msg.IsNull()) ?
                    string.Empty :
                    (banner) ? string.Format("{0}\t{1}", DateTime.Now.ToString(), msg) : msg
                    );
            sw.Close();
        }

        private static bool ConnectToDatabase(string dbConnectSection, string user, string pwd, string connKey)
        {
            try
            {
                string strConn = ConfigurationManager.ConnectionStrings[dbConnectSection].ConnectionString;

                if (strConn.IsNullOrWhiteSpace()) return false;

                strConn = strConn.Replace("<user>", user);
                strConn = strConn.Replace("<pwd>", pwd);

                ClsConnection conScan = new ClsConnection(strConn, "Oracle.DataAccess.Client");
                conScan.DbConnectionKey = connKey;
                ClsConMgr.Manager.AddConnection(conScan);

                object o = conScan.GetScalar("Select 1 from dual");
                return (!o.IsNull());
            }
            catch (Exception ex)
            {
                msgWrite("Error: " + ex.Message);
                ClsErrorHandler.LogException(ex);
                return false;
            }
        }
    }
}
