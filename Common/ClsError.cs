using System;
using System.Text;
using System.Collections.Generic;

namespace CS2010.Common
{
	/// <summary>Class that handles assembling errors and warnings</summary>
	/// <remarks>Potential problems can result from improper use of Reset methods. We need to make
	/// sure that we call ResetAll so that we don't get errors from previous runs, but we also need
	/// to make sure that we do not call it after its already been called because we may lose error
	/// information. Typical strategy is to call ResetAll from front end and to ensure that a
	/// method that resets erros is not called by another method that resets/adds errors</remarks>
	/// <example>
	/// bool startNewTrans = Connection.TransactionStart();
	/// try
	/// {
	///		if( !SomeMethod ) return;	// SomeMethod added an error so finally will Rollback
	///		
	///		// or
	///		
	///		ThrowIfNull(something, some message);
	/// }
	/// catch
	/// {	// Add error message to ensure that finally does a Rollback
	///		ClsError.AddError("Exception occurred while....");
	///		throw;
	/// }
	/// finally
	/// {
	///		if( startNewTrans ) Connection.TransactionEnd(!ClsError.HasErrors);
	/// }
	/// 
	/// private bool SomeMethod
	/// {
	///		if( something ) AddError("Failed");
	/// }
	/// </example>
	public static class ClsError
	{
		#region Fields

		private static StringBuilder sbError;
		private static StringBuilder sbWarning;

		private static List<ClsBaseTable> _ErrorObjects;

		#endregion		// #region Fields

		#region Properties

		public static bool HasInfo { get { return HasErrors || HasWarnings; } }
		public static bool HasErrors { get { return sbError != null && sbError.Length > 0; } }
		public static bool HasWarnings { get { return sbWarning != null && sbWarning.Length > 0; } }

		public static string AllInfo { get { return ErrorMsg + "\r\n" + WarningMsg; } }
		public static string ErrorMsg { get { return sbError != null ? sbError.ToString() : null; } }
		public static string WarningMsg { get { return sbWarning != null ? sbWarning.ToString() : null; } }

		public static List<ClsBaseTable> ErrorObjects { get { return _ErrorObjects; } }

		#endregion		// #region Properties

		#region Constructors/Initialization

		static ClsError()
		{
			sbError = new StringBuilder();
			sbWarning = new StringBuilder();
			_ErrorObjects = new List<ClsBaseTable>();
		}

		/// <summary>Resets the private field that holds the error messages</summary>
		public static void ResetErrors()
		{
			if( sbError != null ) sbError.Length = 0;
		}

		/// <summary>Resets the private field that holds the warning messages</summary>
		public static void ResetWarnings()
		{
			if( sbWarning != null ) sbWarning.Length = 0;
		}

		/// <summary>Resets the private list of base table objects</summary>
		public static void ResetObjects()
		{
			if( _ErrorObjects != null ) _ErrorObjects.Clear();
		}

		/// <summary>Resets all private fields: errors, warnings, and base table objects</summary>
		public static void ResetAll()
		{
			ResetErrors();
			ResetWarnings();
			ResetObjects();
		}
		#endregion		// #region Constructors/Initialization

		#region Adding Errors/Warnings

		/// <summary>Add an error message using the same approach as the string.Format or
		/// StringBuilder.AppendFormat methods. This method can be called without specifying
		/// the args parameter. See the .NET help for more information.</summary>
		/// <returns>Returns the formatted string</returns>
		/// <example>AddError("Error"); AddError("Error {0} Info", cnt);</example>
		public static string AddError(string fmt, params object[] args)
		{
			string msg = string.Format(fmt, args);
			sbError.AppendLine(msg);
			return msg;
		}

		/// <summary>Overloaded version of AddError that takes a base table object as the first
		/// parameter</summary>
		public static string AddError(ClsBaseTable tab, string fmt, params object[] args)
		{
			AddErrorObject(tab);
			return AddError(fmt, args);
		}

		/// <summary>Same as AddError except adds a new line to the end of the string</summary>
		public static string AddErrorLine(string fmt, params object[] args)
		{
			string msg = string.Format(fmt, args);
			sbError.AppendLine(msg);
			return msg;
		}

		/// <summary>Overloaded version of AddErrorLine that takes a base table object as the first
		/// parameter</summary>
		public static string AddErrorLine(ClsBaseTable tab, string fmt, params object[] args)
		{
			AddErrorObject(tab);
			return AddErrorLine(fmt, args);
		}

		/// <summary>Add a warning message using the same approach as the string.Format or
		/// StringBuilder.AppendFormat methods. This method can be called without specifying
		/// the args parameter. See the .NET help for more information.</summary>
		/// <returns>Returns the formatted string</returns>
		/// <example>AddWarning("Info"); AddWarning("More {0} Info", cnt);</example>
		public static string AddWarning(string fmt, params object[] args)
		{
			string msg = string.Format(fmt, args);
			sbWarning.AppendLine(msg);
			return msg;
		}

