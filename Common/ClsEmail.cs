using System;
using System.Text;
using System.Net.Mail;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;

namespace CS2010.Common
{
	public class ClsEmail : INotifyPropertyChanged
	{
		#region Constants

		/// <summary>We need to use this SMTP server instead of the host specified in the
		/// config file when we need to send mail to external email addresses
		/// (it has less restrictions on outbound emails).</summary>
		//public const string UnrestrictedSMTPServer = "x1x72.16.2.169";
		public const string UnrestrictedSMTPServer = "relay.appriver.com";
		 
		#endregion		// #region Constants

		#region Fields

		protected List<string> _Attachments;
		protected string _SMTPServer;
		protected string _Subject;
		protected string _Body;
		protected string _From;
		protected string _To;
		protected string _CC;
		protected string _BCC;

		#endregion		// #region Fields

		#region Properties

		/// <summary>Gets/Sets the IP Address of the EmailServer</summary>
		public string SMTPServer
		{
			get { return _SMTPServer; }
			set
			{
				if( string.Compare(value, _SMTPServer, false) == 0 ) return;

				_SMTPServer = value;
				NotifyPropertyChanged("SMTPServer");
			}
		}

		/// <summary>Gets/Sets the email's subject line</summary>
		public string Subject
		{
			get { return _Subject; }
			set
			{
				if( string.Compare(value, _Subject, false) == 0 ) return;

				_Subject = value;
				NotifyPropertyChanged("Subject");
			}
		}

		/// <summary>Gets/Sets the full text of the email message</summary>
		public string Body
		{
			get { return _Body; }
			set
			{
				if( string.Compare(value, _Body, false) == 0 ) return;

				_Body = value;
				NotifyPropertyChanged("Message");
			}
		}

		/// <summary>Gets/Sets the "From" address</summary>
		public string From
		{
			get { return _From; }
			set
			{
				if( string.Compare(value, _From, false) == 0 ) return;

				_From = value;
				NotifyPropertyChanged("From");
			}
		}

		/// <summary>Gets/Sets the "To" address or addresses</summary>
		public string To
		{
			get { return _To; }
			set
			{
				if( string.Compare(value, _To, false) == 0 ) return;

				_To = value;
				NotifyPropertyChanged("To");
			}
		}

		/// <summary>Gets/Sets the "CC" addresses</summary>
		public string CC
		{
			get { return _CC; }
			set
			{
				if( string.Compare(value, _CC, false) == 0 ) return;

				_CC = value;
				NotifyPropertyChanged("CC");
			}
		}

		/// <summary>Gets/Sets the "BCC" addresses</summary>
		public string BCC
		{
			get { return _BCC; }
			set
			{
				if( string.Compare(value, _BCC, false) == 0 ) return;

				_BCC = value;
				NotifyPropertyChanged("BCC");
			}
		}

		public List<string> Attachments
		{
			get { return _Attachments; }
		}

		public string AttachmentString
		{
			get
			{
				if( Attachments == null || Attachments.Count <= 0 )
					return string.Empty;

				StringBuilder sb = new StringBuilder();
				int count = 0;
				foreach( string s in Attachments )
				{
					if( string.IsNullOrEmpty(s) == true ) continue;

					sb.AppendFormat("{0}{1}", ( count > 0 ? ", " : null), s);
					count++;
					if( count > 4 )
					{
						count = 0;
						sb.AppendLine();
					}
				}

				return sb.ToString();
			}
		}

		public string MemoFormat
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendFormat("From: {0}\r\n", From);
				sb.AppendFormat("To: {0}\r\n", To);
				sb.AppendFormat("CC: {0}\r\n", CC);
				if( string.IsNullOrEmpty(BCC) == false )
					sb.AppendFormat("BCC: {0}\r\n", BCC);

				string s = AttachmentString;
				if( string.IsNullOrEmpty(s) == false )
					sb.AppendFormat("Attachments: {0}\r\n", s);

				sb.AppendFormat("Subject: {0}\r\n", Subject);
				sb.AppendFormat("{0}\r\n", Body);

