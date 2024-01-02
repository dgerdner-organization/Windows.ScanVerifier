using System;
using System.Reflection;
using System.Globalization;
using System.Drawing.Design;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;

namespace CS2010.Common
{
	#region Date Handling

	public static class Dates
	{
		#region Constants

		public const string DateFormatDefault = "yyyy-MM-dd";
		public const string DateFormatEdit = "yyMMdd";

		#endregion		// #region Constants

		/// <summary>Checks to see if a date is within a specific range</summary>
		/// <param name="dtCurrent">The date to use as the starting point (usually
		/// the current date which can be obtained from a Connection object). The
		/// earlier and later values will be added to this value to obtain
		/// the lower and upper bounds of the range</param>
		/// <param name="dtCheck">The date to check</param>
		/// <param name="earlier">Offset (in days) from today's date (used to
		/// calculate the lower bound of the range)</param>
		/// <param name="later">Offset (in days) from today's date (used to
		/// calculate the upper bound of the range)</param>
		/// <returns>True if date is in range, false if not</returns>
		public static bool IsReasonableDate(DateTime dtCurrent, DateTime dtCheck,
			int earlier, int later)
		{
			if( dtCurrent.AddDays(later) < dtCheck ) return false;
			if( dtCurrent.AddDays(earlier) > dtCheck ) return false;

			return true;
		}

		/// <summary>
		/// Default: +/- ten days from today is reasonable
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		/// <summary>Checks to see if a date is within a 20 day range of a
		/// specific starting point (10 days earlier, 10 days later)</summary>
		/// <param name="dtCurrent">The date to use as the starting point (usually
		/// the current date which can be obtained from a Connection object). Ten
		/// days will be subtracted/added from this date to get the lower/upper
		/// bounds of the range</param>
		/// <param name="dtCheck">The date to check</param>
		/// <returns>True if date is in range, false if not</returns>
		public static bool IsReasonableDate(DateTime dtCurrent, DateTime dtCheck)
		{
			return IsReasonableDate(dtCurrent, dtCheck, -10, 10);
		}
	}
	#endregion		// #region Date Handling

	#region DateRange structure

	/// <summary>Represents a range of dates</summary>
	[Serializable, TypeConverter(typeof(DateRangeConverter))]
	public struct DateRange
	{
		#region Static Properties/Methods

		private static ConstructorInfo _Constructor;
		private static ConstructorInfo Constructor
		{
			get
			{
				if( _Constructor == null ) LoadConstructor();
				return _Constructor;
			}
		}

		public static readonly DateRange Empty = new DateRange();

		static DateRange()
		{
			LoadConstructor();
		}

		public static void LoadConstructor()
		{
			_Constructor = typeof(DateRange).GetConstructor
				(new Type[] { typeof(DateTime?), typeof(DateTime?) });
		}

		public static InstanceDescriptor GetDescriptor(object val)
		{
			try
			{
				if( val == null || val.GetType() != typeof(DateRange) )
					return new InstanceDescriptor
						(Constructor, new object[] { null, null });

				DateRange range = (DateRange)val;
				return new InstanceDescriptor
					(Constructor, new object[] { range.From, range.To });
			}
			catch
			{
				return new InstanceDescriptor
					(Constructor, new object[] { null, null });
			}
		}

		public static string GetString(object val)
		{
			try
			{
				if( val == null || val.GetType() != typeof(DateRange) )
					return string.Empty;
				DateRange range = (DateRange)val;
				return range.ToString();
			}
			catch
			{
				return string.Empty;
			}
		}
		#endregion		// #region Static Properties/Methods

		#region Fields

		/// <summary>Data storage for the From property</summary>
		private DateTime? _From;
		/// <summary>Data storage for the To property</summary>
		private DateTime? _To;

		#endregion		// #region Fields

		#region Properties

