using System;
using System.IO;
using System.Data;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;

namespace CS2010.Common
{
	public class ClsConvert
	{
		#region Data Conversion

		public static string BoolToYN(bool val)
		{
			return ( val == true ) ? "Y" : "N";
		}

		public static string YNToggleNull(string val)
		{
			if( string.IsNullOrEmpty(val) ) return null;
			bool yn = YNToBool(val);
			return BoolToYN(!yn);
		}

		public static bool YNToBool(string val)
		{
			if( string.IsNullOrEmpty(val) == true ) return false;
			return ( char.ToUpper(val[0]) == 'Y' );
		}

		public static string YNToActiveInactive(string val)
		{
			if (val == "Y")
				return "Active";
			return "Inactive";
		}

		public static bool ValidateYN(string val)
		{
			if (string.IsNullOrEmpty(val)) return false;
			return val == "Y" || val == "N";
		}

		public static bool StringToBool(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true ) return false;
			return YNToBool(obj as string);
		}
		public static string BlobToString(byte[] blobIn)
		{
			string strOut;
			System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
			strOut = enc.GetString(blobIn).ToString();
			return strOut;
		}
		public static byte[] StringToBlob(string strIn)
		{
			byte[] blobOut;
			System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
			blobOut = enc.GetBytes(strIn.ToString());
			return blobOut;
		}


		public static bool? YNToBoolNull(string val)
		{
			if( string.IsNullOrEmpty(val) == true ) return null;
			return ( char.ToUpper(val[0]) == 'Y' );
		}

