using System;
using System.Data;
using System.Text;
using System.ComponentModel;

namespace CS2010.Common
{
	#region Address Structures

	public struct CityType
	{
		/// <summary>Default - This is the "preferred" name - by the USPS - for a city.
		/// Each ZIP Code has one - and only one - "default" name. In most cases, this is
		/// what people who live in that area call the city as well.</summary>
		public const string Default = "D";
		/// <summary>Acceptable - This name can be used for mailing purposes. Often times
		/// alternative names are large neighborhoods or sections of the city/town. In
		/// some cases a ZIP Code may have several "acceptable" names which is used to
		/// group towns under one ZIP Code.</summary>
		public const string Acceptable = "A";
		/// <summary>Not Acceptable - This name is, in many cases, a nickname that
		/// residents give that location. According to the USPS, you should NOT send mail
		/// to that ZIP Code using the "not acceptable" name when mailing.</summary>
		public const string NotAcceptable = "N";
	}

	public struct PostalType
	{
		/// <summary>Standard - A "standard" ZIP Code is what most people think of when
		/// they talk about ZIP Codes - essentially a town, city, or a division of a
		/// city that has mail service.</summary>
		public const string Standard = "S";
		/// <summary>PO Box Only - Rural towns, groups of towns, or even high-growth areas
		/// of cities are given a "PO Box Only" ZIP Code type.</summary>
		public const string POBoxOnly = "P";
		/// <summary>Unique - Companies, organizations, and institutions that receive
		/// large quantities of mail are given a "unique" ZIP Code type.</summary>
		public const string Unique = "U";
		/// <summary>Military - Military bases overseas - and often vessels and ships -
		/// are given a "military" ZIP Code type</summary>
		public const string Military = "M";
	}
	#endregion		// #region Address Structures

	#region Database Table/Column Names

	public struct PostalDB
	{
		/// <summary>Database name of the postal code table</summary>
		public const string PostalName = "R_POSTAL";
		/// <summary>Name of the column in the <see cref="PostalTable"/>
		/// that holds the city names</summary>
		public const string CityColumn = "Postal_City";
		/// <summary>Name of the column in the <see cref="PostalTable"/>
		/// that holds the state codes</summary>
		public const string StateColumn = "State_Prov_Cd";
		/// <summary>Name of the column in the <see cref="PostalTable"/>
		/// that holds the postal code</summary>
		public const string PostalCdColumn = "Postal_Cd";
		/// <summary>Name of the column in the <see cref="PostalTable"/>
		/// that holds the city type</summary>
		public const string CityTypeColumn = "City_Type";
		/// <summary>Name of the column in the <see cref="PostalTable"/>
		/// that holds the city type</summary>
		public const string PostalTypeColumn = "Postal_Type_Cd";
		/// <summary>PostalTypeColumn value for military postal codess (APOs)</summary>
		public const string APOType = "M";
	}
	#endregion

	#region IAddress

	public interface IAddress
	{
		string Addr1 { get; set; }
		string Addr2 { get; set; }
		string Addr3 { get; set; }
		string City { get; set; }
		string State_Prov_Cd { get; set; }
		string Postal_Cd { get; set; }
		string Country_Cd { get; set; }
	}
	#endregion		// #region IAddress

	#region Address - IAddress Implementation

	public class Address : INotifyPropertyChanged, IAddress
	{
		#region Fields

		protected string _Addr1;
		protected string _Addr2;
		protected string _Addr3;
		protected string _City;
		protected string _State_Prov_Cd;
		protected string _Postal_Cd;
		protected string _Country_Cd;

		#endregion		// #region Fields

		#region Properties

