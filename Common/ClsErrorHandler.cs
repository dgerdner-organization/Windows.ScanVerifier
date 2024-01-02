using System;
using System.Data;
using System.Text;
using System.Net.Mail;
using System.Diagnostics;
using System.Configuration;
using System.Security.Principal;
using System.Collections.Generic;
using Oracle.DataAccess.Client;

namespace CS2010.Common
{
	public class ClsException : ApplicationException
	{
		#region Fields

		protected bool _LogException;

		#endregion		// #region Fields

		#region Properties

		public bool LogException { get { return _LogException; } }

		#endregion		// #region Properties

		#region Constructors

		public ClsException(bool logMsg, string msg, params object[] args)
			: base(string.Format(msg, args))
		{
			_LogException = logMsg;
		}

		public ClsException(bool logMsg, Exception innerEx, string msg, params object[] args)
			: base(string.Format(msg, args), innerEx)
		{
			_LogException = logMsg;
		}
		#endregion		// #region Constructors
	}

	public static class ClsErrorHandler
	{
		#region Fields

		private static EventLog ApplicationLog;
		private static EventLog DebugLog;
		private static Dictionary<int, string> ErrorTable;

		#endregion		// #region Fields

		#region Properties

		public static string User
		{
			get
			{
				try
				{
					WindowsIdentity user = WindowsIdentity.GetCurrent();
					return ( user != null ) ? user.Name : "Unknown";
				}
				catch( Exception ex )
				{
					Trace.WriteLine(ex.Message);
					return "Error";
				}
			}
		}

		private static string MachineName
		{
			get
			{
				try
				{
					string name = System.Net.Dns.GetHostName();
					return !string.IsNullOrEmpty(name) ? name : "Unknown";
				}
				catch( Exception ex )
				{
					Trace.WriteLine(ex.Message);
					return "Error";
				}
			}
		}
		#endregion		// #region Properties

		#region Constructors

		static ClsErrorHandler()
		{
			try
			{
				ErrorTable = new Dictionary<int, string>();

				DebugLog = null;
				ApplicationLog = null;

				CreateDebugLog();
				CreateApplicationLog();
			}
			catch (Exception ex)
			{
				WriteDebugLog("Exception in static ClsErrorHandler constructor: {0}", ex.Message);
			}
		}

		private static void CreateDebugLog()
		{
			try
			{
				DebugLog = new EventLog("Application", ".", "AAL");
				Console.WriteLine("DebugLog created successfully");
			}
			catch (Exception ex)
			{
				Console.WriteLine("Creating Debug Log failed: " + ex.Message);
			}
		}

		private static void CreateApplicationLog()
		{
			try
			{
				string evLogName = ClsConfig.Log.EventLogName;
				string evSource = ClsConfig.Log.EventLogSource;
				if (!EventLog.SourceExists(evSource))
					EventLog.CreateEventSource(evSource, evLogName);
				else
				{
					string srcLogName = EventLog.LogNameFromSourceName(evSource, ".");
					if (string.Compare(srcLogName, evLogName, true) != 0)
					{
						WriteDebugLog("Event Source {0} is already registered to another event log. Using {1} instead of {2}",
							evSource, srcLogName, evLogName);
						evLogName = srcLogName;
						// If something like this happens, run the event manager utility. In the
						// utility use the delete event source button, then delete the log button,
						// then reboot the server. Go back into the utility and use the
						// create event source button. Had this problem with the ArcSys source
						// and the ARC log.
					}
				}
				ApplicationLog = new EventLog(evLogName, ".", evSource);
				Console.WriteLine("Application Log created successfully {0} {1}", evLogName, evSource);
			}
			catch (Exception ex)
			{
				WriteDebugLog("Create Log failed Name: {0} Source: {1}\r\n{2}",
					ClsConfig.Log.EventLogName, ClsConfig.Log.EventLogSource, ex.Message);
			}
		}

		private static void WriteDebugLog(string msg, params object[] args)
		{
			string text = string.Format(msg, args);
			if (DebugLog != null)
				DebugLog.WriteEntry(text);
			else
				Console.WriteLine(text);
		}
		#endregion		// #region Constructors

		#region Public Methods

		public static void LoadErrorMessages(DataTable dtError)
		{
			if( dtError == null ) return;

			foreach( DataRow dr in dtError.Rows )
			{
				int? key = ClsConvert.ToInt32Nullable(dr["ERROR_NO"]);
				string val = ClsConvert.ToString(dr["ERROR_MSG"]);
				if( key != null && string.IsNullOrEmpty(val) == false )
					ErrorTable[key.Value] = val;
			}
		}

		public static string LogMessage(string msg, params object[] args)
		{
			return WriteEntry(EventLogEntryType.Information, msg, args);
		}

