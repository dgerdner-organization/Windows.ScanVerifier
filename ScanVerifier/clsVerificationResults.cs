using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScanVerifier
{
    public class clsVerificationResults
    {
        public bool connect_to_database { get; set; }

        public long s1_ap_database_ct { get; set; }
        public long s1_je_database_ct { get; set; }
        public long s1_ap_archive_ct { get; set; }
        public long s1_je_archive_ct { get; set; }

        public bool s1_ap_problems 
        {
            get
            {
                return (s1_ap_database_ct != s1_ap_archive_ct);
            }
        }

        public bool s1_je_problems
        {
            get
            {
                return (s1_je_database_ct != s1_je_archive_ct);
            }
        }

        public List<string> s1_ap_files { get; set; }
        public List<string> s1_je_files { get; set; }

        public long s2_ap_file_repository { get; set; }
        public long s2_je_file_repository { get; set; }
        public long s2_ap_archive { get; set; }
        public long s2_je_archive { get; set; }

        public bool s2_ap_problems
        {
            get
            {
                return (s2_ap_archive > 0);
            }
        }

        public bool s2_je_problems
        {
            get
            {
                return (s2_je_archive > 0);
            }
        }

        public List<string> s2_ap_files { get; set; }
        public List<string> s2_je_files { get; set; }

    }
}