		/// <summary>Gets/Sets the from value of the date range</summary>
		[Browsable(true), DefaultValue(null),
		EditorBrowsable(EditorBrowsableState.Always),
		Editor(typeof(DateTimeEditor), typeof(UITypeEditor)),
		Description("Gets/Sets the from value of the date range")]
		public DateTime? From
		{
			get { return _From; }
			set
			{
				if( _From == value ) return;

				_From = value;
			}
		}

		/// <summary>Gets/Sets the to value of the date range</summary>
		[Browsable(true), DefaultValue(null),
		EditorBrowsable(EditorBrowsableState.Always),
		Editor(typeof(DateTimeEditor), typeof(UITypeEditor)),
		Description("Gets/Sets the to value of the date range")]
		public DateTime? To
		{
			get { return _To; }
			set
			{
				if( _To == value ) return;

				_To = value;
			}
		}

		/// <summary>Gets the from date value of the date range with a time
		/// portion of 12:00:00 AM (useful for SQL queries)</summary>
		[Browsable(false)]
		public DateTime FromDate
		{
			get { return _From.GetValueOrDefault(DateTime.MinValue).Date; }
		}

		/// <summary>Gets the to date value of the date range with a time
		/// portion of 11:59:59 PM (useful for SQL queries)</summary>
		[Browsable(false)]
		public DateTime ToDate
		{
			get
			{
				DateTime t = _To.GetValueOrDefault(DateTime.MaxValue).Date;
				return new DateTime(t.Year, t.Month, t.Day, 23, 59, 59);
			}
		}

		/// <summary>Returns a value of true if both the From and To properties
		/// are null (meaning there is no date range)</summary>
		[Browsable(false)]
		public bool IsEmpty { get { return ( _From == null && _To == null ); } }

		/// <summary>Returns true when a From value was specified without a To
		/// value. Returns false under 3 scenarios: 1) From/To both specified,
		/// 2) From/To both missing, 3) From missing, To specified.</summary>
		/// <example>
		/// From		To			Result
		/// Specified	Missing		True
		/// Specified	Specified	False
		/// Missing		Missing		False
		/// Missing		Specified	False
		/// </example>
		[Browsable(false)]
		public bool OnlyFrom { get { return ( _From != null && _To == null ); } }

		/// <summary>Returns true when a To value was specified without a From
		/// value. Returns false under 3 scenarios: 1) From/To both specified,
		/// 2) From/To both missing, 3) From specified, To missing.</summary>
		/// <example>
		/// From		To			Result
		/// Missing		Specified	True
		/// Specified	Specified	False
		/// Missing		Missing		False
		/// Specified	Missing		False
		/// </example>
		[Browsable(false)]
		public bool OnlyTo { get { return ( _From == null && _To != null ); } }

		#endregion		// #region Properties

		#region Constructors

		/// <summary>Constructor allowing values for From and/or To</summary>
		/// <param name="f">Initial From value (can be null)</param>
		/// <param name="t">Initial To value (can be null)</param>
		public DateRange(DateTime? f, DateTime? t)
		{
			_From = f;
			_To = t;
		}
		#endregion		// #region Constructors

		#region Overrides

		/// <summary>Override of the ToString method that returns a string with
		/// the date range. The string can be empty, it can have a from without a
		/// to portion, a to without a from, or both a from and a to, depending on
		/// the From and To date properties</summary>
		/// <returns>A string representation of the date range</returns>
		public override string ToString()
		{
			if( IsEmpty == true ) return string.Empty;

			if( OnlyFrom == true )
				return string.Format("From {0}", ClsConfig.FormatDate(_From));
			else if( OnlyTo == true )
				return string.Format("To {0}", ClsConfig.FormatDate(_To));
			else
				return string.Format("From {0} To {1}",
					ClsConfig.FormatDate(_From), ClsConfig.FormatDate(_To));
		}

		/// <summary>Override of the Equals method</summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>True if the date range is the same, false if not</returns>
		public override bool Equals(object obj)
		{
			if( obj == null || GetType() != obj.GetType() ) return false;

			DateRange r = (DateRange)obj;
			return IsEqual(this, r);
		}

