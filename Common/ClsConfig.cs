using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.Diagnostics;
using System.Configuration;
using System.Collections.Specialized;

namespace CS2010.Common
{
	/// <summary>This is a centralized repository for common application
	/// configuration settings (usually specified in app.config file)</summary>
	public static class ClsConfig
	{
		#region Fields

		private static string _SectionName;

		private static LogConfig _LogSection;

		#endregion		// #region Fields

		#region Properties

		/// <summary>Name of section in config file where we will attempt to retrieve the settings.
		/// If the section does not exist or the key does not exist in that section we will fall
		/// back to the "appSettings" section. Note, the section name defaults to the section that
		/// matches the connection string name (handled in the constructor), but the section can
		/// be overriden using this property by setting a new section name.</summary>
		public static string SectionName
		{
			get { return _SectionName; } set { _SectionName = value; }
		}

		/// <summary>Contains the event log configuration values</summary>
		public static LogConfig Log
		{
			get { return _LogSection; }
		}
		#endregion		// #region Properties

		#region Constructors

		/// <summary>Static constructor (called automatically the 1st time
		/// the class is accessed)</summary>
		static ClsConfig()
		{
			_LogSection = new LogConfig();
			_SectionName = ClsEnvironment.ConnectionStringName;
		}
		#endregion		// #region Constructors

		#region Public Methods

		public static string FormatDate(DateTime? aDate)
		{
			return ( aDate != null ) ? aDate.Value.ToString(DateFormat) : string.Empty;
		}

		public static string FormatDate(DateTime? aDate, string format)
		{
			string fmt = string.IsNullOrEmpty(format) ? DateFormat : format.Trim();
			return ( aDate != null ) ? aDate.Value.ToString(fmt) : string.Empty;
		}

		/// <summary>Get a unique filename (without path information)</summary>
		/// <param name="extra">Optional text to be appended to the filename</param>
		/// <param name="ext">Optional file extension</param>
		/// <returns>Filename with format date-time-extra.ext. Date: 4 digit year, 2 digit
		/// month, 2 digit day. Time: hours (24 hour format), minutes, and seconds. Extra
		/// is the optional text that was specified as a parameter.</returns>
		public static string GetUniqueFileName(string extra, string ext)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("{0}-{1}", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"),
				ClsEnvironment.UserName);

			string s = ( string.IsNullOrEmpty(extra) == false ) ? extra.Trim() : null;
			if( string.IsNullOrEmpty(s) == false ) sb.AppendFormat("-{0}", s);

			s = ( string.IsNullOrEmpty(ext) == false ) ? ext.Trim() : null;
			if( string.IsNullOrEmpty(s) == false ) sb.AppendFormat(".{0}", s);

			return sb.ToString();
		}

		/// <summary>Get a unique filename including path. This overload accepts the path
		/// to append to the filename. The overload that does not require a path will use
		/// the default path (ExportDir in the config file).</summary>
		/// <param name="extra">Optional text to be appended to the filename</param>
		/// <param name="ext">Optional file extension</param>
		/// <param name="path">Path to be appended to the filename</param>
		/// <param name="userSubDir">True to add a user sub-directory to the path
		/// (i.e. path\user\file.ext), false to just use the path</param>
		/// <param name="createDir">True to create the full path where the file will
		/// eventually be saved to, false to just return the filename</param>
		/// <returns>Filename including path, format path\date-time-extra.ext. Path may
		/// include a user sub-directory (i.e. path\user\name.ext). See GetUniqueFileName
		/// for more information on the format of the file name.</returns>
		public static string GetUniqueFileNameWithPath(string extra, string ext,
			string path, bool userSubDir, bool createDir)
		{
			string dir = ( userSubDir )
				? Path.Combine(path, ClsEnvironment.UserName) : path;
			string name = GetUniqueFileName(extra, ext);

			if( createDir && Directory.Exists(dir) == false )
				Directory.CreateDirectory(dir);

			return Path.Combine(dir, name);
		}

