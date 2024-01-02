using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CS2010.Common;
using System.Data;
using System.IO;

namespace ScanVerifier
{
    public class clsScanVerify
    {

        #region Connection Manager

        protected static ClsConnection Connection
        {
            get { return ClsConMgr.Manager["SCAN"]; }
        }

        #endregion		// #region Connection Manager

        public void Verify(clsVerificationResults vr)
        {
            try
            {
                string sql = "";
                DataTable dt = null;

                sql = "Select ap.image_ap_id, ap.folder_nm, ap.file_nm from t_image_ap ap order by ap.image_ap_id";
                dt = Connection.GetDataTable(sql);

                vr.s1_ap_database_ct = dt.Rows.Count;

                foreach (DataRow dr in dt.Rows)
                {
                    if (VerifyFile(dr[1].ToString(), dr[2].ToString()))
                        vr.s1_ap_archive_ct++;
                    else
                    {
                        if (vr.s1_ap_files.IsNull()) vr.s1_ap_files = new List<string>();
                        vr.s1_ap_files.Add(string.Format("AP File {0}\\{1} not found.", dr[1].ToString(), dr[2].ToString()));
                        Program.Audit(string.Format("AP File {0}\\{1} not found.", dr[1].ToString(), dr[2].ToString()));
                    }
                }
                sql = "Select je.image_je_id, je.folder_nm, je.file_nm from t_image_je je order by je.image_je_id";
                dt = Connection.GetDataTable(sql);

                vr.s1_je_database_ct = dt.Rows.Count;

                foreach (DataRow dr in dt.Rows)
                {
                    if (VerifyFile(dr[1].ToString(), dr[2].ToString()))
                        vr.s1_je_archive_ct++;
                    else
                    {
                        if (vr.s1_je_files.IsNull()) vr.s1_je_files = new List<string>();
                        vr.s1_je_files.Add(string.Format("JE File {0}\\{1} not found.", dr[1].ToString(), dr[2].ToString()));
                        Program.Audit(string.Format("JE File {0}\\{1} not found.", dr[1].ToString(), dr[2].ToString()));
                    }
                }

                List<string> lst_ap_file_repository = GetFileList(clsAppConfig.FileRepository, "AP*.PDF");

                foreach (string sap in lst_ap_file_repository)
                {
                    vr.s2_ap_file_repository++;

                    if (VerifyFile(clsAppConfig.APFolder, sap))
                    {
                        vr.s2_ap_archive++;
                        if (vr.s2_ap_files.IsNull()) vr.s2_ap_files = new List<string>();
                        vr.s2_ap_files.Add(sap);
                    }
                }

                List<string> lst_je_file_repository = GetFileList(clsAppConfig.FileRepository, "JE*.PDF");

                foreach (string sje in lst_je_file_repository)
                {
                    vr.s2_je_file_repository++;

                    if (VerifyFile(clsAppConfig.JEFolder, sje))
                    {
                        vr.s2_je_archive++;
                        if (vr.s2_je_files.IsNull()) vr.s2_je_files = new List<string>();
                        vr.s2_je_files.Add(sje);
                    }
                }

            }
            catch (Exception ex)
            {
                Program.msgWrite("Error: " + ex.Message);
                ClsErrorHandler.LogException(ex);
            }
        }

        private bool VerifyFile(string FolderNm, string FileNm)
        {
            try
            {
                string FullFileNm = string.Format("{0}\\{1}", FolderNm, FileNm);
                return VerifyFile(FullFileNm);
            }
            catch (Exception ex)
            {
                ClsErrorHandler.LogException(ex);
                return false;
            }
        }

        private bool VerifyFile(string FullFileNm)
        {
            try
            {
                return System.IO.File.Exists(FullFileNm);
            }
            catch (Exception ex)
            {
                ClsErrorHandler.LogException(ex);
                return false;
            }
        }

        public static List<string> GetFolderList(string strFolderName, string pattern)
        {
            try
            {
                string[] files = Directory.GetFiles(strFolderName, pattern);
                return files.ToList<string>();
            }
            catch (Exception ex)
            {
                ClsErrorHandler.LogException(ex);
                return null;
            }
        }

        public static List<string> GetFileList(string strFolderName, string pattern)
        {
            try
            {
                string[] files = Directory.GetFiles(strFolderName, pattern);

                for (int intX = 0; intX < files.Count(); intX++)
                    files[intX] = System.IO.Path.GetFileName(files[intX]);

                return files.ToList<string>();
            }
            catch (Exception ex)
            {
                ClsErrorHandler.LogException(ex);
                return null;
            }
        }

    }
}