		public string Addr1
		{
			get { return _Addr1; }
			set
			{
				if( _Addr1 == value ) return;
				_Addr1 = value;
				NotifyPropertyChanged("Addr1"); }
		}
		public string Addr2
		{
			get { return _Addr2; }
			set
			{
				if( _Addr2 == value ) return;
				_Addr2 = value;
				NotifyPropertyChanged("Addr2"); }
		}
		public string Addr3
		{
			get { return _Addr3; }
			set
			{
				if( _Addr3 == value ) return;
				_Addr3 = value;
				NotifyPropertyChanged("Addr3");
			}
		}
		public string City
		{
			get { return _City; }
			set
			{
				if( _City == value ) return;
				_City = value;
				NotifyPropertyChanged("City");
			}
		}
		public string State_Prov_Cd
		{
			get { return _State_Prov_Cd; }
			set
			{
				if( _State_Prov_Cd == value ) return;
				_State_Prov_Cd = value;
				NotifyPropertyChanged("State_Prov_Cd");
			}
		}
		public string Postal_Cd
		{
			get { return _Postal_Cd; }
			set
			{
				if( _Postal_Cd == value ) return;
				_Postal_Cd = value;
				NotifyPropertyChanged("Postal_Cd");
			}
		}
		public string Country_Cd
		{
			get { return _Country_Cd; }
			set
			{
				if( _Country_Cd == value ) return;
				_Country_Cd = value;
				NotifyPropertyChanged("Country_Cd");
			}
		}

		public string AddressBox
		{
			get
			{
				return string.Format("{0}\r\n{1}\r\n{2}\r\n{3} {4} {5}\r\n{6}",
					Addr1, Addr2, Addr3, City, State_Prov_Cd, Postal_Cd,
					Country_Cd);
			}
		}

		public string GetAddressBox(bool countryOnNewLine)
		{
			return string.Format("{0}\r\n{1}\r\n{2}\r\n{3} {4} {5}{6}{7}",
				Addr1, Addr2, Addr3, City, State_Prov_Cd, Postal_Cd,
				(countryOnNewLine ? "\r\n" : " "), Country_Cd);
		}
		#endregion		// #region Properties

		#region Constructors

		public Address(string line1, string line2, string line3, string aCity,
			string aState, string aZip, string aCountry)
		{
			_Addr1 = line1;
			_Addr2 = line2;
			_Addr3 = line3;
			_City = aCity;
			_State_Prov_Cd = aState;
			_Postal_Cd = aZip;
			_Country_Cd = aCountry;
		}

		public Address(IAddress anAddr)
		{
			Copy(anAddr, this);
		}

		public Address()
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

		public static void Copy(IAddress src, IAddress dest)
		{
			dest.Addr1 = src.Addr1;
			dest.Addr2 = src.Addr2;
			dest.Addr3 = src.Addr3;
			dest.City = src.City;
			dest.State_Prov_Cd = src.State_Prov_Cd;
			dest.Postal_Cd = src.Postal_Cd;
			dest.Country_Cd = src.Country_Cd;
		}

		public static string FormatAddress(IAddress src)
		{
			Address addr = new Address(src);
			return addr.FormatAddress();
		}

		public static bool ValidateAddress(IAddress src, StringBuilder sbError)
		{
			if( string.IsNullOrEmpty(src.Addr1) == true &&
				string.IsNullOrEmpty(src.Addr2) == true &&
				string.IsNullOrEmpty(src.Addr3) == true )
				sbError.AppendLine("Missing address line");
			if( string.IsNullOrEmpty(src.City) == true )
				sbError.AppendLine("Missing city");
			if( string.IsNullOrEmpty(src.Country_Cd) == true )
				sbError.AppendLine("Missing country code");
			else if( src.Country_Cd.Length > 1 )
			{
				if( src.Country_Cd.StartsWith("US", true, null) == true )
				{
					if( string.IsNullOrEmpty(src.State_Prov_Cd) == true )
						sbError.AppendLine("Missing state/province code");
					if( ValidatePostalCode(src) == false )
						sbError.AppendLine("Missing or invalid zip code");
				}
			}

			if( ValidateText(src.Addr1) == false )
				sbError.AppendLine("Addr1 contains one or more invalid characters");
			if( ValidateText(src.Addr2) == false )
				sbError.AppendLine("Addr2 contains one or more invalid characters");
			if( ValidateText(src.Addr3) == false )
				sbError.AppendLine("Addr3 contains one or more invalid characters");
			if( ValidateText(src.City) == false )
				sbError.AppendLine("City contains one or more invalid characters");
			if( ValidateText(src.State_Prov_Cd) == false )
				sbError.AppendLine("State/Prov contains one or more invalid characters");
			if( ValidateText(src.Country_Cd) == false )
				sbError.AppendLine("Country contains one or more invalid characters");
			if( ValidateText(src.Postal_Cd) == false )
				sbError.AppendLine("Postal code contains one or more invalid characters");

			return sbError.Length == 0;
		}