		/// <summary>Get a unique filename including path. This overload does not expect
		/// the path, and will instead use the default path (ExportDir in the config file).
		/// There is an overload that allows the path to be specified explicitly.</summary>
		/// <param name="extra">Optional text to be appended to the filename</param>
		/// <param name="ext">Optional file extension</param>
		/// <param name="userSubDir">True to add a user sub-directory to the path
		/// (i.e. path\user\file.ext), false to just use the path</param>
		/// <param name="createDir">True to create the full path where the file will
		/// eventually be saved to, false to just return the filename</param>
		/// <returns>Filename including path, format path\date-time-extra.ext. Path may
		/// include a user sub-directory (i.e. path\user\name.ext). See GetUniqueFileName
		/// for more information on the format of the file name.</returns>
		public static string GetUniqueFileNameWithPath(string extra, string ext,
			bool userSubDir, bool createDir)
		{
			return GetUniqueFileNameWithPath(extra, ext, ExportDir, userSubDir, createDir);
		}
		#endregion		// #region Public Methods

		#region Helper Methods

		public static string ReadConfigValue(string key)
		{
			try
			{
				NameValueCollection nvc =
					ConfigurationManager.GetSection(SectionName) as NameValueCollection;

				string val = ( nvc != null ) ? nvc[key] : null;
				if( val == null ) val = ConfigurationManager.AppSettings[key];

				return val;
			}
			catch
			{
				return null;
			}
		}

		public static string ReadStringValue(string key, string defaultVal)
		{
			try
			{
				string val = ReadConfigValue(key);
				return ( val != null ) ? val : defaultVal;
			}
			catch( Exception ex )
			{
				ClsErrorHandler.LogException(ex);
				return defaultVal;
			}
		}

		public static bool ReadBooleanValue(string key, bool defaultVal)
		{
			try
			{
				string s = ReadConfigValue(key);
				string val = !string.IsNullOrEmpty(s) ? s.Trim() : null;
				if( string.IsNullOrEmpty(val) ) return defaultVal;

				bool result = false;
				if( bool.TryParse(val, out result) == false ) return defaultVal;

				return result;
			}
			catch( Exception ex )
			{
				ClsErrorHandler.LogException(ex);
				return defaultVal;
			}
		}

		public static DateTime? ReadDateTimeValue(string key, DateTime? defaultVal)
		{
			try
			{
				string s = ReadConfigValue(key);
				string val = !string.IsNullOrEmpty(s) ? s.Trim() : null;
				if( string.IsNullOrEmpty(val) ) return defaultVal;

				DateTime result;
				if( DateTime.TryParse(val, out result) == false ) return defaultVal;

				return result;
			}
			catch( Exception ex )
			{
				ClsErrorHandler.LogException(ex);
				return defaultVal;
			}
		}

		public static Color ReadColorValue(string key, Color defaultVal)
		{
			try
			{
				string s = ReadConfigValue(key);
				string val = !string.IsNullOrEmpty(s) ? s.Trim() : null;

				return ( !string.IsNullOrEmpty(val) ) ? Color.FromName(val) : defaultVal;
			}
			catch( Exception ex )
			{
				ClsErrorHandler.LogException(ex);
				return defaultVal;
			}
		}

		public static T ReadNumeric<T>(string key, T defaultVal) where T : struct,
			IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
		{
			try
			{
				string s = ReadConfigValue(key);
				string val = !string.IsNullOrEmpty(s) ? s.Trim() : null;
				if( string.IsNullOrEmpty(val) ) return defaultVal;

				T num = new T();
				object[] args = { val, num };
				object o = typeof(T).InvokeMember("TryParse",
					BindingFlags.InvokeMethod, null, num, args);

				bool ok = (bool)o;
				return ( ok ) ? (T)args[1] : defaultVal;
			}
			catch( Exception ex )
			{

				ClsErrorHandler.LogException(ex);
				return defaultVal;
			}
		}
		#endregion		// #region Helper Methods

		#region Config Settings

		public static string DBAccess
		{
			get { return ReadStringValue("DBAccess", null); }
		}

		public static string DateFormat
		{
			get { return ReadStringValue("DateFormat", "yyyy-MM-dd"); }
		}