		/// <summary>Override of the GetHashCode method. It returns an XOR of the
		/// hash codes of the From and To properties</summary>
		/// <returns>The hash value of the date range</returns>
		public override int GetHashCode()
		{
			return From.GetHashCode() ^ To.GetHashCode();
		}
		#endregion		// #region Overrides

		#region Public Methods

		/// <summary>Reset the from and to fields to null</summary>
		public void Clear()
		{
			From = null;
			To = null;
		}

		/// <summary>Combines the column name and the from and to variable names
		/// into a string that can be used in a WHERE SQL clause. It takes into
		/// account whether there is a From and/or To date and will return an
		/// empty string if neither exists, a statement with a >= if there is
		/// a From without a To, a statement with a &lt;= if there is a To without
		/// a From, or a between statement if both From and To exist.
		/// <para>For example: colName = bk.Cutoff_Dt, fromName = @CTOFROM,
		/// and toName = @CTOTO, would produce the following:</para>
		/// <para>"bk.Cutoff_Dt >= @CTOFROM" if From != null and To == null,
		/// "bk.Cutoff_Dt &lt;= @CTOFROM" if From == null and To != null, or</para>
		/// <para>"bk.Cutoff_Dt BETWEEN @CTOFROM AND @CTOTO" if
		/// From != null and To != null</para></summary>
		/// <param name="colName">The name of the column to compare</param>
		/// <param name="fromName">The name of the variable that will be passed
		/// to the database if there is a From date</param>
		/// <param name="toName">The name of the variable that will be passed
		/// to the database if there is a To date</param>
		/// <returns>A string that can be appended to a WHERE clause</returns>
		public string WhereClause(string colName, string fromName, string toName)
		{
			if( IsEmpty == true ) return string.Empty;

			if( OnlyFrom == true )
				return string.Format(" TRUNC({0}) >= TRUNC({1}) ", colName, fromName);
			else if( OnlyTo == true )
				return string.Format(" TRUNC({0}) <= TRUNC({1}) ", colName, toName);
			else
				return string.Format(" TRUNC({0}) BETWEEN TRUNC({1}) AND TRUNC({2}) ",
					colName, fromName, toName);
		}

		public string WhereClauseSqlServer(string colName, string fromName,
			string toName)
		{
			if( IsEmpty == true ) return string.Empty;

			if( OnlyFrom == true )
				return string.Format(" CAST(FLOOR(CAST({0} AS FLOAT)) AS DATETIME) >= CAST(FLOOR(CAST({1} AS FLOAT)) AS DATETIME) ", colName, fromName);
			else if( OnlyTo == true )
				return string.Format(" CAST(FLOOR(CAST({0} AS FLOAT)) AS DATETIME) <= CAST(FLOOR(CAST({1} AS FLOAT)) AS DATETIME) ", colName, toName);
			else
				return string.Format(" CAST(FLOOR(CAST({0} AS FLOAT)) AS DATETIME) BETWEEN CAST(FLOOR(CAST({1} AS FLOAT)) AS DATETIME) AND CAST(FLOOR(CAST({2} AS FLOAT)) AS DATETIME) ",
					colName, fromName, toName);
		}
		#endregion		// #region Public Methods

		#region Static Methods

		public static bool operator ==(DateRange l, DateRange r)
		{
			return IsEqual(l, r);
		}

		public static bool operator !=(DateRange l, DateRange r)
		{
			return !IsEqual(l, r);
		}

		/// <summary>Compares two date range values</summary>
		/// <param name="l">The first object to compare</param>
		/// <param name="r">The second object to compare</param>
		/// <returns>True if the ranges are equal, false if not</returns>
		public static bool IsEqual(DateRange l, DateRange r)
		{
			if( l.IsEmpty == true && r.IsEmpty == true ) return true;

			bool fromMatch = false, toMatch = false;
			if( l.From == null && r.From == null )
				fromMatch = true;
			else if( l.From != null && r.From != null
				&& l.From.Value == r.From.Value ) fromMatch = true;

			if( l.To == null && r.To == null )
				toMatch = true;
			else if( l.To != null && r.To != null && l.To.Value == r.To.Value )
				toMatch = true;

			return ( fromMatch == true && toMatch == true );
		}
		#endregion		// #region Static Methods
	}
	#endregion		// #region DateRange structure