				return sb.ToString();
			}
		}
		#endregion		// #region Properties

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void NotifyPropertyChanged(String info)
		{
			if( PropertyChanged != null )
				PropertyChanged(this, new PropertyChangedEventArgs(info));
		}
		#endregion

		#region Constructors

		/// <summary>Default constructor</summary>
		public ClsEmail()
		{
			_Attachments = new List<string>();
		}

		public ClsEmail(ClsEmail src)
		{
			CopyFrom(src);
		}

		/// <summary>Constructor expecting To, Subject and Body (SMTP server
		/// and From email address are obtained automatically from the .NET
		/// mailSettings section of the config file)</summary>
		/// <param name="sendTo">The addresses to send the email to</param>
		/// <param name="subject">The subject of the email</param>
		/// <param name="body">The email message body</param>
		public ClsEmail(string sendTo, string subject, string body)
		{
			_To = sendTo;
			_Subject = subject;
			_Body = body;

			_Attachments = new List<string>();
		}

		/// <summary>Constructor expecting from, to, subject and body</summary>
		/// <param name="sendFrom">The email address of the sender</param>
		/// <param name="sendTo">The addresses to send the email to</param>
		/// <param name="subject">The subject of the email</param>
		/// <param name="body">The email message body</param>
		public ClsEmail(string sendFrom, string sendTo, string subject,
			string body)
		{
			_From = sendFrom;
			_To = sendTo;
			_Subject = subject;
			_Body = body;

			_Attachments = new List<string>();
		}

		/// <summary>Constructor expecting To, CC, BCC, Subject, Body
		/// (SMTP server and From email address are obtained automatically
		/// from the .NET mailSettings section of the config file)</summary>
		/// <param name="sendTo">The addresses to send the email to</param>
		/// <param name="sendCC">The addresses to CC the email to</param>
		/// <param name="sendBCC">The addresses to BCC the email to</param>
		/// <param name="subject">The subject of the email</param>
		/// <param name="body">The email message body</param>
		public ClsEmail(string sendTo, string sendCC, string sendBCC,
			string subject, string body)
		{
			_To = sendTo;
			_CC = sendCC;
			_BCC = sendBCC;
			_Subject = subject;
			_Body = body;

			_Attachments = new List<string>();
		}

		/// <summary>Constructor expecting from, to, cc, bcc, subject, body</summary>
		/// <param name="sendFrom">The email address of the sender</param>
		/// <param name="sendTo">The addresses to send the email to</param>
		/// <param name="sendCC">The addresses to CC the email to</param>
		/// <param name="sendBCC">The addresses to BCC the email to</param>
		/// <param name="subject">The subject of the email</param>
		/// <param name="body">The email message body</param>
		public ClsEmail(string sendFrom, string sendTo, string sendCC,
			string sendBCC, string subject, string body)
		{
			_From = sendFrom;
			_To = sendTo;
			_CC = sendCC;
			_BCC = sendBCC;
			_Subject = subject;
			_Body = body;

			_Attachments = new List<string>();
		}
		#endregion		// #region Constructors

		#region Public Methods

		public void CopyFrom(ClsEmail src)
		{
			From = src._From;
			To = src._To;
			CC = src._CC;
			BCC = src._BCC;
			Body = src._Body;
			Subject = src._Subject;
			SMTPServer = src._SMTPServer;

			_Attachments = new List<string>(src._Attachments);
		}

		/// <summary>Add a primary recipient.
		/// Call this method once for each address</summary>
		/// <param name="sendTo">The recipient email address</param>
		public void AddTo(string sendTo)
		{
			_To = ( string.IsNullOrEmpty(_To) == true )
				? sendTo : string.Format("{0},{1}", _To, sendTo);
		}

		/// <summary>Add a CC recipient.
		/// Call this method once for each address</summary>
		/// <param name="sendCC">The recipient email address</param>
		public void AddCC(string sendCC)
		{
			_CC = ( string.IsNullOrEmpty(_CC) == true )
				? sendCC : string.Format("{0},{1}", _CC, sendCC);
		}

		/// <summary>Add a BCC recipient.
		/// Call this method once for each address</summary>
		/// <param name="sendBCC">The recipient email address</param>
		public void AddBCC(string sendBCC)
		{
			_BCC = ( string.IsNullOrEmpty(_BCC) == true )
				? sendBCC : string.Format("{0},{1}", _BCC, sendBCC);
		}

		public void AddAttachment(string fileName)
		{
			if( Attachments.Exists(delegate(string s)
			{ return ( string.Compare(s, fileName, true) == 0 );} ) == false )
				Attachments.Add(fileName);
		}


		public void SendMail()
		{
			SendMail(false);
		}


		/// <summary>Send the email. Before calling this function you must
		/// populate the attributes either through the constructor
		/// new ClsEmail(from, to, subject, msg) or using the From, To, Subject,
		/// and Body properties</summary>
		public void SendMail(bool blnHTML)
		{

			try
			{
				MailMessage mMsg = new MailMessage();

				// Use the From address if it was specified, otherwise it defaults
				// to what is in the .NET mailSettings section of the app.config file
				if (string.IsNullOrEmpty(_From) == false)
					mMsg.From = new MailAddress(_From);

				mMsg.To.Add(_To);
				mMsg.Subject = _Subject;
				mMsg.Body = _Body;
				mMsg.IsBodyHtml = blnHTML;

				if (string.IsNullOrEmpty(_CC) == false) mMsg.CC.Add(_CC);
				if (string.IsNullOrEmpty(_BCC) == false) mMsg.Bcc.Add(_BCC);

				foreach (string s in _Attachments)
				{
					try
					{
						if (string.IsNullOrEmpty(s) == false)
							mMsg.Attachments.Add(new Attachment(s));
					}
					catch (Exception ex)
					{
						Trace.WriteLine("Error adding attachment: " + ex.Message);
					}
				}

				// Use the SMTPServer if it was specified, otherwise it defaults
				// to what is in the .NET mailSettings section of the app.config file
				SmtpClient client = (string.IsNullOrEmpty(_SMTPServer) == true)
					? new SmtpClient() : new SmtpClient(_SMTPServer);
				if (client.Host == "relay.appriver.com")
					client.Port = 2525;
				client.Send(mMsg);
			}
			catch (Exception ex)
			{
				ClsErrorHandler.LogException(ex);
			}
		}

		public static ClsEmail EmailCustomerService(string subject, string txt)
		{
			ClsEmail em = new ClsEmail(null, ClsConfig.CustomerServiceGroup, subject, txt);
			em.SendMail();
			return em;
		}

		public static ClsEmail EmailCustomerService(string sTo, string from, string subject, string txt)
		{
			ClsEmail em = null;
			if( string.IsNullOrEmpty(from) )
				em = new ClsEmail(ClsConfig.CustomerServiceGroup, subject, txt);
			else
				em = new ClsEmail(from, ClsConfig.CustomerServiceGroup, subject, txt);
			if (!string.IsNullOrEmpty(sTo))
				em.AddTo(sTo);
			em.SendMail();
			return em;
		}
		#endregion		// #region Public Methods
	}
}