		public static string ExportDir
		{
			get { return ReadStringValue("ExportDir", @"C:\Temp"); }
		}

		public static string CustomerServiceMsg
		{
			get
			{
				return ReadStringValue("CustomerServiceMsg",
					"Please contact customer service for further assistance");
			}
		}

		public static string CustomerServiceGroup
		{
			get
			{
				return ReadStringValue("CustomerServiceGroup",
					"jroman@amslgroup.com");
			}
		}

		public static string MessageBoxTitle
		{
			get { return ReadStringValue("MessageBoxTitle", "Information"); }
		}

		public static string Title
		{
			get { return ReadStringValue("Title", "");  }
		}

		public static string HelpFile
		{
			get { return ReadStringValue("HelpFile", "App_Help.pdf"); }
		}

		public static string IssueReportingGuide
		{
			get { return ClsConfig.ReadStringValue("IssueReportingHelp", "Issue Reporting Instructions.pdf"); }
		}

		public static byte MaxInsertAttempts
		{
			get { return ReadNumeric<byte>("MaxInsertAttempts", 10); }
		}
		#endregion		// #region Config Settings
	}

	#region Event Log Configuration

	/// <summary>Structure used to get the event logging configuration values
	/// from the app.config file (ClsConfig has a static property of this type
	/// named Log which can be used instead; ClsConfig is a centralized
	/// repository for common application configuration settings).</summary>
	public struct LogConfig
	{
		/// <summary>Gets the name of the Event Log source (this is usually set
		/// to the name of the application in the app.config file)</summary>
		public string EventLogSource
		{
			get
			{
				string s = null;
				try
				{
					s = ConfigurationManager.AppSettings["EventLogSource"];
				}
				catch( Exception ex )
				{
					s = null;
					Trace.WriteLine("Error reading config (EventLogSource)" +
						ex.Message);
				}
				return ( string.IsNullOrEmpty(s) == false ) ? s : "AAL";
			}
		}

		/// <summary>Gets the Event Log Name (multiple applications can use the
		/// same event log, the source of the message is determined by the
		/// EventLogSource property)</summary>
		public string EventLogName
		{
			get
			{
				string s = null;
				try
				{
					s = ConfigurationManager.AppSettings["EventLogName"];
				}
				catch( Exception ex )
				{
					s = null;
					Trace.WriteLine("Error reading config (EventLogName)" +
						ex.Message);
				}
				return ( string.IsNullOrEmpty(s) == false ) ? s : "AAL";
			}
		}

		/// <summary>Gets the name of the machine on which to create the event
		/// log ("." in the config file indicates the local machine)</summary>
		public string EventMachineName
		{
			get
			{
				string s = null;
				try
				{
					s = ConfigurationManager.AppSettings["EventMachineName"];
				}
				catch( Exception ex )
				{
					s = null;
					Trace.WriteLine
						("Error reading config (EventMachineName)" +
						ex.Message);
				}
				return ( string.IsNullOrEmpty(s) == false ) ? s : ".";
			}
		}

		/// <summary>Gets the comma separated list of email addresses that will
		/// receive error/warning messages (usually set to all the members of
		/// the development group)</summary>
		public string EventRecipients
		{
			get
			{
				string s = null;
				try
				{
					s = ConfigurationManager.AppSettings["EventRecipients"];
				}
				catch( Exception ex )
				{
					s = null;
					Trace.WriteLine("Error reading config (EventRecipients)" +
						ex.Message);
				}
				return ( string.IsNullOrEmpty(s) == false )
					? s : "jroman@amslgroup.com";
			}
		}

		/// <summary>Gets the subject of the email message sent to the
		/// development group</summary>
		public string EventEmailSubject
		{
			get
			{
				string s = null;
				try
				{
					s = ConfigurationManager.AppSettings["EventEmailSubject"];
				}
				catch( Exception ex )
				{
					s = null;
					Trace.WriteLine("Error reading config (EventEmailSubject)" +
						ex.Message);
				}
				return ( string.IsNullOrEmpty(s) == false )
					? s : "AAL Application Message";
			}
		}
	}
	#endregion		// #region Event Log Configuration
}