	#region DateRangeConverter class

	/// <summary>Provides a type converter to convert DateRange objects to/from
	/// other representations. Deriving the class from ExpandableObjectConverter
	/// allows a DateRange to be displayed in the property grid as a nested type.
	/// Conversion between object types and the ability to modify the DateRange
	/// directly through the property grid is accomplished/provided by overriding
	/// the various "Convert" methods. The following attribute should be
	/// applied to the DateRange structure (above its declaration) to achieve
	/// this behavior [TypeConverter(typeof(DateRangeConverter))]</summary>
	/// <remarks>Visual Studio should be restarted if changes are made to this
	/// class because: 1) changes will not take effect until after the restart
	/// (because of caching), and the designer may become unstable</remarks>
	public class DateRangeConverter : ExpandableObjectConverter
	{
		#region Overrides

		/// <summary>This method will return true for those types that can be
		/// converted to a DateRange (the code to perform the conversion should
		/// be added to the ConvertFrom method)</summary>
		public override bool CanConvertFrom(ITypeDescriptorContext context,
			Type sourceType)
		{
			// Allow conversion from string
			if( sourceType == typeof(string) ) return true;

			return base.CanConvertFrom(context, sourceType);
		}

		/// <summary>This method converts an object to a DateRange (the code
		/// that indicates whether the conversion can take place should be
		/// added to the CanConvertFrom method)</summary>
		public override object ConvertFrom(ITypeDescriptorContext context,
		   CultureInfo culture, object value)
		{
			if( value == null ) return new DateRange();

			if( value is string )		// Convert a string to a DateRange
			{	// The string should be in one of the formats that is output
				// by the DateRange.ToString() method.
				string v = value as string;
				if( v != null ) v = v.ToLower();
				int fromIndex = v.IndexOf("from");
				int toIndex = v.IndexOf("to");

				DateTime? fdate = null;
				if( fromIndex >= 0 )
				{
					string s = v.Substring(fromIndex + 5).Trim();
					string[] strs = s.Split(null);
					if( strs != null || strs.Length > 0 )
						fdate = DateTime.Parse(strs[0]);
				}
				DateTime? tdate = null;
				if( toIndex >= 0 )
				{
					string s = v.Substring(toIndex + 3).Trim();
					string[] strs = s.Split(null);
					if( strs != null || strs.Length > 0 )
						tdate = DateTime.Parse(strs[0]);
				}

				return new DateRange(fdate, tdate);
			}

			return base.ConvertFrom(context, culture, value);
		}

		/// <summary>This method will return true for the types that a DateRange
		/// can be converted to (the code to perform the conversion should
		/// be added to the ConvertTo method)</summary>
		public override bool CanConvertTo(ITypeDescriptorContext context,
			Type destinationType)
		{	// Allowing conversion to an InstanceDescriptor is what allows a
			// DateRange to modified through the property grid
			if( destinationType == typeof(InstanceDescriptor) ||
				destinationType == typeof(string) ) return true;
			return base.CanConvertTo(context, destinationType);
		}

		/// <summary>This method converts a DateRange to another type (the code
		/// that indicates whether the conversion can take place should be
		/// added to the CanConvertTo method)</summary>
		public override object ConvertTo(ITypeDescriptorContext context,
			CultureInfo culture, object value, Type destinationType)
		{
			if( destinationType == typeof(InstanceDescriptor) )
			{	// Required to allow a DateRange to modified in property grid
				return DateRange.GetDescriptor(value);
			}
			else if( destinationType == typeof(string) )
			{	// Conversion to a string
				return DateRange.GetString(value);
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
		#endregion		// #region Overrides
	}
	#endregion		// #region DateRangeConverter class
}