		/// <summary>Convert a DB object to a Byte array</summary>
		/// <param name="obj">The object to convert</param>
		/// <returns>The object as a byte array or null if the object is DBNull</returns>
		public static byte[] ToByteArray(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true ) return null;
			return (byte[])obj;
		}

		/// <summary>Convert a DB object to a Boolean</summary>
		/// <param name="obj">The object to convert</param>
		/// <returns>The object as a Boolean (false if the object is DBNull)</returns>
		public static Boolean ToBoolean(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true )
				return false;
			return Convert.ToBoolean(obj, CultureInfo.InvariantCulture);
		}
		public static Boolean? ToBooleanNullable(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true ) return null;
			return Convert.ToBoolean(obj, CultureInfo.InvariantCulture);
		}

		/// <summary>Convert a DB object to a Byte</summary>
		/// <param name="obj">The object to convert</param>
		/// <returns>The object converted to a Byte. Returns
		/// Byte.MinValue if the object is DBNull</returns>
		public static Byte ToByte(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true )
				return Byte.MinValue;
			return Convert.ToByte(obj, CultureInfo.InvariantCulture);
		}
		public static Byte? ToByteNullable(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true ) return null;
			return Convert.ToByte(obj, CultureInfo.InvariantCulture);
		}

		/// <summary>Convert a DB object to a Char</summary>
		/// <param name="obj">The object to convert</param>
		/// <returns>The object converted to a Char. Returns
		/// Char.MinValue if the object is DBNull</returns>
		public static Char ToChar(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true )
				return Char.MinValue;
			return Convert.ToChar(obj, CultureInfo.InvariantCulture);
		}
		public static Char? ToCharNullable(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true ) return null;
			return Convert.ToChar(obj, CultureInfo.InvariantCulture);
		}

		/// <summary>Convert a DB object to a DateTime</summary>
		/// <param name="obj">The object to convert</param>
		/// <returns>The object converted to a DateTime. Returns
		/// DateTime.MinValue if the object is DBNull</returns>
		public static DateTime ToDateTime(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true )
				return DateTime.MinValue;
			return Convert.ToDateTime(obj, CultureInfo.InvariantCulture);
		}
		public static DateTime? ToDateTimeNullable(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true ) return null;
			return Convert.ToDateTime(obj, CultureInfo.InvariantCulture);
		}

		/// <summary>Convert a DB object to a Decimal</summary>
		/// <param name="obj">The object to convert</param>
		/// <returns>The object converted to a Decimal. Returns
		/// Decimal.MinValue if the object is DBNull</returns>
		public static Decimal ToDecimal(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true )
				return Decimal.MinValue;
			return Convert.ToDecimal(obj, CultureInfo.InvariantCulture);
		}
		public static Decimal? ToDecimalNullable(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true ) return null;
			return Convert.ToDecimal(obj, CultureInfo.InvariantCulture);
		}

		/// <summary>Convert a DB object to a Double</summary>
		/// <param name="obj">The object to convert</param>
		/// <returns>The Double representation of the object or
		/// Double.MinValue if the object is DBNull</returns>
		public static Double ToDouble(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true )
				return Double.MinValue;
			return Convert.ToDouble(obj, CultureInfo.InvariantCulture);
		}
		public static Double? ToDoubleNullable(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true ) return null;
			return Convert.ToDouble(obj, CultureInfo.InvariantCulture);
		}

		/// <summary>Convert a DB object to a Int16</summary>
		/// <param name="obj">The object to convert</param>
		/// <returns>The Int16 representation of the object or
		/// Int16.MinValue if the object is DBNull</returns>
		public static Int16 ToInt16(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true )
				return Int16.MinValue;
			return Convert.ToInt16(obj, CultureInfo.InvariantCulture);
		}
		public static Int16? ToInt16Nullable(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true ) return null;
			return Convert.ToInt16(obj, CultureInfo.InvariantCulture);
		}

		/// <summary>Convert a DB object to a Int32</summary>
		/// <param name="obj">The object to convert</param>
		/// <returns>The Int32 representation of the object or
		/// Int32.MinValue if the object is DBNull</returns>
		public static Int32 ToInt32(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true )
				return Int32.MinValue;
			return Convert.ToInt32(obj, CultureInfo.InvariantCulture);
		}
		public static Int32? ToInt32Nullable(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true ) return null;
			return Convert.ToInt32(obj, CultureInfo.InvariantCulture);
		}

		/// <summary>Convert a DB object to a Int64</summary>
		/// <param name="obj">The object to convert</param>
		/// <returns>The Int64 representation of the object or
		/// Int64.MinValue if the object is DBNull</returns>
		public static Int64 ToInt64(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true )
				return Int64.MinValue;
			return Convert.ToInt64(obj, CultureInfo.InvariantCulture);
		}
		public static Int64? ToInt64Nullable(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true ) return null;
			return Convert.ToInt64(obj, CultureInfo.InvariantCulture);
		}

		/// <summary>Convert a DB object to an SByte</summary>
		/// <param name="obj">The object to convert</param>
		/// <returns>The object converted to an SByte. Returns
		/// SByte.MinValue if the object is DBNull</returns>
		public static SByte ToSByte(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true )
				return SByte.MinValue;
			return Convert.ToSByte(obj, CultureInfo.InvariantCulture);
		}
		public static SByte? ToSByteNullable(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true ) return null;
			return Convert.ToSByte(obj, CultureInfo.InvariantCulture);
		}

		/// <summary>Convert a DB object to a Single</summary>
		/// <param name="obj">The object to convert</param>
		/// <returns>The Single representation of the object or
		/// Single.MinValue if the object is DBNull</returns>
		public static Single ToSingle(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true )
				return Single.MinValue;
			return Convert.ToSingle(obj, CultureInfo.InvariantCulture);
		}
		public static Single? ToSingleNullable(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true ) return null;
			return Convert.ToSingle(obj, CultureInfo.InvariantCulture);
		}

		/// <summary>Convert a DB object to a String</summary>
		/// <param name="obj">The object to convert</param>
		/// <returns>The Single representation of the object or
		/// Single.Empty if the object is DBNull</returns>
		public static String ToString(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true )
				return null;
			return Convert.ToString(obj, CultureInfo.InvariantCulture);
		}

		/// <summary>Convert a DB object to a UInt16</summary>
		/// <param name="obj">The object to convert</param>
		/// <returns>The UInt16 representation of the object or
		/// UInt16.MinValue if the object is DBNull</returns>
		public static UInt16 ToUInt16(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true )
				return UInt16.MinValue;
			return Convert.ToUInt16(obj, CultureInfo.InvariantCulture);
		}
		public static UInt16? ToUInt16Nullable(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true ) return null;
			return Convert.ToUInt16(obj, CultureInfo.InvariantCulture);
		}

		/// <summary>Convert a DB object to a UInt32</summary>
		/// <param name="obj">The object to convert</param>
		/// <returns>The UInt32 representation of the object or
		/// UInt32.MinValue if the object is DBNull</returns>
		public static UInt32 ToUInt32(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true )
				return UInt32.MinValue;
			return Convert.ToUInt32(obj, CultureInfo.InvariantCulture);
		}
		public static UInt32? ToUInt32Nullable(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true ) return null;
			return Convert.ToUInt32(obj, CultureInfo.InvariantCulture);
		}

		/// <summary>Convert a DB object to a UInt64</summary>
		/// <param name="obj">The object to convert</param>
		/// <returns>The UInt64 representation of the object or
		/// UInt64.MinValue if the object is DBNull</returns>
		public static UInt64 ToUInt64(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true )
				return UInt64.MinValue;
			return Convert.ToUInt64(obj, CultureInfo.InvariantCulture);
		}
		public static UInt64? ToUInt64Nullable(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true ) return null;
			return Convert.ToUInt64(obj, CultureInfo.InvariantCulture);
		}

		public static string ToProperCase(string s)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder(); 
			bool fEmptyBefore = true; 
			foreach (char ch in s) 
			{ 
				char chThis = ch; 
				if (Char.IsWhiteSpace(chThis))
					fEmptyBefore = true; 
				else 
					{ if (Char.IsLetter(chThis) && fEmptyBefore)
						  chThis = Char.ToUpper(chThis); 
					  else
						  chThis = Char.ToLower(chThis); 
					 fEmptyBefore = false; 
					} 
				sb.Append(chThis); 
			} return sb.ToString(); 
		}

		/// <summary>
		/// Convert null into DBNull.Value. No effect on other local objects.
		/// </summary>
		public static object ToDbObject(object obj)
		{
			if( obj == null || Convert.IsDBNull(obj) == true )
				return DBNull.Value;

			Type otype = obj.GetType();
			switch( Type.GetTypeCode(otype) )
			{// JR check null
				case TypeCode.Char:
					Char c = (Char)obj;
					if( c == Char.MinValue ) return System.DBNull.Value;
					break;

				case TypeCode.DateTime:
					DateTime dt = (DateTime)obj;
					if( dt == DateTime.MinValue ) return System.DBNull.Value;
					break;

				case TypeCode.Decimal:
					Decimal decVal = (Decimal)obj;
					if( decVal == Decimal.MinValue ) return System.DBNull.Value;
					break;

				case TypeCode.Double:
					Double dbl = (Double)obj;
					if( dbl == Double.MinValue ) return System.DBNull.Value;
					break;

				case TypeCode.Empty:
					return System.DBNull.Value;

				case TypeCode.Int16:
					Int16 i16 = (Int16)obj;
					if( i16 == Int16.MinValue ) return System.DBNull.Value;
					break;

				case TypeCode.Int32:
					Int32 i32 = (Int32)obj;
					if( i32 == Int32.MinValue ) return System.DBNull.Value;
					break;

				case TypeCode.Int64:
					Int64 i64 = (Int64)obj;
					if( i64 == Int64.MinValue ) return System.DBNull.Value;
					break;

				case TypeCode.SByte:
					SByte sb = (SByte)obj;
					if( sb == SByte.MinValue ) return System.DBNull.Value;
					break;

				case TypeCode.Single:
					Single sing = (Single)obj;
					if( sing == Single.MinValue ) return System.DBNull.Value;
					break;

				case TypeCode.String:
					String str = (String)obj;
					if( str.Trim().Length == 0 ) return System.DBNull.Value;
					break;

				case TypeCode.UInt16:
					UInt16 ui16 = (UInt16)obj;
					if( ui16 == UInt16.MinValue ) return System.DBNull.Value;
					break;

				case TypeCode.UInt32:
					UInt32 ui32 = (UInt32)obj;
					if( ui32 == UInt32.MinValue ) return System.DBNull.Value;
					break;

				case TypeCode.UInt64:
					UInt64 ui64 = (UInt64)obj;
					if( ui64 == UInt64.MinValue ) return System.DBNull.Value;
					break;

				case TypeCode.Byte:
				case TypeCode.Boolean:
				case TypeCode.DBNull:
				case TypeCode.Object:
				default: break;
			}
			return obj;
		}

		/// <summary>Return the null equivalent of the given type</summary>
		public static object NullEquivalent(Type someType)
		{
			if( someType == null ) return null;

			switch( Type.GetTypeCode(someType) )
			{
				case TypeCode.Byte:		return Byte.MinValue;
				case TypeCode.Char:		return Char.MinValue;
				case TypeCode.DateTime:	return DateTime.MinValue;
				case TypeCode.Decimal:	return Decimal.MinValue;
				case TypeCode.Double:	return Double.MinValue;
				case TypeCode.Empty:	return null;
				case TypeCode.Int16:	return Int16.MinValue;
				case TypeCode.Int32:	return Int32.MinValue;
				case TypeCode.Int64:	return Int64.MinValue;
				case TypeCode.SByte:	return SByte.MinValue;
				case TypeCode.Single:	return Single.MinValue;
				case TypeCode.String:	return String.Empty;
				case TypeCode.UInt16:	return UInt16.MinValue;
				case TypeCode.UInt32:	return UInt32.MinValue;
				case TypeCode.UInt64:	return UInt64.MinValue;
				case TypeCode.Boolean:	return null;
				case TypeCode.DBNull:	return null;
				case TypeCode.Object:	return null;
				default: break;
			}
			return null;
		}
		/// <summary>
		/// Given a comma-delimited string add quotes around each field so
		/// it can easily be passed to a Where In clause.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string AddQuotes(string s)
		{
			if( string.IsNullOrEmpty(s) == true ) return null;

			if (s.IndexOf("'") > -1)
			{
				return s;
			}
			string str = s.Replace(",", @"','");
			str = "'" + str + "'";
			return str;
		}

		/// <summary>Return a string with asterisks (*) replaced by percents (%),
		/// and ensure that the string ends in a wild card. May return null if
		/// the given string was null.empty</summary>
		/// <param name="val">The string to examine</param>
		/// <returns>A string that can be used in Oracle sql statements or
		/// null if the passed string was null/empty.</returns>
		public static string AddWildCard(string val)
		{
			if( string.IsNullOrEmpty(val) == true ) return null;

			string s = val.Replace('*', '%');
			return ( s.EndsWith("%") == true ) ? s : s + '%';
		}

		/// <summary>Return a string with asterisks (*) replaced by percents (%).
		/// May return null if the given string was null.empty</summary>
		/// <param name="val">The string to examine</param>
		/// <returns>A string that can be used in Oracle sql statements or
		/// null if the passed string was null/empty.</returns>
		public static string ReplaceWildCard(string val)
		{
			return ( string.IsNullOrEmpty(val) == false )
				? val.Replace('*', '%') : null;
		}

		/// <summary>Return a string with asterisks (*) replaced by percents (%).
		/// May return null if the given string was null.empty</summary>
		/// <param name="val">The string to examine</param>
		/// <returns>A string that can be used in Oracle sql statements or
		/// null if the passed string was null/empty.</returns>
		public static string ReplaceWildCard(string val, bool leadWild, bool endWild)
		{
			if (string.IsNullOrEmpty(val)) return null;

			string s = val.Replace('*', '%');
			if( leadWild && s.StartsWith("%") == false )
				s = "%" + s;
			if (endWild && s.EndsWith("%") == false)
				s = s + "%";
			return s;
		}

		public List<T> DataTableToList<T>(DataTable dt)
			where T : ClsBaseTable, new()
		{
			List<T> lst = new List<T>();
			if( dt != null )
			{
				foreach( DataRow dr in dt.Rows )
				{
					T obj = new T();
					obj.LoadFromDataRow(dr);
					lst.Add(obj);
				}
			}
			return lst;
		}
		#endregion		// #region Data Conversion

		#region Blob/Binary methods

		public static byte[] FileToBlob(string fileName) 
		{
			byte[] blob = null;
			FileStream fs = null;
			BinaryReader br = null;
			try
			{
				fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
				br = new BinaryReader(fs);
				blob = br.ReadBytes((int)fs.Length);
			}
			finally
			{
				if( br != null ) br.Close();
				if( fs != null ) fs.Close();
			}
			return blob;
		}

		public static bool BlobToFile(string fileName, byte[] data) 
		{
			FileStream fs = null;
			BinaryWriter bw = null;
			try
			{
				fs = new FileStream(fileName, FileMode.OpenOrCreate,
					FileAccess.Write);
				bw = new BinaryWriter(fs);
				bw.Write(data);
			}
			finally
			{
				if( bw != null ) bw.Close();
				if( fs != null ) fs.Close();
			}
			return true;
		}
		#endregion		// #region Blob/Binary methods

		public static void ViewFile(string fileName)
		{
			Process proc = new Process();
			proc.StartInfo.FileName = fileName;
			proc.StartInfo.ErrorDialog = true;
			proc.StartInfo.CreateNoWindow = false;
			proc.StartInfo.UseShellExecute = true;
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;

			if( proc.Start() == true )
				if( proc.HasExited == false ) proc.WaitForExit(2000);
		}

		public static void StartProcess(string program, string folder,
			string args)
		{
			Process proc = new Process();
			proc.StartInfo.Arguments = args;
			proc.StartInfo.FileName = program;
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.ErrorDialog = false;
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			proc.StartInfo.WorkingDirectory = folder;
			proc.Start();
		}

		/// <summary>Divide an amount evenly over a number of items</summary>
		/// <param name="totalAmt">The total amount to divide</param>
		/// <param name="itemCount">The number of items to divide by</param>
		/// <param name="resultAmt">The result of the division rounded down to 2 decimal
		/// places (i.e. 23.451 and 23.459 both produce a value of 23.45).</param>
		/// <param name="extraItems">The numer of items that will get an extra 0.01 added
		/// to their results to account for any amount left over if the division resulted
		/// in a value with more than 2 decimal places. See the remarks section for more
		/// information on this value.</param>
		/// <remarks>There will be instances where the amount will not divide evenly to 2
		/// decimal places, so we have to account for the left over amount. For example,
		/// 10.01 / 3 = 3.3366... We cannot use this as our result so we truncate any
		/// digits after the 2nd decimal place, and we get 3.33 (note, we did not round up
		/// to 3.34). Now 3.33 x 3 = 9.99, and 10.01 - 9.99 = 0.02. So our resultAmt is
		/// 3.33 which would apply to all the items, and our left over amount is 0.02
		/// which would apply to some but not all of the items. In this case extraItems
		/// would return with a value of 2, meaning an extra 0.01 will apply to 2 out of
		/// the 3 items. So all items would initially be 3.33, 3.33, 3.33, but after we
		/// apply 0.01 to 2 items, we get 3.34, 3.34, 3.33.</remarks>
		public static void DivideCurrencyEvenly(decimal totalAmt, int itemCount,
			out decimal resultAmt, out int extraItems)
		{
			// Div is the actual result of the division. We will not be able to use this
			// value if it has more than 2 decimal places.
			decimal div = totalAmt / itemCount;
			// We round down by 1st multiplying by 100 which saves our 2 decimal places
			decimal div100 = div * 100M;
			// Then we truncate the fractional part
			decimal divTruncated = Decimal.Truncate(div100);
			// We finally divide by 100, which restores our 2 decimal places,
			// and gives us our result
			resultAmt = divTruncated / 100M;

			// Now we calculate any left over by first multiplying our result
			// by the number of items
			decimal subTotal = resultAmt * itemCount;
			// Subtracting this value from our total amount gives us our remainder
			decimal amtLeft = totalAmt - subTotal;
			// Multiply by 100 to get the number of items that will get an extra 0.01
			extraItems = ToInt32(amtLeft * 100M);
		}

		public static bool IsNumeric(string value)
		{
			try
			{
				Double TempDouble = Double.Parse(value);
				return true;
			}
			catch
			{
				return false;
			}
		}
		public static bool IsAlpha(string str)
		{
			char[] strArray = str.ToCharArray();
			foreach( char s in strArray )
			{
				if( s >= 'A' && s <= 'Z' )
					return true;
			}
			return false;
		}
	}
}