using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Diagnostics;

namespace CS2010.Common
{
	public static class ClsExtension
    {

        #region String Extension

        public static string RemoveRightChar(this string s, int numberOfChars)
        {
            return s.Remove(s.Length - numberOfChars); 
        }

        public static string RemoveComma(this string s)
		{
			StringBuilder sb = new StringBuilder(s);
			sb.Replace(",", "");
			return sb.ToString();
		}

		public static Boolean IsNull(this string s)
		{
			return string.IsNullOrEmpty(s);
		}

		public static Boolean IsNotNull(this string s)
		{
			return !IsNull(s);
		}

        public static Boolean IsInteger(this string s)
        {
            int outResult;

            try
            {
                return Int32.TryParse(s, out outResult);
            }
            catch 
            {
                return false;
            }
        }

		public static int ToInt(this string s)
		{
			return ClsConvert.ToInt32(s);
		}

		public static double ToDouble(this string s)
		{
			return ClsConvert.ToDouble(s);
		}

		public static Boolean IsNullOrWhiteSpace(this string s)
		{
			return string.IsNullOrWhiteSpace(s);
		}

		public static string NullTrim(this string s)
		{
			if (string.IsNullOrWhiteSpace(s)) return null;

			string result = s.Trim();
			return string.IsNullOrWhiteSpace(result) ? null : result;
		}

		public static string NullTrimUpper(this string s)
		{
			if (string.IsNullOrWhiteSpace(s)) return null;

			string result = s.ToUpper().Trim();
			return string.IsNullOrWhiteSpace(result) ? null : result;
		}

		public static string NullTrunc(this string s, int maxLen)
		{
			if (string.IsNullOrWhiteSpace(s)) return null;

			string result = s.NullTrim();
			if (string.IsNullOrWhiteSpace(result)) return null;

			return (result.Length > maxLen) ? result.Substring(0, maxLen) : result;
		}

		public static string NullTrimAddQuotes(this string s)
		{
			if (string.IsNullOrWhiteSpace(s)) return null;

			string strim = s.Trim();
			return string.IsNullOrWhiteSpace(strim) ? null : "'" + strim + "'";
		}

        public static DateTime ToDateYYYYMMDD(this string s)
        {
            try
            {
                string y = s.Substring(0,4);
                string m = s.Substring(4,2);
                string d = s.Substring(6,2);

                return new DateTime(y.ToInt(), m.ToInt(), d.ToInt());
            } 
            catch{
                return DateTime.Now;
            }

        }

		#endregion

		#region Date Extension

		public static string FormatDate(this DateTime dt)
		{
			return string.Format("{0:yyyy-MM-dd}", dt);
		}

		public static string FormatDateTime(this DateTime dt)
		{
			return string.Format("{0:yyyy-MM-dd hh:mm:ss tt}", dt);
		}

		public static string FormatDateTime24Hr(this DateTime dt)
		{
			return string.Format("{0:yyyy-MM-dd HH:mm:ss tt}", dt);
		}

        public static string FormatToOracleDate(this DateTime dt)
        {
            return string.Format("TO_DATE('{0:MM-dd-yyyy}','MM-DD-YYYY')", dt);
        }

		#endregion

		#region Object Extension

		public static DateTime ToDateTime(this object o)
		{
			return Convert.ToDateTime(o);
		}

        public static DateTime? ToDateTimeNullable(this object o)
        {
            if (o.IsNull()) return null;
            return o.ToDateTime();
        }

		public static double ToDouble(this object o)
		{
			return ClsConvert.ToDouble(o);
		}

		public static long ToLong( this object o)
		{
			return ClsConvert.ToInt64(o);
		}

		public static int ToInt(this object o)
		{
			return ClsConvert.ToInt32(o);
		}

		public static Decimal ToDecimal(this object o)
		{
			Debug.WriteLine(o);
			return ClsConvert.ToDecimal(o);
		}

		public static bool IsNull(this object o)
		{
			if (o == System.DBNull.Value) return true;

			return (o == null);
		}

		public static bool IsNotNull(this object o)
		{
			return !IsNull(o);
		}

		public static byte[] ToByteArray(this object o)
		{
			return ClsConvert.ToByteArray(o);
		}

		#endregion

		#region DataTable

		public static bool IsNull(this DataTable dt)
		{
			if (dt == null) return true;
			if (dt.Rows.Count < 1) return true;

			return false;
		}

		public static bool IsNotNull(this DataTable dt)
		{
			return !IsNull(dt);
		}

		#endregion
	}
}
