using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace CS2010.Common
{
	/// <summary>FtpClient class performs the actions of a simple ftp client</summary>
	public class FtpClient
	{
		#region Enums

		// used for GetDir() flags
		public enum DirMode { Complete, NamesOnly };

		#endregion		// #region Enums

		#region Fields

		private bool IsConnected;			// indicate connection to host
		private bool IsLoggedIn;			// indicate user is logged on
		private bool IsVerbose;				// send feedback
		private int Port_Number;			// default FTP port
		private TcpClient theTcpClient;		// command channel
		private StreamReader theCmdReader;	// text reader
		private StreamWriter theCmdWriter;	// text writer

		/// <summary>Delegate to supply all commands sent and replies to user</summary>
		public delegate void CmdEventHandler(string cmd);

		/// <summary>receive all commands</summary>
		public event CmdEventHandler cmdEvent;

		#endregion		// #region Fields

		#region Constructors/Initialization/Cleanup

		/// <summary>Constructor: Default</summary>
		public FtpClient()
		{
			IsConnected = false;
			IsLoggedIn = false;
			IsVerbose = true;
			Port_Number = 21;
			theTcpClient = null;
			theCmdReader = null;
			theCmdWriter = null;
		}

		/// <summary>Closes TcpClient and cleans up</summary>
		public void Cleanup()
		{
			if( !IsConnected ) return;

			theCmdReader.Close();		// close text reader
			theCmdReader.Dispose();		// dispose
			theCmdReader = null;

			theCmdWriter.Close();		// close text writer
			theCmdWriter.Dispose();		// dispose
			theCmdWriter = null;

			theTcpClient.Close();	// close command channel
			theTcpClient = null;

			IsConnected = false;
			IsLoggedIn = false;
		}
		#endregion		// #region Constructors/Initialization/Cleanup

		#region Connect/Disconnect

		/// <summary>Connect to the remote Host FTP server</summary>
		/// <param name="host">Host name of FTP server</param>
		public void Connect(string host)
		{
			if( IsConnected ) throw new ApplicationException("FTP Connection already open");

			IsVerbose = true;	// send all commands and replies to CmdEvent handlers

			// create a TcpClient control for Transport connection
			theTcpClient = new TcpClient(host, Port_Number); // use default port 21
			theTcpClient.ReceiveTimeout = 10000; // wait 10 seconds before aborting
			theTcpClient.SendTimeout = 10000; // wait 10 seconds before aborting

			theCmdReader = new StreamReader(theTcpClient.GetStream());  // text reader
			theCmdWriter = new StreamWriter(theTcpClient.GetStream()); // text writer

			IsConnected = true;

			string reply = ReadReply();	// 220 reply(multiline) if successful
			if( reply[0] != '2' ) throw new ApplicationException(reply);
		}

		/// <summary>Log in user to a connected remote FTP server</summary>
		/// <param name="userName">User name</param>
		/// <param name="pwd">User Password</param>
		public void Login(string userName, string pwd)
		{
			if( !IsConnected ) throw new ApplicationException("Not Connected to Host");

			if( userName == "" ) userName = "anonymous";
			if( pwd == "" ) pwd = "anonymous";

			string reply = SendCommand("USER " + userName);
			// the server must reply with 331
			if( reply[0] != '3' ) throw new ApplicationException(reply);

			reply = SendCommand("PASS " + pwd);
			// the server must reply with 230, which is a successful login
			if( reply[0] != '2' ) throw new ApplicationException(reply);

			IsLoggedIn = true;
		}

		/// <summary>disconnect remote host</summary>
		public void Disconnect()
		{
			if( IsConnected ) SendCommand("QUIT");
			Cleanup();
		}
		#endregion		// #region Connect/Disconnect

		#region General FTP Operations

		/// <summary>Sets the current remote directory</summary>
		/// <param name="dir">Directory name</param>
		public bool SetCurrentDirectory(string dir)
		{
			if( !IsLoggedIn ) return false;

			string reply = SendCommand("CWD " + dir);
			// server must reply with 250, else the directory does not exist
			if( reply[0] != '2' ) throw new ApplicationException(reply);

			return true;
		}

		/// <summary>Sends a command to remote host and waits for reply</summary>
		/// <param name="sCmd">command to server</param>
		public string SendCommand(string cmd)
		{
			if( !IsConnected ) return "000";
			WriteLog(cmd);
			try
			{
				theCmdWriter.WriteLine(cmd);
				theCmdWriter.Flush();	// send the data
			}
			catch( Exception ex )
			{
				WriteLog("Write timeout Error:" + ex.Message);
				Cleanup(); // disconnect and cleanup
				throw new ApplicationException
					("Write Failed: Closing connection", ex);
			}
			return ReadReply();	// wait for reply from Host
		}

		/// <summary>Get List of host files and directories from UNIX server and
		/// Send filenames to "dirEvent" subscribers </summary>
		/// <param name="dir">Directory path. "" = CWD</param>
		/// <param name="dirFlg">Mode</param>
		public string[] GetDir(string host, string user, string pwd, string dir,
			DirMode dirFlg)
		{
			string[] result = null;
			try
			{
				Connect(host);
				Login(user, pwd);
				SendCommand("CWD " + dir);
				result = GetDir("", dirFlg);
			}
			finally
			{
				Disconnect();
			}
			return result;
		}

		/// <summary>Get List of host files and directories from UNIX server
		/// and send filenames to "dirEvent" subscribers</summary>
		/// <param name="dir">Directory path. "" = CWD</param>
		/// <param name="dirFlg">Mode</param>
		public string[] GetDir(string dir, DirMode dirFlg)
		{
			if( !IsConnected ) throw new ApplicationException("Not connected to Host");
			if( !IsLoggedIn ) throw new ApplicationException("User not logged in");

			WriteLog("Reading Directory: " + dir);

			IsVerbose = false;	// disable feedback

			Socket dSocket = CreateDataSocket();
			string cmd = "LIST " + dir;
			if( dirFlg == DirMode.NamesOnly ) cmd = "NLST " + dir;
			string reply = SendCommand(cmd.Trim());
			if( reply[0] != '1' ) throw new ApplicationException(reply);

			byte[] bytes = new byte[4096];	// buffer to receive data bytes
			int nBytes = 0; // number of bytes read
			string s = "";	// string to hold all converted ASCII data
			while( ( nBytes = dSocket.Receive(bytes, bytes.Length, 0) ) > 0 )
				s += Encoding.ASCII.GetString(bytes, 0, nBytes); // convert to ASCII
			dSocket.Close();	// close data connection
			reply = ReadReply();	// 226- Transfer Complete

			IsVerbose = true;	// re-enable feedback

			if( reply[0] != '2' ) throw new ApplicationException(reply);

			return s.Replace("\r", null).Split('\n'); // convert to string array
		}
		#endregion		// #region General FTP Operations

		#region Transfer Operations

		/// <summary>Download a file to a directory</summary>
		/// <param name="host">Host name of FTP server</param>
		/// <param name="user">User name</param>
		/// <param name="pwd">User Password</param>
		/// <param name="remoteFolder">Directory to download from on host</param>
		/// <param name="remFilename">Filename on host</param>
		/// <param name="locDir">Directory on local computer "" = current dir</param>
		/// <param name="locFile">Filename on local computer, "" = same name as remote file</param>
		public bool Download(string host, string user, string pwd,
			string remoteFolder, string remFilename, string locDir, string locFile)
		{
			bool ret = false;
			try
			{
				Connect(host);
				Login(user, pwd);
				ret = SetCurrentDirectory(remoteFolder);
				if( ret == true ) ret = Download(remFilename, locDir, locFile);
			}
			finally
			{
				Disconnect();
			}
			return ret;
		}

		/// <summary>Download a file to a directory</summary>
		/// <param name="remFilename">Filename on host</param>
		/// <param name="locDir">Directory on local computer "" = current dir</param>
		/// <param name="locFile">Filename on local computer, "" = same name as remote file</param>
		public bool Download(string remFilename, string locDir, string locFile)
		{
			if( !IsLoggedIn ) throw new ApplicationException("User not logged in");
			if( remFilename == "" ) throw new ApplicationException("Remote Filename Empty");

			if( locDir != "" )
				if( locDir[locDir.Length - 1] != '\\' && locDir[locDir.Length - 1] != '/' )
					locDir += "\\"; // ensure there is a seperator character
			if( string.IsNullOrEmpty(locFile ) ) locFile = Path.GetFileName(remFilename);
			string locFilename = locDir + locFile;

			string reply = null;
			Socket dSocket = null;
			try
			{
				dSocket = CreateDataSocket();	// set for data transfer
				reply = SendCommand("RETR " + remFilename); // request a file
				if( reply.Substring(0, 1) != "1" ) throw new ApplicationException(reply);

				byte[] bytes = new byte[1024];	// read buffer
				int nBytes = 0;
				using( FileStream fs = File.Create(locFilename) ) // creates new file
				{
					while( ( nBytes = dSocket.Receive(bytes, bytes.Length, 0) ) > 0 )
						fs.Write(bytes, 0, nBytes);
					fs.Close();
				}
			}
			finally
			{
				if( dSocket != null ) dSocket.Close();	// close data connection
			}

			reply = ReadReply();	// wait for result code 226 Transfer complete
			if( reply[0] != '2' ) throw new ApplicationException(reply);

			return true;
		}

		/// <summary>Upload a file to a server directory
		/// Note: Opens and closes a connection </summary>
		/// <param name="host">Host name of FTP server</param>
		/// <param name="user">User name</param>
		/// <param name="pwd">User Password</param>
		/// <param name="locFilename">Filename on local computer</param>
		/// <param name="remDir">Directory to upload to on host. "" = CWD</param>
		/// <param name="remFile">The name of the remote file. "" uses name of local file</param>
		public bool Upload(string host, string user, string pwd,
			string locFilename, string remDir, string remFile)
		{
			bool ret = false;
			try
			{
				Connect(host);
				Login(user, pwd);
				ret = Upload(locFilename, remDir, remFile);
			}
			finally
			{
				Disconnect();
			}
			return ret;
		}

		/// <summary>Upload a file to a server directory (using same name)
		/// Note: Opens and closes a connection </summary>
		/// <param name="host">Host name of FTP server</param>
		/// <param name="user">User name</param>
		/// <param name="pwd">User Password</param>
		/// <param name="fsLocalFile">Filestream created for local file</param>
		/// <param name="remDir">Directory to upload to on host. "" = CWD</param>
		/// <param name="remFile">The name of the remote file. "" uses name of local file</param>
		public bool Upload(string host, string user, string pwd,
			FileStream fsLocalFile, string remDir, string remFile)
		{
			bool ret = false;
			try
			{
				Connect(host);
				Login(user, pwd);
				ret = Upload(fsLocalFile, remDir, remFile);
			}
			finally
			{
				Disconnect();
			}
			return ret;
		}

		/// <summary>Upload an entire folder to the server's current directory</summary>
		/// <param name="locDir">Folder on local computer</param>
		/// <param name="locArkDir">Folder to copy all files to after successful ftp</param>
		public bool Upload(string locDir, string locArkDir)
		{
			StringBuilder sMsg = new StringBuilder();
			bool bReturn = true;
			string[] sFiles = System.IO.Directory.GetFiles(locDir);
			foreach (string sF in sFiles)
			{
				if (!Upload(sF, "", ""))
					bReturn = false;
				string sFileName = Path.GetFileName(sF);
				try
				{
					System.IO.File.Move(sF, locArkDir + sFileName);
				}
				catch (Exception ex)
				{
					// May2015: We've been having a problem where a file is successfully ftp'd, but when it
					// is sent to the Archive folder it remains in the Out folder.  So the next time through
					// the Move command throws an exception and the others files do not get processed.
					//
					// Now, if we receive an "already exists" message we delete the file from the OUT folder 
					// and continue.  Also, if the move fails for any reason whatsoever we consume the error
					// and keep on processing, so other files are not "clogged-up" in the queue.
					string s = ex.Message;
					if (s.ToLower().Contains("already exists"))
					{
						System.IO.File.Delete(sF);
						continue;
					}
					ClsErrorHandler.LogException(ex);
				}
			}
			return bReturn;
		}
		/// <summary>Upload a file to a server directory</summary>
		/// <param name="locFilename">Filename on local computer</param>
		/// <param name="remDir">Directory to upload to on host. "" = CWD</param>
		/// <param name="remFile">The name of the remote file. "" uses name of local file</param>
		public bool Upload(string locFilename, string remDir, string remFile)
		{
			using( FileStream fs = File.Open(locFilename, FileMode.Open, FileAccess.Read,
				FileShare.None) )	// open local file for reading
			{
				return Upload(fs, remDir, remFile);
			}
		}

		/// <summary>Upload a file to a server directory</summary>
		/// <param name="fsLocalFile">Filestream created for local file</param>
		/// <param name="remDir">Directory to upload to on host. "" = CWD</param>
		/// <param name="remFile">The name of the remote file. "" uses name of local file</param>
		public bool Upload(FileStream fsLocalFile, string remDir, string remFile)
		{
			if( !IsLoggedIn ) throw new ApplicationException("User not logged in");
			if( fsLocalFile == null ) throw new ApplicationException("Local file not opened");

			if( remDir != "" )
				if( remDir[remDir.Length - 1] != '/' )
					remDir += "/";  // ensure there is a seperator
			if( string.IsNullOrEmpty(remFile) )
				remFile = Path.GetFileName(fsLocalFile.Name); // use original filename
			string remFilename = remDir + remFile;

			string reply = null;
			Socket dSocket = null;
			try
			{
				dSocket = CreateDataSocket(); // create data connection with host
				reply = SendCommand("STOR " + remFilename);
				if( reply[0] != '1' ) throw new ApplicationException(reply);
				byte[] bytes = new byte[1024];
				int nBytes = 0;
				while( ( nBytes = fsLocalFile.Read(bytes, 0, bytes.Length) ) > 0 ) // read data
					dSocket.Send(bytes, nBytes, 0);	// send to host
			}
			finally
			{
				if( dSocket != null ) dSocket.Close(); // close data connection
			}

			reply = ReadReply();	// wait for message 226 Transfer Complete
			if( reply[0] != '2' ) throw new ApplicationException(reply);

			return true;
		}

		#endregion		// #region Public Methods

		#region Helper Methods

		// Read entire (multi-line) replies from server
		private string ReadReply()
		{
			string s = "";
			try
			{
				s = theCmdReader.ReadLine();		// get first line of reply
				string end = s.Substring(0, 3) + " ";	// save reply number plus space
				while( s.Substring(0, 4) != end )
				{
					WriteLog(s);				// log line
					s = theCmdReader.ReadLine();	// read multi-line replies
				}
				WriteLog(s);					// log last line
			}
			catch( Exception ex )
			{
				WriteLog("Timeout Error:" + ex.Message);
				Cleanup(); // disconnect? and cleanup

				throw new ApplicationException("Read Error: Closing connection", ex);
			}
			if( s.Length < 4 ) throw new ApplicationException("Invalid Reply From Server");
			if( s[0] == '2' ) WriteLog(""); // add blank line - end of sequence

			return s;	// return last line read
		}

		// create socket for data transfer, returns null on error
		private Socket CreateDataSocket()
		{
			string reply = SendCommand("PASV"); // request a data connection
			// returns: "227 Entering Passive Mode (204,127,12,38,13,193)."
			if( reply[0] != '2' ) throw new ApplicationException(reply);

			// extract IP Address and Port number
			int n1 = reply.IndexOf("(");
			int n2 = reply.IndexOf(")");
			string[] sa = reply.Substring(n1 + 1, n2 - n1 - 1).Split(',');
			string ipAddr = sa[0] + "." + sa[1] + "." + sa[2] + "." + sa[3];
			int nPort = int.Parse(sa[4]) * 256 + int.Parse(sa[5]);

			Socket socket = null;	// data transfer socket
			try
			{	// connect to host data channel
				socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
				socket.Connect(new IPEndPoint(IPAddress.Parse(ipAddr), nPort));
			}
			catch( Exception ex )
			{
				if( socket != null ) socket.Close();
				WriteLog(ex.Message);
				throw new ApplicationException("Error creating data connection", ex);
			}
			return socket;
		}

		// supply commands and replies to "cmdEvent" subscribers
		private void WriteLog(string logMsg)
		{
			if( cmdEvent != null && IsVerbose ) cmdEvent(logMsg);
		}
		#endregion		// #region Helper Methods
	}
}