using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CS2010.Common;

namespace ScanVerifier
{
    public class clsVerificationEmail : IDisposable
    {
        private string html = @"
            <html>

                <head>
                    <title>AP / JE Scan Verification</title>

                    <style>

                        html, body {
                            font-family: Calibri;
                        }

                        .good {
                            color: green;
                        }

                        .bad {
                            color: red;
                        }

                        table {
                            border: 1px solid black;
                            width: 500px;
                            text-align: center;
                            background-color: #111111;
                            padding: 0px;
                            margin: 0px;
                            color: #ffffff;
                        }

                        td{
                            border: 1px solid black;
                            padding: 0px;
                            margin: 0px;
                            background-color: #ffffff;
                            color: #000000;
                        }

                    </style>


                </head>

                <body>

                    <h1>AP / JE Scan Verifier for [DATE_TIME]</h1>
                    <div>Connect to database: <span class='[CONNECT_TO_DATABASE_CLASS]'>[CONNECT_TO_DATABASE]</span> ...</div>

                    <hr />

                    <h1>Scenario # 1: Compare the 'Database' to the 'AP / JE File Archives'</h1>
                    <span>If <span class='bad'>problems</span> are found then this indicates that the 'Database' and the 'AP / JE Archives' are NOT in sync.</span>
                    <table>

                        <tr>
                            <th>Type</th>
                            <th>Database Count</th>
                            <th>Archive</th>
                            <th>Problems ?</th>
                        </tr>

                        <tr>
                            <td>AP</td>
                            <td>[S1_AP_DATABASE_CT]</td>
                            <td>[S1_AP_ARCHIVE_CT]</td>
                            <td class='[S1_AP_PROBLEMS_CLASS]'>[S1_AP_PROBLEMS]</td>
                        </tr>

                        <tr>
                            <td>JE</td>
                            <td>[S1_JE_DATABASE_CT]</td>
                            <td>[S1_JE_ARCHIVE_CT]</td>
                            <td class='[S1_JE_PROBLEMS_CLASS]'>[S1_JE_PROBLEMS]</td>
                        </tr>

                    </table>

                    <hr />

                    <h1>Scenario # 2: Compare the 'File Repository' to the 'AP / JE Archive'</h1>
                    <span>If <span class='bad'>problems</span> are found then this indicates that there are files in the 'File Repository' that DO exist in the 'AP / JE Archives' and either we encountered a permissions issue, a delete operation failed or some other error ... manual intervention is needed.</span>

                    <table>

                        <tr>
                            <th>Type</th>
                            <th>File Repository</th>
                            <th>Archive</th>
                            <th>Problems ?</th>
                        </tr>

                        <tr>
                            <td>AP</td>
                            <td>[S2_AP_REPOSITORY]</td>
                            <td>[S2_AP_ARCHIVE]</td>
                            <td class='[S2_AP_PROBLEMS_CLASS]'>[S2_AP_PROBLEMS]</td>
                        </tr>

                        <tr>
                            <td>JE</td>
                            <td>[S2_JE_REPOSITORY]</td>
                            <td>[S2_JE_ARCHIVE]</td>
                            <td class='[S2_JE_PROBLEMS_CLASS]'>[S2_JE_PROBLEMS]</td>
                        </tr>

                    </table>

                    <h3>List of 'problem' files in the 'AP File Repository'.</h3>
                    [S2_AP_LIST_OF_FILES]

                    <h3>List of 'problem' files in the 'JE File Repository'.</h3>
                    [S2_JE_LIST_OF_FILES]

                    <hr />

                    <h1>Sources</h1>
                    <table>

                        <tr>
                            <td>Database</td>
                            <td>SCANP</td>
                        </tr>
                        <tr>
                            <td>File </td>
                            <td>[FILE_REPOSITORY]</td>
                        </tr>
                        <tr>
                            <td>AP Archive</td>
                            <td>[AP_FOLDER]</td>
                        </tr>
                        <tr>
                            <td>JE Archive</td>
                            <td>[JE_FOLDER]</td>
                        </tr>



                    </table>


                </body>


                </html>
            
            ";

