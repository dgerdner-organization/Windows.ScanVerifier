using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace CS2010.Common
{
	public class FolderMonitor
	{
		#region Fields/Properties

		private DateTime _Compare_Dt;
		public DateTime Compare_Dt { get { return _Compare_Dt; } }

		private StringBuilder sbEmailBody;
		public string EmailBody { get { return sbEmailBody.ToString(); } }

		private string _EmailSubject;
		public string EmailSubject { get { return _EmailSubject; } }

		private List<FolderItem> _Folders;

		private List<ExtraFileInfo> _AllFiles;
		public List<ExtraFileInfo> AllFiles { get { return _AllFiles; } }

		#endregion		// #region Fields/Properties

		#region Constructors

		public FolderMonitor()
		{
			sbError = new StringBuilder();
			sbInfo = new StringBuilder();

			_Folders = new List<FolderItem>();
			_AllFiles = new List<ExtraFileInfo>();

			FolderNames = new List<string>();
			FolderFilePattern = new List<string>();
			FolderFileAge = new List<double>();

			sbEmailBody = new StringBuilder();

			_Compare_Dt = DateTime.Now;
		}
		#endregion		// #region Constructors

		#region Error/Message Handling

		public StringBuilder sbInfo;
		public StringBuilder sbError;

		public void ResetInfo() { sbInfo.Length = 0; }
		public void ResetErrors() { sbError.Length = 0; }
		public bool HasInfo { get { return sbInfo != null && sbInfo.Length > 0; } }
		public bool HasErrors { get { return sbError != null && sbError.Length > 0; } }
		public string AllMsgs
		{
			get
			{
				string err = sbError.ToString().Trim();
				string info = sbInfo.ToString().Trim();
				bool hasBoth = !string.IsNullOrEmpty(err) && !string.IsNullOrEmpty(info);
				return string.Format("{0}{1}{2}", err, (hasBoth ? "\r\n\r\n" : null), info);
			}
		}

		private void LogInfo(string msg, params object[] args)
		{
			try
			{
				string s = string.Format(msg, args);
				sbInfo.AppendLine(s);
			}
			catch( Exception ex )
			{
				Console.WriteLine("Exception writing info msg: {0}\r\n{1}",
					msg, ex.Message);
			}
		}

		private bool LogError(Exception ex, string msg, params object[] args)
		{
			try
			{
				string s = (msg != null) ? string.Format(msg, args) : string.Empty;
				if( ex != null ) sbError.AppendLine(ex.Message);
				if( !string.IsNullOrEmpty(s) ) sbError.AppendLine(s);
			}
			catch( Exception ex2 )
			{
				Console.WriteLine("Exception writing error msg: {0}\r\n{1}",
					msg, ex2.Message);
			}
			return false;
		}
		#endregion		// #region Error/Message Handling

		#region Configuration

		private string _FolderConfig;
		public string FolderConfig { get { return _FolderConfig; } }

		private string _MatchConfig;
		public string MatchConfig { get { return _MatchConfig; } }

		private string _AgeConfig;
		public string AgeConfig { get { return _AgeConfig; } }

		private string _EmailAddrConfig;
		public string EmailAddrConfig { get { return _EmailAddrConfig; } }

		private string _EmailTitleConfig;
		public string EmailTitleConfig { get { return _EmailTitleConfig; } }

		private List<string> FolderNames;
		private List<string> FolderFilePattern;
		private List<double> FolderFileAge;

		private bool LoadConfigValues()
		{
			try
			{
				FolderNames.Clear();
				FolderFilePattern.Clear();
				FolderFileAge.Clear();

				string folderAppKey = "MonitorFolderName";
				_FolderConfig = ClsConfig.ReadStringValue(folderAppKey, @"C:\, C:\Temp\Archive, ..\..\");

				string patternAppKey = "MonitorFilePattern";
				_MatchConfig = ClsConfig.ReadStringValue(patternAppKey, null);

				string ageAppKey = "MonitorFileAge";
				_AgeConfig = ClsConfig.ReadStringValue(ageAppKey, null);

				string addrAppKey = "MonitorEmailAddr";
				string addrVal = ClsConfig.ReadStringValue(addrAppKey, "jroman@amslgroup.com");
				_EmailAddrConfig = (addrVal != null) ? addrVal.Trim() : null;

				string titleAppKey = "MonitorEmailTitle";
				string titleVal = ClsConfig.ReadStringValue(titleAppKey, "Folder Monitor");
				_EmailTitleConfig = (titleVal != null) ? titleVal.Trim() : null;

				LogInfo("Config Values\r\n{0} = '{1}'\r\n{2} = '{3}'\t{4} = '{5}'\r\n{6} = '{7}'\t{8} = '{9}'\r\n",
					folderAppKey, FolderConfig, patternAppKey, MatchConfig, ageAppKey, AgeConfig,
					addrAppKey, EmailAddrConfig, titleAppKey, EmailTitleConfig);

				string[] dirs =
					(FolderConfig != null) ? FolderConfig.Split(new char[] { ',' }) : null;
				if( dirs == null || dirs.Length <= 0 || string.IsNullOrEmpty(EmailAddrConfig) )
				{
					if( dirs == null || dirs.Length <= 0 )
						LogError(null, "Missing or invalid value specified for {0} in app.config",
							folderAppKey);
					if( string.IsNullOrEmpty(EmailAddrConfig) )
						LogError(null, "Missing monitor email address, check {0} in app.config",
							addrAppKey);
					return false;
				}

				foreach( string d in dirs )
				{
					string dir = (d != null) ? d.Trim() : null;
					if( string.IsNullOrEmpty(dir) ) continue;

					string fullPath = Path.GetFullPath(dir);
					if( FolderNames.Exists(delegate(string dp)
					{ return string.Compare(dp, fullPath, true) == 0; }) ) continue;

					FolderNames.Add(fullPath);
				}

				if( FolderNames.Count <= 0 )
					return LogError(null, "No folders found to monitor, check {0} in app.config",
						folderAppKey);

				string[] matches =
					(MatchConfig != null) ? MatchConfig.Split(new char[] { ',' }) : null;
				if( matches != null && matches.Length > 0 )
				{
					foreach( string m in matches )
					{
						string match = (m != null) ? m.Trim() : null;
						if( !string.IsNullOrEmpty(match) ) FolderFilePattern.Add(match);
					}
				}

				if( FolderFilePattern.Count <= 0 )
					LogInfo(null, "No file patterns specified, check {0} in app.config",
						patternAppKey);

				string[] ages = (AgeConfig != null) ? AgeConfig.Split(new char[] { ',' }) : null;
				if( ages != null && ages.Length > 0 )
				{
					foreach( string a in ages )
					{
						string sAge = (a != null) ? a.Trim() : null;
						double dAge = 5;
						if( !string.IsNullOrEmpty(sAge) && ClsConvert.IsNumeric(sAge) )
							dAge = ClsConvert.ToDouble(sAge);
						FolderFileAge.Add(dAge);
					}
				}

				if( FolderFileAge.Count <= 0 )
					LogInfo(null, "No file ages specified, check {0} in app.config",
						ageAppKey);

				return true;
			}
			catch( Exception ex )
			{
				return LogError(ex, "Exception in LoadConfigValues");
			}
		}
		#endregion		// #region Configuration

		#region Monitor Folders

		public void LoadFiles(bool readFile)
		{
			try
			{
				ResetErrors();
				ResetInfo();

				_Folders.Clear();
				_AllFiles.Clear();
				sbEmailBody.Length = 0;

				if( !LoadConfigValues() ) return;

				_Compare_Dt = DateTime.Now;
				for( int i = 0; i < FolderNames.Count; i++ )
				{
					string dir = FolderNames[i];
					string match = (i < FolderFilePattern.Count) ? FolderFilePattern[i] : "*.txt";
					double age = (i < FolderFileAge.Count) ? FolderFileAge[i] : 5;
					FolderItem fdi = new FolderItem(dir, match, age);
					_Folders.Add(fdi);

					LogInfo("Start check for files in '{0}' matching '{1}' older than {2} minutes as of {3}",
						dir, match, age, Compare_Dt.ToString("dd-MMM-yyyy HH:mm"));

					fdi.LoadAgedFiles(Compare_Dt, readFile);
					if( fdi.HasErrors ) LogError(null, fdi.sbError.ToString());
					if( fdi.sbInfo.Length > 0 ) LogInfo(fdi.sbInfo.ToString());

					_AllFiles.AddRange(fdi.AgedFiles);
				}

				_Folders.Sort(delegate(FolderItem left, FolderItem right)
				{
					return string.Compare(left.Folder_Nm, right.Folder_Nm, true);
				});

				GenerateEmail();
			}
			catch( Exception ex )
			{
				LogError(ex, "LoadFiles exception");
			}
		}

		private void GenerateEmail()
		{
			try
			{
				_EmailSubject = EmailTitleConfig;
				sbEmailBody.Length = 0;

				int count = 0;
				foreach( FolderItem fdi in _Folders )
				{
					sbEmailBody.AppendFormat("{0} file(s) in '{1}' over {2} minutes old\r\n\r\n",
						fdi.AgedFiles.Count, fdi.Folder_Nm, fdi.AgeMin);
					foreach( ExtraFileInfo efi in fdi.AgedFiles )
						sbEmailBody.AppendFormat("{0}\t\tCreated: {1:dd-MMM-yyyy HH:mm}\tLast modified: {2:dd-MMM-yyyy HH:mm}\r\n",
							efi.SystemInfo.Name, efi.SystemInfo.CreationTime, efi.SystemInfo.LastWriteTime);
					count += fdi.AgedFiles.Count;
					sbEmailBody.AppendLine();
				}

				_EmailSubject = string.Format("{0} (Files pending: {1})", EmailTitleConfig, count);
			}
			catch( Exception ex )
			{
				LogError(ex, "Exception in GenerateEmail");
			}
		}
		#endregion		// #region Monitor Folders
	}

	public class FolderItem
	{
		#region Fields/Properties

		private double _AgeMin;
		public double AgeMin { get { return _AgeMin; } }

		private string _Folder_Nm;
		public string Folder_Nm { get { return _Folder_Nm; } }

		private string _MatchingPattern;
		public string MatchingPattern { get { return _MatchingPattern; } }

		private List<ExtraFileInfo> _AgedFiles;
		public List<ExtraFileInfo> AgedFiles { get { return _AgedFiles; } }

		#endregion		// #region Fields/Properties

		#region Constructors

		public FolderItem(string dirNm, string pattern, double minutes)
		{
			_Folder_Nm = dirNm;
			_AgeMin = minutes;
			_MatchingPattern = pattern;
			_AgedFiles = new List<ExtraFileInfo>();

			sbError = new StringBuilder();
			sbInfo = new StringBuilder();
		}
		#endregion		// #region Constructors

		#region Error/Message Handling

		public StringBuilder sbInfo;
		public StringBuilder sbError;

		public void ResetInfo() { sbInfo.Length = 0; }
		public void ResetErrors() { sbError.Length = 0; }
		public bool HasInfo { get { return sbInfo != null && sbInfo.Length > 0; } }
		public bool HasErrors { get { return sbError != null && sbError.Length > 0; } }
		public string AllMsgs
		{
			get
			{
				string err = sbError.ToString().Trim();
				string info = sbInfo.ToString().Trim();
				bool hasBoth = !string.IsNullOrEmpty(err) && !string.IsNullOrEmpty(info);
				return string.Format("{0}{1}{2}", err, (hasBoth ? "\r\n\r\n" : null), info);
			}
		}

		private void LogInfo(string msg, params object[] args)
		{
			try
			{
				string s = string.Format(msg, args);
				sbInfo.AppendLine(s);
			}
			catch( Exception ex )
			{
				Console.WriteLine("Exception writing info msg: {0}\r\n{1}",
					msg, ex.Message);
			}
		}

		private bool LogError(Exception ex, string msg, params object[] args)
		{
			try
			{
				string s = (msg != null) ? string.Format(msg, args) : string.Empty;
				if( ex != null ) sbError.AppendLine(ex.Message);
				if( !string.IsNullOrEmpty(s) ) sbError.AppendLine(s);
			}
			catch( Exception ex2 )
			{
				Console.WriteLine("Exception writing error msg: {0}\r\n{1}",
					msg, ex2.Message);
			}
			return false;
		}
		#endregion		// #region Error/Message Handling

		#region Scanning Folder

		public void LoadAgedFiles(DateTime dtCompare, bool readFile)
		{
			ResetErrors();
			ResetInfo();

			int count = 0;
			try
			{
				_AgedFiles.Clear();

				string[] allFiles = Directory.GetFiles(_Folder_Nm, MatchingPattern);
				if( allFiles == null || allFiles.Length <= 0 )
				{
					LogInfo("No files found matching {0} in {1}", MatchingPattern, _Folder_Nm);
					return;
				}

				foreach( string tmp in allFiles )
				{
					string file = (tmp != null) ? tmp.Trim() : null;
					if( string.IsNullOrEmpty(file) ) continue;

					FileInfo fi = new FileInfo(file);
					if( (fi.Attributes & FileAttributes.System) == FileAttributes.System ||
						(fi.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden ||
						(fi.Attributes & FileAttributes.Temporary) == FileAttributes.Temporary )
						continue;

					count++;
					ExtraFileInfo efi = new ExtraFileInfo(fi, null, dtCompare, AgeMin);
					if( efi.AgeCreated.TotalMinutes < _AgeMin ) continue;

					_AgedFiles.Add(efi);

					if( readFile )
					{
						using( StreamReader sr = new StreamReader(fi.OpenRead()) )
						{
							efi.FileContents = sr.ReadToEnd();
						}
					}
				}

				_AgedFiles.Sort(delegate(ExtraFileInfo left, ExtraFileInfo right)
				{
					if( left.SystemInfo.CreationTime < right.SystemInfo.CreationTime ) return -1;
					if( left.SystemInfo.CreationTime > right.SystemInfo.CreationTime ) return 1;
					if( left.SystemInfo.LastWriteTime < right.SystemInfo.LastWriteTime ) return -1;
					if( left.SystemInfo.LastWriteTime > right.SystemInfo.LastWriteTime ) return 1;
					return string.Compare(left.SystemInfo.Name, right.SystemInfo.Name, true);
				});
			}
			catch( Exception ex )
			{
				LogError(ex, "LoadAgedFiles Exception");
			}
			finally
			{
				LogInfo("{0} total file(s) in '{1}', {2} file(s) older than {3} minutes found",
					count, _Folder_Nm, _AgedFiles.Count, _AgeMin);
			}
		}
		#endregion		// #region Scanning Folder
	}

	public class ExtraFileInfo
	{
		#region Fields/Properties

		private FileInfo _SystemInfo;
		public FileInfo SystemInfo
		{
			get { return _SystemInfo; }
			set { _SystemInfo = value; }
		}

		private string _FileContents;
		public string FileContents
		{
			get { return _FileContents; }
			set { _FileContents = value; }
		}

		private DateTime? _Compare_Dt;
		public DateTime? Compare_Dt
		{
			get { return _Compare_Dt; }
			set { _Compare_Dt = value; }
		}

		private double _AgeMin;
		public double AgeMin { get { return _AgeMin; } }
	
		public TimeSpan AgeCreated
		{
			get
			{
				if( SystemInfo == null || Compare_Dt == null ) return TimeSpan.Zero;

				TimeSpan ts = Compare_Dt.Value - SystemInfo.CreationTime;
				return ts;
			}
		}

		public TimeSpan AgeModified
		{
			get
			{
				if( SystemInfo == null || Compare_Dt == null ) return TimeSpan.Zero;

				TimeSpan ts = Compare_Dt.Value - SystemInfo.LastWriteTime;
				return ts;
			}
		}
		#endregion		// #region Fields/Properties

		#region Constructors

		public ExtraFileInfo(FileInfo fi, string fileData, DateTime? compDt, double minimumAge)
		{
			_SystemInfo = fi;
			_FileContents = fileData;
			_Compare_Dt = compDt;
			_AgeMin = minimumAge;
		}
		#endregion		// #region Constructors
	}
}