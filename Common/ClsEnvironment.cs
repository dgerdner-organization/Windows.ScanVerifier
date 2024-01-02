using System;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Configuration;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Net;

namespace CS2010.Common
{
	/// <summary>Class providing information about the current execution
	/// environment</summary>
	public class ClsEnvironment
	{
		#region Process Section

		/// <summary>The name of the visual studio process</summary>
		public const string VisualStudioProcessName = "devenv";
		/// <summary>The name of the asp.net process</summary>
		public const string ASPDotNetProcessName = "aspnet_wp";

		/// <summary>Gets the name of the process currently executing</summary>
		public static string ProcessName
		{
			get
			{
				try
				{
					return System.Diagnostics.Process.GetCurrentProcess().ProcessName;
				}
				catch
				{
					return ASPDotNetProcessName;
				}
			}
		}

		/// <summary>Returns true if currently executing in a web browser
		/// (asp.net aspnet_wp.exe) false if not</summary>
		public static bool IsWebMode
		{
			get
			{
				return string.Compare(ProcessName, ASPDotNetProcessName,
					true) == 0;
			}
		}

		/// <summary>Returns true if currently in design mode (executing process
		/// is Visual Studio designer), false if running an application</summary>
		public static bool IsDesignMode
		{
			get
			{
				return string.Compare(ProcessName, VisualStudioProcessName,
					true) == 0;
			}
		}

		/// <summary>Returns true if currently running an application, false if in
		/// design mode (executing process is Visual Studio designer)</summary>
		public static bool IsRunTimeMode { get { return !IsDesignMode; } }

		#endregion		// #region Process Section

		#region User Section

		private static string _Version = null;
		/// <summary>Gets/Sets the value of the application version</summary>
		public static string Version
		{
			get { return _Version; }
			set { _Version = value; }
		}

		private static long? _User_Id = null;
		/// <summary>Gets/Sets the value of User_Id</summary>
		public static long? User_Id
		{
			get { return _User_Id; } set { _User_Id = value; }
		}
		private static string _UserName = null;
		public static string UserName
		{
			get { return _UserName; } set { _UserName = value; }
		}
		private static string _Password = null;
		public static string Password
		{
			get { return _Password; } set { _Password = value; }
		}
		private static string _Contract_Cd;
		public static string Contract_Cd
		{
			get { return _Contract_Cd; } set { _Contract_Cd = value; }
		}
		private static string _User_Control_Cd;
		public static string User_Control_Cd
		{
			get { return _User_Control_Cd; }
			set { _User_Control_Cd = value; }
		}
		private static string _ConnectionKey;
		public static string ConnectionKey
		{
			get { return _ConnectionKey; }
			set { _ConnectionKey = value; }
		}
		private static string _ConnectionStringName;
		public static string ConnectionStringName
		{
			get { return _ConnectionStringName; }
			set { _ConnectionStringName = value; }
		}
		private static string _Database;
		public static string Database
		{
			get { return _Database; }
			set { _Database = value; }
		}

		public static bool IsDeveloper { get { return CheckDeveloper(_UserName); } }

		public static bool IsIT
		{
			get { return CheckDeveloper(_UserName) || CheckInfrastructure(_UserName); }
		}

		public static bool CheckDeveloper(string s)
		{
			return (
				string.Compare(s, "jdorney", true) == 0 ||
				string.Compare(s, "dgerdner", true) == 0 ||
				string.Compare(s, "jroman", true) == 0 ||
				string.Compare(s, "treilly", true) == 0 );
		}

		public static bool CheckInfrastructure(string s)
		{
			return (
				string.Compare(s, "jerrico", true) == 0 ||
				string.Compare(s, "abernabe", true) == 0 ||
				string.Compare(s, "abernabe2", true) == 0 ||
				string.Compare(s, "bminogue", true) == 0 );
		}

		public static bool CheckIT(string s)
		{
			if( CheckDeveloper(s) ) return true;

			return (
				string.Compare(s, "administrator", true) == 0 ||
				string.Compare(s, "snarborough", true) == 0 ||
				string.Compare(s, "jerrico", true) == 0 ||
				string.Compare(s, "abernabe", true) == 0 ||
				string.Compare(s, "abernabe2", true) == 0 ||
				string.Compare(s, "bminogue", true) == 0 );
		}