        public void SendEmail(clsVerificationResults vr)
        {
            try
            {
                string h = html;

                h = h.Replace("[DATE_TIME]", DateTime.Now.ToShortDateString());
                h = h.Replace("[CONNECT_TO_DATABASE]", (vr.connect_to_database) ? "Good" : "NO");
                h = h.Replace("[CONNECT_TO_DATABASE_CLASS]", (vr.connect_to_database) ? "good" : "bad");

                h = h.Replace("[S1_AP_DATABASE_CT]", vr.s1_ap_database_ct.ToString());
                h = h.Replace("[S1_AP_ARCHIVE_CT]", vr.s1_ap_archive_ct.ToString());
                h = h.Replace("[S1_AP_PROBLEMS]", (vr.s1_ap_problems) ? "YES" : "None");
                h = h.Replace("[S1_AP_PROBLEMS_CLASS]", (vr.s1_ap_problems) ? "bad" : "good");

                h = h.Replace("[S1_JE_DATABASE_CT]", vr.s1_je_database_ct.ToString());
                h = h.Replace("[S1_JE_ARCHIVE_CT]", vr.s1_je_archive_ct.ToString());
                h = h.Replace("[S1_JE_PROBLEMS]", (vr.s1_je_problems) ? "YES" : "None");
                h = h.Replace("[S1_JE_PROBLEMS_CLASS]", (vr.s1_je_problems) ? "bad" : "good");

                h = h.Replace("[S2_AP_REPOSITORY]", vr.s2_ap_file_repository.ToString());
                h = h.Replace("[S2_AP_ARCHIVE]", vr.s2_ap_archive.ToString());
                h = h.Replace("[S2_AP_PROBLEMS]", (vr.s2_ap_problems) ? "YES" : "None");
                h = h.Replace("[S2_AP_PROBLEMS_CLASS]", (vr.s2_ap_problems) ? "bad" : "good");

                h = h.Replace("[S2_JE_REPOSITORY]", vr.s2_je_file_repository.ToString());
                h = h.Replace("[S2_JE_ARCHIVE]", vr.s2_je_archive.ToString());
                h = h.Replace("[S2_JE_PROBLEMS]", (vr.s2_je_problems) ? "YES" : "None");
                h = h.Replace("[S2_JE_PROBLEMS_CLASS]", (vr.s2_je_problems) ? "bad" : "good");

                if (vr.s2_ap_files.IsNull())
                {
                    h = h.Replace("[S2_AP_LIST_OF_FILES]", "-None-");
                }
                else if (vr.s2_ap_files.Count == 0)
                {
                    h = h.Replace("[S2_AP_LIST_OF_FILES]", "-None-");
                }
                else
                {
                    StringBuilder sap_html = new StringBuilder();
                    foreach (string sap in vr.s2_ap_files)
                    {
                        sap_html.AppendFormat("<div>{0}</div>", sap);
                    }
                    h = h.Replace("[S2_AP_LIST_OF_FILES]", sap_html.ToString()); 
                }

                if (vr.s2_je_files.IsNull())
                {
                    h = h.Replace("[S2_JE_LIST_OF_FILES]", "-None-");
                }
                else if (vr.s2_je_files.Count == 0)
                {
                    h = h.Replace("[S2_JE_LIST_OF_FILES]", "-None-");
                }
                else
                {
                    StringBuilder sje_html = new StringBuilder();
                    foreach (string sje in vr.s2_je_files)
                    {
                        sje_html.AppendFormat("<div>{0}</div>", sje);
                    }
                    h = h.Replace("[S2_JE_LIST_OF_FILES]", sje_html.ToString());
                }

                h = h.Replace("[FILE_REPOSITORY]", clsAppConfig.FileRepository);
                h = h.Replace("[AP_FOLDER]", clsAppConfig.APFolder);
                h = h.Replace("[JE_FOLDER]", clsAppConfig.JEFolder);

                Program.Audit(h);

                ClsEmail e = new ClsEmail();

                e.To = clsAppConfig.EmailTo;
                e.From = clsAppConfig.EmailTo;
                e.Subject = "AP / JE SCAN VERIFIER";
                e.Body = h;
                e.SMTPServer = clsAppConfig.SMTPServer;
                //e.SMTP_PORT = clsAppConfig.SMTPPort;
                e.SendMail(true);

            }
            catch (Exception ex)
            {
                ClsErrorHandler.LogException(ex);
            }
        }


        public void Dispose()
        {
            // Do nothing
        }
    }
}
