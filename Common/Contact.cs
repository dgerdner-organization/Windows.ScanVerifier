using System;
using System.Data;
using System.Text;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace CS2010.Common
{
	#region IContact

/*	public interface IContact
	{
		string Name { get; set; }	// contact name
		string Phone1 { get; set; }	// home
		string Phone1Ext { get; set; }
		string Phone2 { get; set; }	// work
		string Phone2Ext { get; set; }
		string Phone3 { get; set; }	// mobile
		string Phone3Ext { get; set; }
		string Fax { get; set; }
		string Email { get; set; }
	}*/
	#endregion		// #region IContact

	#region Contact - IContact Implementation

	/*public class Contact : INotifyPropertyChanged, IContact
	{
		#region Fields

		protected string _Name;
		protected string _Phone1;
		protected string _Phone1Ext;
		protected string _Phone2;
		protected string _Phone2Ext;
		protected string _Phone3;
		protected string _Phone3Ext;
		protected string _Fax;
		protected string _Email;

		#endregion		// #region Fields

		#region Properties

		public string Name
		{
			get { return _Name; }
			set
			{
				if( _Name == value ) return;
				_Name = value;
				NotifyPropertyChanged("Name");
			}
		}

		public string Phone1
		{
			get { return _Phone1; }
			set
			{
				if( _Phone1 == value ) return;
				_Phone1 = value;
				NotifyPropertyChanged("Phone1");
			}
		}

		public string Phone1Ext
		{
			get { return _Phone1Ext; }
			set
			{
				if( _Phone1Ext == value ) return;
				_Phone1Ext = value;
				NotifyPropertyChanged("Phone1Ext");
			}
		}

		public string Phone2
		{
			get { return _Phone2; }
			set
			{
				if( _Phone2 == value ) return;
				_Phone2 = value;
				NotifyPropertyChanged("Phone2");
			}
		}

		public string Phone2Ext
		{
			get { return _Phone2Ext; }
			set
			{
				if( _Phone2Ext == value ) return;
				_Phone2Ext = value;
				NotifyPropertyChanged("Phone2Ext");
			}
		}

		public string Phone3
		{
			get { return _Phone3; }
			set
			{
				if( _Phone3 == value ) return;
				_Phone3 = value;
				NotifyPropertyChanged("Phone3");
			}
		}

		public string Phone3Ext
		{
			get { return _Phone3Ext; }
			set
			{
				if( _Phone3Ext == value ) return;
				_Phone3Ext = value;
				NotifyPropertyChanged("Phone3Ext");
			}
		}

		public string Fax
		{
			get { return _Fax; }
			set
			{
				if( _Fax == value ) return;
				_Fax = value;
				NotifyPropertyChanged("Fax");
			}
		}

		public string Email
		{
			get { return _Email; }
			set
			{
				if( _Email == value ) return;
				_Email = value;
				NotifyPropertyChanged("_Email");
			}
		}
		#endregion		// #region Properties

		#region Readonly Properties

		public string Phone1Format
		{
			get
			{
				if( string.IsNullOrEmpty(Phone1) == true ) return null;
				if( string.IsNullOrEmpty(Phone1Ext) == true ) return Phone1;
				return string.Format("{0} x{1}", Phone1, Phone1Ext);
			}
		}

		public string Phone2Format
		{
			get
			{
				if( string.IsNullOrEmpty(Phone2) == true ) return null;
				if( string.IsNullOrEmpty(Phone2Ext) == true ) return Phone2;
				return string.Format("{0} x{1}", Phone2, Phone2Ext);
			}
		}

		public string Phone3Format
		{
			get
			{
				if( string.IsNullOrEmpty(Phone3) == true ) return null;
				if( string.IsNullOrEmpty(Phone3Ext) == true ) return Phone3;
				return string.Format("{0} x{1}", Phone3, Phone3Ext);
			}
		}

		public string ContactBox
		{
			get { return FormatContact(); }
		}

		public string NameAndContact
		{
			get { return FormatNameAndContact(); }
		}
		#endregion		// #region Readonly Properties

		#region Constructors

		public Contact(string ph1, string ph1Ext, string ph2, string ph2Ext, string ph3,
			string ph3Ext, string faxNo, string emailAddr)
		{
			_Phone1 = ph1;
			_Phone1Ext = ph1Ext;
			_Phone2 = ph2;
			_Phone2Ext = ph2Ext;
			_Phone3 = ph3;
			_Phone3Ext = ph3Ext;
			_Fax = faxNo;
			_Email = emailAddr;
		}

		public Contact(IContact aContact)
		{
			Copy(aContact, this);
		}

		public Contact()
		{
			Reset();
		}
		#endregion		// #region Constructors

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged(String info)
		{
			if( PropertyChanged != null )
				PropertyChanged(this, new PropertyChangedEventArgs(info));
		}
		#endregion		// #region INotifyPropertyChanged Members

		#region Static Methods

		public static void Copy(IContact src, IContact dest)
		{
			dest.Phone1 = src.Phone1;
			dest.Phone1Ext = src.Phone1Ext;
			dest.Phone2 = src.Phone2;
			dest.Phone2Ext = src.Phone2Ext;
			dest.Phone3 = src.Phone3;
			dest.Phone3Ext = src.Phone3Ext;
			dest.Fax = src.Fax;
			dest.Email = src.Email;
		}

		public static bool ValidatePhone(string phone, string ext, StringBuilder sbError)
		{
			if( string.IsNullOrEmpty(phone) == false && phone.Length < 10 )
			{
				sbError.AppendFormat
					("Invalid phone format {0}, must be at least 10 digits\r\n", phone);
				return false;
			}

			if( string.IsNullOrEmpty(phone) == true && string.IsNullOrEmpty(ext) == false )
			{
				sbError.AppendFormat
					("Cannot have an extension {0} without a phone number\r\n", ext);
				return false;
			}

			if( string.IsNullOrEmpty(phone) == false && IsValidPhone(phone) == false )
			{
				sbError.AppendFormat
					("Invalid phone format {0}, must have at least 10 numeric digits\r\n",
					phone);
				return false;
			}

			if( string.IsNullOrEmpty(ext) == false && IsValidExt(ext) == false )
			{
				sbError.AppendFormat
					("Invalid extension format {0}, must contain only numbers\r\n", ext);
				return false;
			}

			return true;
		}

		public static bool IsValidPhone(string phone)
		{
			if( string.IsNullOrEmpty(phone) == true ) return true;

			int numCount = 0;
			foreach( char c in phone )
				if( char.IsDigit(c) == true )
					numCount++;

			return numCount >= 10;
		}

		public static bool IsValidExt(string ext)
		{
			if( string.IsNullOrEmpty(ext) == true ) return true;

			foreach( char c in ext )
				if( char.IsDigit(c) == false ) return false;
			return true;
		}

		public static bool ValidateEmail(string emailAddr)
		{
			return Regex.IsMatch(emailAddr, @"^[A-Z0-9._%-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$",
				RegexOptions.IgnoreCase);
		}

		public static bool ValidateContact(IContact src, StringBuilder sbError)
		{
			ValidatePhone(src.Phone1, src.Phone1Ext, sbError);
			ValidatePhone(src.Phone2, src.Phone2Ext, sbError);
			ValidatePhone(src.Phone3, src.Phone3Ext, sbError);
			ValidatePhone(src.Fax, null, sbError);
			if( ValidateEmail(src.Email) == false )
				sbError.AppendFormat("Invalid email address {0}\r\n", src.Email);

			return sbError.Length == 0;
		}
		#endregion		// #region Static Methods

		#region Public Methods

		public void Reset()
		{
			Phone1 = null;
			Phone1Ext = null;
			Phone2 = null;
			Phone2Ext = null;
			Phone3 = null;
			Phone3Ext = null;
			Fax = null;
			Email = null;
		}

		public void LoadFromDataRow(DataRow dr)
		{
			Phone1 = ClsConvert.ToString(dr["Phone1"]);
			Phone1Ext = ClsConvert.ToString(dr["Phone1Ext"]);
			Phone2 = ClsConvert.ToString(dr["Phone2"]);
			Phone2Ext = ClsConvert.ToString(dr["Phone2Ext"]);
			Phone3 = ClsConvert.ToString(dr["Phone3"]);
			Phone3Ext = ClsConvert.ToString(dr["Phone3Ext"]);
			Fax = ClsConvert.ToString(dr["Fax"]);
			Email = ClsConvert.ToString(dr["Email"]);
		}

		public void CopyToDataRow(DataRow dr)
		{
			dr["Phone1"] = ClsConvert.ToDbObject(_Phone1);
			dr["Phone1Ext"] = ClsConvert.ToDbObject(_Phone1Ext);
			dr["Phone2"] = ClsConvert.ToDbObject(_Phone2);
			dr["Phone2Ext"] = ClsConvert.ToDbObject(_Phone2Ext);
			dr["Phone3"] = ClsConvert.ToDbObject(_Phone3);
			dr["Phone3Ext"] = ClsConvert.ToDbObject(_Phone3Ext);
			dr["Fax"] = ClsConvert.ToDbObject(_Fax);
			dr["Email"] = ClsConvert.ToDbObject(_Email);
		}

		public string FormatContact()
		{
			StringBuilder sb = new StringBuilder();
			string s1 = Phone1Format, s2 = Phone2Format, s3 = Phone3Format;
			if( string.IsNullOrEmpty(s1) == false ) sb.AppendLine(s1);
			if( string.IsNullOrEmpty(s2) == false ) sb.AppendLine(s2);
			if( string.IsNullOrEmpty(s3) == false ) sb.AppendLine(s3);
			if( string.IsNullOrEmpty(_Fax) == false ) sb.AppendLine(_Fax);
			if( string.IsNullOrEmpty(_Email) == false ) sb.AppendLine(_Email);
			return sb.ToString();
		}

		public string FormatContact(string label1, string label2, string label3)
		{
			StringBuilder sb = new StringBuilder();
			string s1 = Phone1Format, s2 = Phone2Format, s3 = Phone3Format;
			if( string.IsNullOrEmpty(s1) == false ) sb.AppendFormat("{0}: {1}", label1, s1);
			if( string.IsNullOrEmpty(s2) == false ) sb.AppendFormat("{0}: {1}", label2, s2);
			if( string.IsNullOrEmpty(s3) == false ) sb.AppendFormat("{0}: {1}", label3, s3);
			if( string.IsNullOrEmpty(_Fax) == false ) sb.AppendLine("Fax: " + _Fax);
			if( string.IsNullOrEmpty(_Email) == false ) sb.AppendLine("Email: " + _Email);
			return sb.ToString();
		}

		public string FormatNameAndContact()
		{
			StringBuilder sb = new StringBuilder();
			string s1 = Name;
			string s2 = FormatContact();
			if( string.IsNullOrEmpty(s1) == false ) sb.AppendLine(s1);
			if( string.IsNullOrEmpty(s2) == false ) sb.AppendLine(s2);
			return sb.ToString();
		}

		public string FormatNameAndContact(string label1, string label2, string label3)
		{
			StringBuilder sb = new StringBuilder();
			string s1 = Name;
			string s2 = FormatContact(label1, label2, label3);
			if( string.IsNullOrEmpty(s1) == false ) sb.AppendLine(s1);
			if( string.IsNullOrEmpty(s2) == false ) sb.AppendLine("Name: " + s2);
			return sb.ToString();
		}

		public static string FormatContact(IContact src)
		{
			Contact ct = new Contact(src);
			return ct.FormatContact();
		}

		public static string FormatContact(IContact src, string label1, string label2,
			string label3)
		{
			Contact ct = new Contact(src);
			return ct.FormatContact(label1, label2, label3);
		}

		public static string FormatNameAndContact(IContact src)
		{
			Contact ct = new Contact(src);
			return ct.FormatNameAndContact();
		}

		public static string FormatNameAndContact(IContact src, string label1, string label2,
			string label3)
		{
			Contact ct = new Contact(src);
			return ct.FormatNameAndContact(label1, label2, label3);
		}
		#endregion		// #region Public Methods
	}*/
	#endregion		// #region Contact - IContact Implementation

	#region Contact

	public class Contact
	{
		#region Static Methods

		public static string _Error;
		public static string Error { get { return _Error; } }

		public static string FormatPhone(string phone, string ext)
		{
			if( string.IsNullOrEmpty(phone) == true ) return null;
			if( string.IsNullOrEmpty(ext) == true ) return phone;
			return string.Format("{0} x{1}", phone, ext);
		}

		public static bool ValidatePhone(string phone, string ext)
		{
			if( string.IsNullOrEmpty(phone) == true && string.IsNullOrEmpty(ext) == false )
			{
				_Error = string.Format
					("Cannot have an extension {0} without a phone number\r\n", ext);
				return false;
			}

			if( string.IsNullOrEmpty(phone) == false && IsValidPhone(phone) == false )
			{
				_Error = string.Format
					("Invalid phone format {0}, must have at least 10 numeric digits\r\n",
					phone);
				return false;
			}

			if( string.IsNullOrEmpty(ext) == false && IsValidExt(ext) == false )
			{
				_Error = string.Format
					("Invalid extension format {0}, must contain only numbers\r\n", ext);
				return false;
			}

			return true;
		}

		public static bool IsValidPhone(string phone)
		{
			if( string.IsNullOrEmpty(phone) == true ) return true;

			int numCount = 0;
			foreach( char c in phone )
				if( char.IsDigit(c) == true )
					numCount++;

			return numCount >= 10;
		}

		public static bool IsValidExt(string ext)
		{
			if( string.IsNullOrEmpty(ext) == true ) return true;

			foreach( char c in ext )
				if( char.IsDigit(c) == false ) return false;
			return true;
		}

		public static bool ValidateEmail(string emailAddr)
		{
			if( string.IsNullOrEmpty(emailAddr) == true ) return true;

			if( Regex.IsMatch(emailAddr, @"^[A-Z0-9._%-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$",
				RegexOptions.IgnoreCase) == false )
			{
				_Error = string.Format("Invalid email address {0}\r\n", emailAddr);
				return false;
			}
			return true;
		}

		public static bool ValidateEmailList(string emailAddrList)
		{
			if( string.IsNullOrEmpty(emailAddrList) == true ) return true;

			string[] addresses = emailAddrList.Split(new char[] { ';', ',' });
			if( addresses == null || addresses.Length <= 0 ) return true;

			StringBuilder sb = new StringBuilder();
			foreach( string s in addresses )
			{
				_Error = null;
				string addr = string.IsNullOrEmpty(s) ? null : s.Trim();
				if( !ValidateEmail(addr) ) sb.Append(Error);
			}

			_Error = sb.ToString();
			return sb.Length == 0;
		}
		#endregion		// #region Static Methods
	}
	#endregion		// #region Contact
}