		public static string CheckDevMachine(string s)
		{
			if( !string.IsNullOrEmpty(s) )
			{
				s = s.ToLower();
				if( s.Contains("dorney") ) return "jdorney";
				if( s.Contains("gerdner") ) return "dgerdner";
				if( s.Contains("roman") ) return "jroman";
				if( s.Contains("reilly") ) return "treilly";
			}
			return null;
		}
		#endregion		// #region User Section

		#region Assembly/Configuration Section

		public static Dictionary<string, string> GetModuleInfo(Assembly anAssembly)
		{
			AssemblyName asmName = anAssembly.GetName();
			Dictionary<string, string> result = new Dictionary<string,string>();
			result.Add("Name", asmName.Name);
			result.Add("Version", asmName.Version.ToString(2));

			foreach( AssemblyName an in anAssembly.GetReferencedAssemblies() )
				if( result.ContainsKey(an.Name) == false )
					result.Add(an.Name, an.Version.ToString());
			return result;
		}

		public static string[] GetDbInfo()
		{
			List<string> lst = new List<string>();
			ConnectionStringSettings css =
				ConfigurationManager.ConnectionStrings
				[ClsEnvironment.ConnectionStringName];

			Dictionary<string, string> kvps =
				ParseConnection(css.ConnectionString);
			lst.Add(css.Name);
			lst.Add(kvps["Data Source"]);
			if( kvps.ContainsKey("Initial Catalog") == true )
				lst.Add(kvps["Initial Catalog"]);
			else
				lst.Add("-");
			lst.Add(css.ProviderName);

			return lst.ToArray();
		}

		/// <summary></summary>
		public static Dictionary<string, string> ParseConnection(string conStr)
		{
			Dictionary<string, string> result = new Dictionary<string, string>();
			if( conStr == null || conStr == string.Empty ) return result;

			string[] kvps = conStr.Split(new char[] { ';' });
			foreach( string s in kvps )
			{
				string[] items = s.Split(new char[] { '=' }, 2);
				if( items != null && items.Length == 2 )
					result.Add(items[0].Trim(), items[1].Trim());
			}
			return result;
		}
		#endregion		// #region Assembly/Configuration Section

		#region Unmanaged Code Section

		[DllImport("advapi32.dll")]
		public static extern int LogonUserA(string lpszUserName,
			string lpszDomain, string lpszPassword, int dwLogonType,
			int dwLogonProvider, ref IntPtr phToken);

		[DllImport("kernel32.dll", CharSet=CharSet.Auto)]
		public static extern bool CloseHandle(IntPtr handle);

		public static bool VerifyDomainAccount(string user, string pwd)
		{
			IntPtr token = IntPtr.Zero;
			int ret = LogonUserA(user, "wlhi", pwd, 3, 0, ref token);
			CloseHandle(token);
			return ret != 0;
		}

		#endregion		// #region Unmanaged Code Section

		#region Printing

		private static string _LastPrinterName;
		private static Dictionary<string, string> _LastPrinters;

		public static string LastPrinterName
		{
			get { return _LastPrinterName; } set { _LastPrinterName = value; }
		}

		public static void UpdateLastPrinter(string docNm, string printerNm)
		{
			LastPrinterName = printerNm;
			if( _LastPrinters == null )
			{
				_LastPrinters = new Dictionary<string, string>();
				_LastPrinters.Add(docNm, printerNm);
			}
			else
			{
				if( _LastPrinters.ContainsKey(docNm) )
					_LastPrinters[docNm] = printerNm;
				else
					_LastPrinters.Add(docNm, printerNm);
			}
		}

		public static string GetLastPrinter(string docNm)
		{
			return ( _LastPrinters != null && _LastPrinters.ContainsKey(docNm) )
				? _LastPrinters[docNm] : _LastPrinterName;
		}
		#endregion

        public static Dictionary<string, string> GlobalDictionary = new Dictionary<string, string>();

        /// <summary>
        /// This is for testing purposes only.  Testing a Global Error Class
        /// JD [2009-04-24]
        /// </summary>
        #region Errors (Testing)

        static public ClsBaseClass ErrorsAndWarnings = new ClsBaseClass();

        #endregion

		#region IP ADdress
		public static string IpAddress
		{
			get
			{
				string url = "http://checkip.dyndns.org";
				System.Net.WebRequest req = System.Net.WebRequest.Create(url);
				System.Net.WebResponse resp = req.GetResponse();
				System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
				string response = sr.ReadToEnd().Trim();
				string[] a = response.Split(':');
				string a2 = a[1].Substring(1);
				string[] a3 = a2.Split('<');
				string a4 = a3[0];
				return a4;
			}
		}
		#endregion
	}
}