		/// <summary>Same as AddWarning except adds a new line to the end of the string</summary>
		public static string AddWarningLine(string fmt, params object[] args)
		{
			string msg = string.Format(fmt, args);
			sbWarning.AppendLine(msg);
			return msg;
		}

		/// <summary>Add a base table object. Returns -1 if the object already exists, or returns
		/// its position in the list after being added.</summary>
		public static int AddErrorObject(ClsBaseTable tab)
		{
			if( _ErrorObjects.Contains(tab) ) return -1;

			_ErrorObjects.Add(tab);
			return _ErrorObjects.Count - 1;
		}
		#endregion		// #region Adding Errors/Warnings

        #region Add Error Test Methods

        /// <summary>
        /// Tests to see if the obj is null.  If it is we add an error.
        /// </summary>
        /// <returns>Returns True if (obj == null) else false</returns>
        public static Boolean TestNullAddError(object obj, string fmt, params object[] args)
        {
            return TestAddError(obj, null, fmt, args);
        }

        /// <summary>
        /// Tests to see if the obj is null or empty; which is a specific test for strings.  
        /// If there is an error, we add it.
        /// </summary>
        /// <returns>Returns True if (obj == null or empty) else false</returns>
        public static Boolean TestNullEmptyAddError(string obj, string fmt, params object[] args)
        {
            if (string.IsNullOrEmpty(obj))
            {
                AddError(fmt, args);
                return true;
            }

            return false;
        } 

        /// <summary>
        /// Tests the parameters 'obj' and 'test' to see if they are the same.  If they are
        /// we add and error to the our ClsError object and return true.
        /// </summary>
        /// <returns>Returns True if (obj == test) else false</returns>
        public static Boolean TestAddError(object obj, object test, string fmt, params object[] args)
        {

            if (obj == test)
            {
                AddError(fmt, args);
                return true;
            }

            return false;
        }

        #endregion

        #region Throwing Exceptions

        /// <summary>Throws our ClsException if the given object is null and adds the specified
		/// error message. This method uses the same approach as the string.Format or
		/// StringBuilder.AppendFormat methods. This method can be called without specifying
		/// the args parameter. See the .NET help for more information.</summary>
		/// <example>ThrowIfNull(oa, "Info"); ThrowIfNull(oa, "Extra {0} Info", cnt);</example>
		public static void ThrowIfNull(object obj, string fmt, params object[] args)
		{
			ThrowIfNull(null, obj, fmt, args);
		}

		/// <summary>Overloaded version of ThrowIfNull that takes a base table object as the first
		/// parameter</summary>
		public static void ThrowIfNull(ClsBaseTable biz, object obj, string fmt, params object[] args)
		{
			if( obj == null )
			{
				if( biz != null ) AddErrorObject(biz);
				throw new ClsException(false, fmt, args);
			}
		}

		/// <summary>Throws our ClsException if the given numeric value is zero and adds the
		/// specified error message. This method uses the same approach as the string.Format or
		/// StringBuilder.AppendFormat methods. This method can be called without specifying
		/// the args parameter. See the .NET help for more information.</summary>
		/// <example>ThrowIfZero(seq, "Info"); ThrowIfZero(seq, "Extra {0} Info", cnt);</example>
		public static void ThrowIfZero<T>(T val, string fmt, params object[] args)
			where T : struct
		{
			ThrowIfZero<T>(null, val, fmt, args);
		}

		/// <summary>Overloaded version of ThrowIfZero that takes a base table object as the first
		/// parameter</summary>
		public static void ThrowIfZero<T>(ClsBaseTable biz, T val, string fmt, params object[] args)
			where T : struct
		{
			Type type = typeof(T);
			TypeCode dataType = Type.GetTypeCode(type);
			bool isZero = false;
			switch( dataType )
			{
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.Char:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
				case TypeCode.Double:
				case TypeCode.Single:
				case TypeCode.Decimal:
					T zv = (T)Convert.ChangeType(0, type);
					isZero = val.Equals(zv);
					break;		// break and evaluate code that checks for zero

				case TypeCode.Boolean:
				case TypeCode.DBNull:
				case TypeCode.DateTime:
				case TypeCode.Empty:
				case TypeCode.Object:
				case TypeCode.String:
				default:
					return;		// return and do not check for zero
			}

			if( isZero )
			{
				if( biz != null ) AddErrorObject(biz);
				throw new ClsException(false, fmt, args);
			}
		}
		#endregion		// #region Throwing Exceptions
	}
}