		protected static bool ValidateText(string s)
		{
			if( string.IsNullOrEmpty(s) ) return true;

			foreach( char c in s )
				if( !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c) && c != '/' && c != '-' )
					return false;

			return true;
		}

		protected static bool ValidatePostalCode(IAddress src)
		{
			string zip = ( string.IsNullOrEmpty(src.Postal_Cd) == true )
				? string.Empty : src.Postal_Cd.Trim();
			StringBuilder sb = new StringBuilder(zip);
			sb.Replace(" ", null).Replace("-", null);
			if( string.Compare(src.Country_Cd, "USA", true) == 0 ||
				string.Compare(src.Country_Cd, "US", true) == 0)
				return ( sb.Length == 5 || sb.Length == 9 );
			return ( sb.Length > 0 );
		}
		#endregion		// #region Static Methods

		#region Public Methods

		public void Reset()
		{
			Addr1 = null;
			Addr2 = null;
			Addr3 = null;
			City = null;
			State_Prov_Cd = null;
			Postal_Cd = null;
			Country_Cd = null;
		}

		public void LoadFromDataRow(DataRow dr)
		{
			Addr1 = ClsConvert.ToString(dr["Addr1"]);
			Addr2 = ClsConvert.ToString(dr["Addr2"]);
			Addr3 = ClsConvert.ToString(dr["Addr3"]);
			City = ClsConvert.ToString(dr["City"]);
			State_Prov_Cd = ClsConvert.ToString(dr["State_Prov_Cd"]);
			Postal_Cd = ClsConvert.ToString(dr["Postal_Cd"]);
			Country_Cd = ClsConvert.ToString(dr["Country_Cd"]);
		}

		public void CopyToDataRow(DataRow dr)
		{
			dr["Addr1"] = ClsConvert.ToDbObject(_Addr1);
			dr["Addr2"] = ClsConvert.ToDbObject(_Addr2);
			dr["Addr3"] = ClsConvert.ToDbObject(_Addr3);
			dr["City"] = ClsConvert.ToDbObject(_City);
			dr["State_Prov_Cd"] = ClsConvert.ToDbObject(_State_Prov_Cd);
			dr["Postal_Cd"] = ClsConvert.ToDbObject(_Postal_Cd);
			dr["Country_Cd"] = ClsConvert.ToDbObject(_Country_Cd);
		}

		public string FormatAddress(bool countryOnNewLine)
		{
			StringBuilder sb = new StringBuilder();

			if( string.IsNullOrEmpty(Addr1) == false )
				sb.AppendLine(this.Addr1);

			if( string.IsNullOrEmpty(Addr2) == false )
				sb.AppendLine(this.Addr2);

			if( string.IsNullOrEmpty(Addr3) == false )
				sb.AppendLine(this.Addr3);

			string crlf = (countryOnNewLine)? "\r\n" : " ";
			if (string.IsNullOrEmpty(City) == false &&
				string.IsNullOrEmpty(State_Prov_Cd) == false)
				sb.AppendFormat("{0}, {1} {2}{3}", City, State_Prov_Cd,
					Postal_Cd, crlf);
			else if (string.IsNullOrEmpty(City) == false)
				sb.AppendFormat("{0} {1}{2}", City, Postal_Cd, crlf);
			else if (string.IsNullOrEmpty(State_Prov_Cd) == false)
				sb.AppendFormat("{0} {1}{2}", State_Prov_Cd, Postal_Cd, crlf);
			else
				sb.AppendFormat("{0}{1}", Postal_Cd, crlf);

			if( string.IsNullOrEmpty(Country_Cd) == false )
				sb.AppendLine(this.Country_Cd);

			return sb.ToString();
		}

		public string FormatAddress()
		{
			return FormatAddress(true);
		}
		#endregion		// #region Public Methods
	}
	#endregion		// #region Address - IAddress Implementation
}