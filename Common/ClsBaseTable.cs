using System;
using System.Data;
using System.Text;
using System.Reflection;
using System.Data.Common;
using System.ComponentModel;
using System.Collections.Generic;

namespace CS2010.Common
{
	/// <summary>Base class for the biz table objects</summary>
	public abstract class ClsBaseTable : INotifyPropertyChanged
	{
		#region Constructors

		/// <summary>Default constuctor</summary>
		public ClsBaseTable() { }

		/// <summary>Constructor that fills the object from the given DataRow</summary>
		/// <param name="dr"></param>
		public ClsBaseTable(DataRow dr) { LoadFromDataRow(dr); }

		#endregion		// #region Constructors

		#region Abstract methods

		public abstract int Insert();
		public abstract int Update();
		public abstract int Delete();

		public abstract void ResetColumns();

		public virtual void LoadFromDataReader(DbDataReader dr) { }
		public abstract void LoadFromDataRow(DataRow dr);
		public abstract void CopyToDataRow(DataRow dr);

		#endregion		// #region Abstract methods

		#region Virtual methods

		public virtual bool ReloadFromDB() { return false; }
		protected virtual void OnReload() { }

		public virtual void ResetFKs() { }
		public virtual void SetDefaults() { }
		public virtual bool ValidateInsert() { return true; }
		public virtual bool ValidateUpdate() { return true; }
		public virtual bool ValidateDelete() { return true; }

		#endregion		// #region Virtual methods

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void NotifyPropertyChanged(String info)
		{
			if( PropertyChanged != null )
				PropertyChanged(this, new PropertyChangedEventArgs(info));
		}
		#endregion

		#region Change Tracking (IsDirty)

		/// <summary>Storage for <see cref="IsDirty"/> property</summary>
		protected bool _IsDirty;

		/// <summary>Gets whether changes have been made to this object</summary>
		public bool IsDirty { get { return _IsDirty; } set { _IsDirty = value; } }

		#endregion		#region Change Tracking (IsDirty)

		#region Error/Warning Members

		/// <summary>Used to store error information for this object. This field can also be
		/// accessed through the indexer, e.g. this[columnName].</summary>
		protected Dictionary<string, string> _Errors =
			new Dictionary<string, string>();

		/// <summary>Used to warning information for this object. This field can also be
		/// accessed through the AddWarning method.</summary>
		protected StringBuilder sbWarnings = new StringBuilder();

		/// <summary>Returns true if there are errors messages in the object</summary>
		public bool HasErrors { get { return _Errors.Count > 0; } }
		/// <summary>Clear all errors associated with the object</summary>
		public void ResetErrors() { _Errors.Clear(); }

		/// <summary>Returns true if there are warning messages in the object</summary>
		public bool HasWarnings { get { return sbWarnings.Length > 0; } }
		/// <summary>Clear all warnings associated with the object</summary>
		public void ResetWarnings() { sbWarnings.Length = 0; }

		public void FillError(DataRow dr)
		{
			foreach (string colName in _Errors.Keys)
			{
				if( dr.Table.Columns.Contains(colName) )
					dr.SetColumnError(colName, _Errors[colName]);
			}
		}

		/// <summary>Returns any error information associated with the object</summary>
		public string Error
		{
			get
			{
				if( _Errors.Count <= 0 ) return string.Empty;

				StringBuilder sb = new StringBuilder();
				foreach( string s in _Errors.Keys ) sb.AppendLine(_Errors[s]);

				return sb.ToString();
			}
		}

		public string this[string columnName]
		{
			get
			{
				return ( _Errors.Count <= 0 ||
					_Errors.ContainsKey(columnName) == false )
					? string.Empty : _Errors[columnName];
			}
			set
			{
				_Errors[columnName] = value;
			}
		}

		/// <summary>Returns any error information associated with the object</summary>
		public string Warning
		{
			get { return sbWarnings.ToString(); }
		}

		public void AddWarning(string fmt, params object[] args)
		{
			sbWarnings.AppendFormat(fmt, args);
		}
		#endregion

		#region Helper Methods

		protected string GetAddress(bool trimBlankLines)
		{
			return GetAddress(trimBlankLines, true);
		}

		protected string GetAddress(bool trimBlankLines, bool countryOnNewLine)
		{
			IAddress ia = this as IAddress;
			if (ia == null) return string.Empty;

			Address a = new Address(ia);
			return (trimBlankLines == true)
				? a.FormatAddress(countryOnNewLine) : a.GetAddressBox(countryOnNewLine);
		}
		#endregion		// #region Helper Methods
	}

	#region ComboItem

	/// <summary>A class that can be used to create combo boxes that are not
	/// bound to a database table</summary>
	[Serializable]
	public class ComboItem : INotifyPropertyChanged
	{
		#region Fields

		private string _Code;				// Code property
		private string _Description;		// Description property

		#endregion		// #region Fields

		#region Properties

		/// <summary>Gets/Sets the code value for this item</summary>
		public string Code
		{
			get { return _Code; }
			set
			{
				string val = string.IsNullOrEmpty(value) ? null : value.Trim();

				if( string.IsNullOrEmpty(val) ) val = null;

				if( string.Compare(_Code, val, false) == 0 ) return;

				_Code = val;

				NotifyPropertyChanged("Code");
			}
		}

		/// <summary>Gets/Sets the description value for this item</summary>
		public string Description
		{
			get { return _Description; }
			set
			{
				string val = string.IsNullOrEmpty(value) ? null : value.Trim();

				if( string.IsNullOrEmpty(val) ) val = null;

				if( string.Compare(_Description, val, false) == 0 ) return;

				_Description = val;

				NotifyPropertyChanged("Description");
			}
		}

		/// <summary>Gets the code followed by the description</summary>
		public string CodeDescription
		{
			get { return string.Format("{0} - {1}", Code, Description); }
		}

		/// <summary>Gets the description followed by the code</summary>
		public string DescriptionCode
		{
			get { return string.Format("{0} - {1}", Description, Code); }
		}
		#endregion		// #region Properties

		#region Constructors

		/// <summary>Default constructor (sets all properties to null)</summary>
		public ComboItem()
		{
			_Code = null;
			_Description = null;
		}

		/// <summary>Constructor expecting values for both properties</summary>
		/// <param name="cd">The code value</param>
		/// <param name="dsc">The description value</param>
		public ComboItem(string cd, string dsc)
		{
			_Code = cd;
			_Description = dsc;
		}
		#endregion		// #region Constructors

		#region Overrides

		/// <summary>Override of the ToString() method, returns the code value
		/// followed by the description value</summary>
		/// <returns>Code value followed by the description value</returns>
		public override string ToString()
		{
			return CodeDescription;
		}
		#endregion		// #region Overrides

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void NotifyPropertyChanged(String propertyName)
		{
			if( PropertyChanged != null )
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}
	#endregion		// #region ComboItem
}