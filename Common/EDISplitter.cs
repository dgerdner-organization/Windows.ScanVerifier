using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace CS2010.Common
{
	public class EDISplitter
	{
		#region Fields/Properties

		/// <summary>The original EDI text file name (including path)</summary>
		private string MainFile;

		private Dictionary<string, string> SplitFileContents;
		/// <summary>Total number of files that should be created by CreateNewFiles()</summary>
		public int TotalFiles { get { return SplitFileContents.Count; } }

		private int _FilesCreated;
		/// <summary>Total number of files that were actually created. If this is less than
		/// the TotalFiles property then there was an error.</summary>
		public int FilesCreated { get { return _FilesCreated; } }

		private string _AllText;
		/// <summary>The entire contents of the EDI text file</summary>
		public string AllText { get { return _AllText; } }

		private string _Header;
		/// <summary>The "header" section of the EDI text file. It includes all text up to the
		/// first ~ST* delimiter in the file.</summary>
		public string Header { get { return _Header; } }

		private string _RawFooter;
		/// <summary>The "footer" section of the EDI text file. It includes all text following
		/// and including the ~GE* delimiter in the file.</summary>
		public string RawFooter { get { return _RawFooter; } }

		private string _Footer;
		/// <summary>A modified version of RawFooter where the first number after the ~GE* is
		/// hardcoded to 1 since the new file will only contain 1 ~ST* segment.</summary>
		public string Footer { get { return _Footer; } }

		private string _Detail;
		/// <summary>The section from the original EDI text file in between the Header and Footer.
		/// This text includes all of the ~ST* segments.</summary>
		public string Detail { get { return _Detail; } }

		#endregion		// #region Fields/Properties

		#region Constructors

		public EDISplitter()
		{
			sbError = new StringBuilder();
			sbInfo = new StringBuilder();
			SplitFileContents = new Dictionary<string, string>();
		}
		#endregion		// #region Constructors

		#region Error/Message Handling

		public StringBuilder sbInfo;
		public StringBuilder sbError;

		public void ResetInfo() { sbInfo.Length = 0; }
		public void ResetErrors() { sbError.Length = 0; }
		public bool HasErrors { get { return sbError != null && sbError.Length > 0; } }

		private void LogInfo(string msg, params object[] args)
		{
			try
			{
				string s = string.Format(msg, args);
				sbInfo.AppendLine(s);
				ClsErrorHandler.LogMessage(s);
			}
			catch( Exception ex )
			{
				Console.WriteLine("Error writing info msg to event log: {0}\r\n{1}",
					msg, ex.Message);
			}
		}

		private bool LogError(Exception ex, string msg, params object[] args)
		{
			try
			{
				string s = (msg != null) ? string.Format(msg, args) : string.Empty;
				sbError.AppendLine(s);
				if( ex == null )
					ClsErrorHandler.LogError(s);
				else
					ClsErrorHandler.LogException(ex, null, s);
			}
			catch( Exception ex2 )
			{
				Console.WriteLine("Error writing info msg to event log: {0}\r\n{1}",
					msg, ex2.Message);
			}
			return false;
		}
		#endregion		// #region Error/Message Handling

		#region Parsing/Splitting

		/// <summary>Gets a string list of all the new filenames that will be created when
		/// CreateNewFiles is called</summary>
		public List<string> GetFileList()
		{
			return new List<string>(SplitFileContents.Keys);
		}

		/// <summary>Gets the proposed contents of the given output filename. Can be used
		/// in conjuction with GetFileList to loop over all files when necessary.</summary>
		public string GetFileText(string fileKey)
		{
			return SplitFileContents[fileKey];
		}

		/// <summary>This is the first method that should be called for a given EDI text file.
		/// The CreateNewFiles method will not work unless this method is called and has completed
		/// successfully.</summary>
		/// <param name="fileName">Filename including path of the original EDI text file</param>
		/// <param name="moveIfError">If true an attempt is made to move the file to the error
		/// folder if the parsing failed. If false, the file will not be moved. This is to allow
		/// different behavior between the windows front-end and the console app. When running
		/// from windows, we may not want the file moved if the error occurred, but for the console
		/// app, the file should be moved immediately when an error occurs.</param>
		public bool ParseOriginalFile(string fileName, bool moveIfError)
		{
			try
			{
				StringBuilder sbMove = new StringBuilder();
				bool ret = ParseMainFile(fileName);
				if( ret == false && moveIfError == true )
				{	// If parse failed, and we want the file moved when there is an error,
					// then attempt the move, but only if the file exists
					if( File.Exists(MainFile) ) MoveOriginalFile(false, sbMove);
				}

				if( sbMove.Length > 0 ) LogInfo(sbMove.ToString());

				return ret;
			}
			catch( Exception ex )
			{
				LogError(ex, "ParseOriginalFile Method Exception");
				return false;
			}
		}

		private bool ParseMainFile(string fileName)
		{
			try
			{
				MainFile = fileName;
				SplitFileContents.Clear();
				sbInfo.Length = 0;
				sbError.Length = 0;
				_FilesCreated = 0;

				if( !File.Exists(MainFile) )
					return LogError(null, "File {0} not found", MainFile);

				using( StreamReader sr = new StreamReader(File.OpenRead(MainFile)) )
				{
					_AllText = sr.ReadToEnd();
					sr.Close();
				}

				if( !ValidateFile() ) return false;

				if( !ParseDetail() ) return false;

				return true;
			}
			catch( Exception ex )
			{
				LogError(ex, "ParseMainFile Method Exception");
				return false;
			}
		}

		private bool ValidateFile()
		{
			if( string.IsNullOrEmpty(_AllText) )
				return LogError(null, "File is empty: {0}", MainFile);

			// File should at least have ISA*~GS*~ST*~GE*~IEA*, so it needs to be at least 21 characters
			if( _AllText.Length < 21 )
				return LogError(null, "Invalid file length: {0}", MainFile);

			// First 4 characters should be ISA*
			string isa = _AllText.Substring(0, 4);
			if( string.Compare(isa, "ISA*") != 0 )
				return LogError(null, "File does not start with the ISA delimiter: {0}", MainFile);

			int isaIndex2 = _AllText.IndexOf("~ISA*", 4, StringComparison.InvariantCultureIgnoreCase);
			if( isaIndex2 >= 0 )
				return LogError(null, "File contains multiple ISA delimiters: {0}", MainFile);

			int gsIndex = _AllText.IndexOf("~GS*", StringComparison.InvariantCultureIgnoreCase);
			if( gsIndex < 0 )
				return LogError(null, "File does not contain the ~GS* delimiter: {0}", MainFile);

			int gsIndex2 = _AllText.IndexOf("~GS*", gsIndex + 4, StringComparison.InvariantCultureIgnoreCase);
			if( gsIndex2 >= 0 )
				return LogError(null, "File contains multiple ~GS* delimiters: {0}", MainFile);

			int stIndexFirst = _AllText.IndexOf("~ST*", StringComparison.InvariantCultureIgnoreCase);
			if( stIndexFirst < 0 )
				return LogError(null, "File does not contain the ~ST* delimiter: {0}", MainFile);

			if( stIndexFirst <= gsIndex )
				return LogError(null, "~GS* does not appear before the ~ST* delimiter: {0}", MainFile);

			int geIndex = _AllText.IndexOf("~GE*", StringComparison.InvariantCultureIgnoreCase);
			if( geIndex < 0 )
				return LogError(null, "File does not contain the ~GE* delimiter: {0}", MainFile);

			int geIndex2 = _AllText.IndexOf("~GE*", geIndex + 4, StringComparison.InvariantCultureIgnoreCase);
			if( geIndex2 >= 0 )
				return LogError(null, "File contains multiple ~GE* delimiters: {0}", MainFile);

			int ieaIndex = _AllText.IndexOf("~IEA*", StringComparison.InvariantCultureIgnoreCase);
			if( ieaIndex < 0 )
				return LogError(null, "File does not contain the ~IEA* delimiter: {0}", MainFile);

			if( ieaIndex <= geIndex )
				return LogError(null, "~GE* does not appear before the ~IEA* delimiter: {0}", MainFile);

			int ieaIndex2 = _AllText.IndexOf("~IEA*", ieaIndex + 5, StringComparison.InvariantCultureIgnoreCase);
			if( ieaIndex2 >= 0 )
				return LogError(null, "File contains multiple ~IEA* delimiters: {0}", MainFile);

			int stIndexLast = _AllText.LastIndexOf("~ST*", StringComparison.InvariantCultureIgnoreCase);
			if( stIndexLast > geIndex )
				return LogError(null, "~GE* does not appear after all ~ST* delimiters: {0}", MainFile);

			_Header = _AllText.Substring(0, stIndexFirst);
			_RawFooter = _AllText.Substring(geIndex);

			StringBuilder sbFooter = new StringBuilder("~GE*1");
			// Find the next asterisk before the IEA section
			int astIndex = _AllText.IndexOf('*', geIndex + 4, ieaIndex - geIndex - 4);
			if( astIndex >= 0 )	// If found append from there
				sbFooter.Append(_AllText.Substring(astIndex));
			else				// else, append from the IEA section
				sbFooter.Append(_AllText.Substring(ieaIndex));

			_Footer = sbFooter.ToString();

			_Detail = _AllText.Substring(stIndexFirst, geIndex - stIndexFirst);

			return true;
		}

		private bool ParseDetail()
		{
			try
			{
				List<string> tmp = new List<string>();
				string[] sections = _Detail.Split(new string[] { "~ST*" }, StringSplitOptions.None);
				if( sections != null && sections.Length > 0 )
				{
					foreach( string stSection in sections )
					{
						if( string.IsNullOrEmpty(stSection) ) continue;

						tmp.Add("~ST*" + stSection);
					}
				}
				if( tmp.Count <= 0 )
					return LogError(null, "Error Parsing Detail: {0}", MainFile);

				string fileNoExt = Path.GetFileNameWithoutExtension(MainFile);
				string prefix = string.Format("{0}-{1}-{2}", fileNoExt,
					DateTime.Now.ToString("MMddHHmms"), tmp.Count);
				string outPath = ClsConfig.ReadStringValue("SplitterOutputPath", ".");
				for (int i = 0; i < tmp.Count; i++)
				{
					string data = string.Format("{0}{1}{2}", Header, tmp[i], Footer);
					string name = string.Format("{0}-{1:0000}.txt", prefix, i + 1);
					string outFile = Path.Combine(outPath, name);
					SplitFileContents.Add(outFile, data);
				}

				return true;
			}
			catch( Exception ex )
			{
				return LogError(ex, "Parse Detail Exception");
			}
		}
		#endregion		// #region Parsing/Splitting

		#region Creating Files

		private bool ValidateWrite()
		{
			try
			{
				if( HasErrors )
					return LogError(null, "Errors were detected, cannot create files");
				if( TotalFiles <= 0 )
					return LogError(null, "There are no files to create");

				return true;
			}
			catch( Exception ex )
			{
				return LogError(ex, "ValidateWrite Exception");
			}
		}

		/// <summary>Creates the new (split) output files. Must call ParseOriginalFile before
		/// calling this method.</summary>
		public bool CreateNewFiles()
		{
			try
			{
				StringBuilder sbProcess = new StringBuilder();
				bool ret = WriteSplitFiles(sbProcess);
				MoveOriginalFile(ret, sbProcess);

				if( sbProcess.Length > 0 ) LogInfo(sbProcess.ToString());

				return ret;
			}
			catch( Exception ex )
			{
				return LogError(ex, "CreateFiles Exception");
			}
		}

		private bool WriteSplitFiles(StringBuilder sbCreated)
		{
			if( !ValidateWrite() ) return false;

			try
			{
				_FilesCreated = 0;
				int errors = 0;
				foreach( string fileName in SplitFileContents.Keys )
				{
					string data = SplitFileContents[fileName];

					try
					{
						using( StreamWriter sw = new StreamWriter(File.Create(fileName)) )
						{
							sw.Write(data);
							sw.Flush();
							sw.Close();
						}
						_FilesCreated++;
						sbCreated.AppendFormat("Created file {0}\r\n", fileName);
					}
					catch( Exception ex )
					{
						LogError(ex, "Error writing {0}", fileName);
						errors++;
					}
				}

				return errors == 0;
			}
			catch( Exception ex )
			{
				return LogError(ex, "WriteSplitFiles Exception");
			}
		}

		private bool MoveOriginalFile(bool success, StringBuilder sbMove)
		{
			string moveFile = null;
			try
			{
				string name = Path.GetFileName(MainFile);
				string movePath = (success)
					? ClsConfig.ReadStringValue("SplitterArchivePath", ".")
					: ClsConfig.ReadStringValue("SplitterErrorPath", ".");

				moveFile = Path.Combine(movePath, name);
				File.Move(MainFile, moveFile);

				sbMove.AppendFormat("File moved from {0} to {1}\r\n", MainFile, moveFile);

				return true;
			}
			catch( Exception ex )
			{
				return LogError(ex, "Original file may not have been moved from {0} to {1}",
					MainFile, moveFile);
			}
		}
		#endregion		// #region Creating Files
	}
}