		public static string LogWarning(string msg, params object[] args)
		{
			return WriteEntry(EventLogEntryType.Warning, msg, args);
		}

		public static string LogError(string msg, params object[] args)
		{
			return WriteEntry(EventLogEntryType.Error, msg, args);
		}

		public static string LogException(Exception ex)
		{
			return LogException(ex, null);
		}

		public static string LogException(Exception ex, ClsBaseTable obj)
		{
			return LogException(ex, obj, null);
		}

		public static string LogException<T>(Exception ex, List<T> lst)
			where T : ClsBaseTable
		{
			string extraInfo = null;
			try
			{
				if( lst != null )
				{
					StringBuilder sb = new StringBuilder();
					foreach( T obj in lst )
						if( obj != null )
							sb.AppendFormat("Object Type: {0}\r\n{1}", obj.GetType().FullName,
								obj.ToString());
					extraInfo = ( sb.Length > 0 ) ? sb.ToString() : null;
				}
			}
			catch
			{
				if (ApplicationLog != null)
					ApplicationLog.WriteEntry("Error logging exception", EventLogEntryType.Error);
				else
					WriteDebugLog("Error logging exception");

			}
			return LogException(ex, null, extraInfo);
		}

		/// <summary>Record the exception to the event logger and return an error
		/// message to display to the user</summary>
		/// <param name="ex">The exception to record</param>
		/// <param name="obj">A business object to report information about</param>
		/// <param name="extraInfo">A string with additional information gathered when the
		/// exception occurred</param>
		/// <returns>An error message</returns>
		/// <remarks>We will check the exception to see if it is an oracle
		/// exception. If it is we will attempt to get the error message from our
		/// error table. Note: it appears that the error number returned in the
		/// oracle exception is positive, while the numbers in the error table are
		/// negative. So after an initial check in the error table with the oracle
		/// error number, we negate the number and check again.</remarks>
		public static string LogException(Exception ex, ClsBaseTable obj, string extraInfo)
		{
			try
			{
				ClsException clsEx = ex as ClsException;
				if( clsEx != null && clsEx.LogException == false ) return ex.Message;

				string error = null;
				OracleException oex = ex as OracleException;
				if( oex != null )
				{
					if( ErrorTable.ContainsKey(oex.Number) == true )
						error = ErrorTable[oex.Number];
					else
					{
						int index = -oex.Number;
						if( ErrorTable.ContainsKey(index) == true )
							error = ErrorTable[index];
					}
				}

				StringBuilder sb = new StringBuilder();
				if( obj != null )
					sb.AppendFormat("\r\nObject Type: {0}\r\n{1}\r\n",
						obj.GetType().FullName, obj.ToString());
				sb.AppendLine(extraInfo);

				if( oex == null || string.IsNullOrEmpty(error) == true )
				{	// Not an oracle exception or no entry in the error table
					WriteEntry(EventLogEntryType.Error, "{0}\r\n{1}\r\n",
						ex.ToString(), sb.ToString());
					return ex.Message;
				}

				WriteEntry(EventLogEntryType.Error,
					"Oracle Exception Message: {0}\r\nError: {1}\r\n" +
					"Source: {2}\r\nStack: {3}\r\n{4}", oex.Message, error,
					oex.Source, oex.StackTrace, sb.ToString());

				SmtpException smtpEx = ex as SmtpException;
				if( smtpEx != null )
				{
					WriteEntry(EventLogEntryType.Error,
						"SMTP Exception Status Code: {0}\r\n", smtpEx.StatusCode);
				}

				return error;
			}
			catch( Exception exNew )
			{
				Trace.WriteLine(exNew.Message);
				return exNew.Message;
			}
		}
		#endregion		// #region Public Methods

		#region Helper Methods

		private static string WriteEntry(EventLogEntryType type, string msg,
			params object[] args)
		{
			try
			{
				string tmp = string.Format(msg, args);
				string s = string.Format
					("WinUser: {0}\r\nMachine {1}\r\nAppUser: {2} (ID {3})\r\n{4} {5}\r\n{6} {7}\r\n{8}",
					User, MachineName, ClsEnvironment.UserName, ClsEnvironment.User_Id,
					AppDomain.CurrentDomain.FriendlyName, ClsEnvironment.Version,
					ClsEnvironment.ConnectionKey, ClsEnvironment.ConnectionStringName,
					tmp);
				if (ApplicationLog != null)
					ApplicationLog.WriteEntry(s, type);
				else if (DebugLog != null)
					DebugLog.WriteEntry(s, type);
				else
					Console.WriteLine("{0} {1}", s, type);
				return s;
			}
			catch( Exception ex )
			{
				string err = "Write Entry to Event Log failed:\r\n" + ex.ToString();
				WriteDebugLog(err);
				return err;
			}
		}
		#endregion		// #region Helper Methods
	}
}