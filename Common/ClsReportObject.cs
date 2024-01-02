using System;
using System.Data;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using CS2010.Common;

namespace CS2010.Common
{
	#region ClsReportObject

	public class ClsReportObject : IEnumerable<ClsReportColumn>, IReportData, IReportFormat
	{
		#region Constructors/Initialization

		public ClsReportObject()
		{
			Reset();
		}

		private ClsLinkHandler _LinkHandler;
		public ClsLinkHandler LinkHandler
		{
			get
			{ return _LinkHandler; }
			set
			{ _LinkHandler = value; }
		}

		public void Reset()
		{
			_Title = null;
			_ReportDisplayType = PushType.Grid;

			_Crystal_File_Nm = _Company_Nm = null;

			_IsTitleStatic = _IsCrystalCollapsed = _PreviewCrystalBeforePrint = false;
			_IsExpandCollapseVisible = true;

			_Grid_Layout_Key = null;
			_IsTableCaptionVisible = false;
			_HideColumnsWhenGrouped = true;

			ClearColumns();
			ClearGroups();
			Groups = null;

			ClearReportData();
			ClearParameters();
			_Parameters = null;
			_Parameters_Dsc = null;
		}
		#endregion		// #region Constructors/Initialization

		#region IReportData Members

		private DataTable _Report_Data;
		/// <summary>The report's underlying data table</summary>
		public DataTable Report_Data
		{
			get { return _Report_Data; }
			set { _Report_Data = value; }
		}

		private Dictionary<string, object> _Parameters;
		/// <summary>Parameters used to generate/filter the datatable</summary>
		public Dictionary<string, object> Parameters
		{
			get { return _Parameters; }
			set { _Parameters = value; }
		}

		private string _Parameters_Dsc;
		/// <summary>Readable description of the parameters used to generate the data</summary>
		public string Parameters_Dsc
		{
			get { return string.IsNullOrEmpty(_Parameters_Dsc) ? "ALL" : _Parameters_Dsc; }
			set { _Parameters_Dsc = value; }
		}

		/// <summary>Clears the report data table (and sets it to NULL)</summary>
		public void ClearReportData()
		{
			if( _Report_Data != null ) _Report_Data.Dispose();
			_Report_Data = null;
		}

		/// <summary>Clear all parameters</summary>
		public void ClearParameters()
		{
			if( _Parameters != null ) _Parameters.Clear();
			_Parameters_Dsc = null;	// also clear the parameter description
		}

		/// <summary>Add a parameter as a key value pair</summary>
		public object AddParameter(string key, object val)
		{
			if( _Parameters == null ) _Parameters = new Dictionary<string, object>();

			object ret = null;
			if( _Parameters.ContainsKey(key) )
			{
				ret = _Parameters[key];
				_Parameters[key] = val;
			}
			else
				_Parameters.Add(key, val);

			return ret;
		}

		/// <summary>Retrieve the given parameter as a string</summary>
		public string GetStringParam(string key)
		{
			if( key != null ) key = key.ToUpper();
			return Parameters.ContainsKey(key) ? Parameters[key] as string : null;
		}

		/// <summary>Retrieve the given parameter as a nullable int</summary>
		public int? GetIntParam(string key)
		{
			if( key != null ) key = key.ToUpper();
			string s = Parameters.ContainsKey(key) ? Parameters[key] as string : null;
			return string.IsNullOrEmpty(s) ? null : ClsConvert.ToInt32Nullable(s);
		}

		/// <summary>Retrieve the given parameter as a nullable decimal</summary>
		public decimal? GetDecimalParam(string key)
		{
			if( key != null ) key = key.ToUpper();
			string s = Parameters.ContainsKey(key) ? Parameters[key] as string : null;
			return string.IsNullOrEmpty(s) ? null : ClsConvert.ToDecimalNullable(s);
		}

		/// <summary>Retrieve the given parameter as a nullable DateTime</summary>
		public DateTime? GetDateParam(string key)
		{
			if( key != null ) key = key.ToUpper();
			string s = Parameters.ContainsKey(key) ? Parameters[key] as string : null;
			return string.IsNullOrEmpty(s) ? null : ClsConvert.ToDateTimeNullable(s);
		}

		/// <summary>Retrieve the given parameter as a DateRange object</summary>
		public DateRange GetDateRangeParam(string keyFrom, string keyTo)
		{
			if( keyFrom != null ) keyFrom = keyFrom.ToUpper();
			if( keyTo != null ) keyTo = keyTo.ToUpper();
			string fr = Parameters.ContainsKey(keyFrom) ? Parameters[keyFrom] as string : null;
			string to = Parameters.ContainsKey(keyTo) ? Parameters[keyTo] as string : null;

			DateTime? f = string.IsNullOrEmpty(fr) ? null : ClsConvert.ToDateTimeNullable(fr);
			DateTime? t = string.IsNullOrEmpty(to) ? null : ClsConvert.ToDateTimeNullable(to);
			return new DateRange(f, t);
		}
		#endregion		// #region IReportData Members

		#region IReportFormat Members

		private string _Title;
		/// <summary>Report title</summary>
		public string Title
		{
			get { return _Title; }
			set { _Title = value; }
		}

		private PushType _ReportDisplayType;
		/// <summary>How to present the report (See PushType enum)</summary>
		public PushType ReportDisplayType
		{
			get { return _ReportDisplayType; }
			set { _ReportDisplayType = value; }
		}

		private string _Crystal_File_Nm;
		/// <summary>Crystal report file name</summary>
		public string Crystal_File_Nm
		{
			get { return _Crystal_File_Nm; }
			set { _Crystal_File_Nm = value; }
		}

		private string _Company_Nm;
		/// <summary>Name of company that will appear on the report</summary>
		public string Company_Nm
		{
			get { return _Company_Nm; }
			set { _Company_Nm = value; }
		}

		private bool _IsTitleStatic;
		/// <summary>Determines whether the title should be remain where it was positioned in
		/// the crystal report designer (false if it should be moved to center)</summary>
		public bool IsTitleStatic
		{
			get { return _IsTitleStatic; }
			set { _IsTitleStatic = value; }
		}

		private bool _IsCrystalCollapsed;
		/// <summary>Determines whether the report starts out collapsed</summary>
		public bool IsCrystalCollapsed
		{
			get { return _IsCrystalCollapsed; }
			set { _IsCrystalCollapsed = value; }
		}

		private bool _IsExpandCollapseVisible;
		/// <summary>Determines whether the Expand/Collapse button is visible</summary>
		public bool IsExpandCollapseVisible
		{
			get { return _IsExpandCollapseVisible; }
			set { _IsExpandCollapseVisible = value; }
		}

		private bool _PreviewCrystalBeforePrint;
		/// <summary> Used to force the report to show a crystal preview when the display type is
		/// Print (i.e. to allow developers to preview before/instead of printing)</summary>
		public bool PreviewCrystalBeforePrint
		{
			get { return _PreviewCrystalBeforePrint; }
			set { _PreviewCrystalBeforePrint = value; }
		}

		private string _Grid_Layout_Key;
		/// <summary>Optional name of the grid layout that should be applied to the grid</summary>
		public string Grid_Layout_Key
		{
			get { return _Grid_Layout_Key; }
			set { _Grid_Layout_Key = value; }
		}

		private bool _IsTableCaptionVisible;
		/// <summary>Determines whether the grid displays its table caption</summary>
		public bool IsTableCaptionVisible
		{
			get { return _IsTableCaptionVisible; }
			set { _IsTableCaptionVisible = value; }
		}

		private bool _HideColumnsWhenGrouped;
		/// <summary>Determines whether grid columns are hidden when they are grouped</summary>
		public bool HideColumnsWhenGrouped
		{
			get { return _HideColumnsWhenGrouped; }
			set { _HideColumnsWhenGrouped = value; }
		}

		#region Columns

		private Dictionary<string, ClsReportColumn> Columns;

		/// <summary>Clear all columns</summary>
		public void ClearColumns()
		{
			if( Columns != null ) Columns.Clear();
		}

		/// <summary>Adds a column with various attributes</summary>
		public ClsReportColumn AddColumn(ClsReportColumn rc)
		{
			if( Columns == null )
				Columns = new Dictionary<string, ClsReportColumn>
					(StringComparer.InvariantCultureIgnoreCase);

			if( Columns.ContainsKey(rc.ColumnName) )
				Columns[rc.ColumnName] = rc;
			else
				Columns.Add(rc.ColumnName, rc);

			return rc;
		}

		/// <summary>Adds a column with the specified attributes</summary>
		public ClsReportColumn AddColumn(string colName, string colCaption, string totalType)
		{
			return AddColumn(new ClsReportColumn(colName, colCaption, totalType));
		}

		/// <summary>Gets/Sets columns using array indexer (i.e. Options["ORDER_ID"])</summary>
		public ClsReportColumn this[string col]
		{
			get { return (Columns != null && Columns.ContainsKey(col)) ? Columns[col] : null; }
			set { AddColumn(value); }
		}
		#endregion		// #region Columns

		#region Groups

		private List<string> Groups;

		/// <summary>Clear all groups</summary>
		public void ClearGroups()
		{
			if( Groups != null ) Groups.Clear();
		}

		/// <summary>Specify columns that will be grouped (clears all existing groups)</summary>
		public void DefineGroups(params string[] args)
		{
			if( args == null || args.Length <= 0 )
			{
				if( Groups != null ) Groups.Clear();
				Groups = null;
				return;
			}

			Groups = new List<string>(args);
			foreach( string col in args )
			{
				if( Columns != null && Columns.ContainsKey(col) ) continue;
				AddColumn(col, col, null);
			}
		}

		/// <summary>Specify columns that will be grouped (adds to existing groups)</summary>
		public void AddGroups(params string[] args)
		{
			if( Groups == null )
			{
				DefineGroups(args);
				return;
			}

			Groups.AddRange(args);
			foreach( string col in args )
			{
				if( Columns != null && Columns.ContainsKey(col) ) continue;
				AddColumn(col, col, null);
			}
		}

		/// <summary>Get a string array of all columns that are grouped</summary>
		public string[] GetGroups()
		{
			return Groups != null && Groups.Count > 0 ? Groups.ToArray() : new string[] { };
		}

		/// <summary>Used to determine if a given column is grouped</summary>
		public bool IsGrouped(string colName)
		{
			if( Groups == null || Groups.Count <= 0 ) return false;

			bool exists = Groups.Exists(delegate(string s)
			{ return string.Compare(colName, s, true) == 0; });

			return exists;
		}
		#endregion		// #region Groups

		#region FormatConditions

		private List<ClsFormatCondition> FormatConditions;

		/// <summary>Clear all columns</summary>
		public void ClearFormatConditions()
		{
			if (FormatConditions != null) FormatConditions.Clear();
		}

		/// <summary>Add a conditional format comparing 2 columns</summary>
		public ClsFormatCondition Add2ColCondition(string leftCol, string conditionalOp,
			string rightCol, bool isBold, bool isItalic)
		{
			if (FormatConditions == null)
				FormatConditions = new List<ClsFormatCondition>();

			ClsFormatCondition fc = new ClsFormatCondition(leftCol, conditionalOp, rightCol,
				isBold, isItalic);
			FormatConditions.Add(fc);

			return fc;
		}

		/// <summary>Add a conditional format comparing 1 column with a specific value</summary>
		public ClsFormatCondition AddSingleColCondition(string leftCol, string conditionalOp,
			object rightVal, bool isBold, bool isItalic)
		{
			if (FormatConditions == null)
				FormatConditions = new List<ClsFormatCondition>();

			ClsFormatCondition fc = new ClsFormatCondition(leftCol, conditionalOp, rightVal,
				isBold, isItalic);
			FormatConditions.Add(fc);

			return fc;
		}

		public ClsFormatCondition[] GetConditions()
		{
			return (FormatConditions != null) ? FormatConditions.ToArray() : null;
		}
		#endregion		// #region Format Condition

		#endregion		// #region IReportFormat Members

		#region IEnumerable/IEnumerable<ClsReportColumn> Members

		public IEnumerator<ClsReportColumn> GetEnumerator()
		{
			return new ClsReportColumnEnum(Columns);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new ClsReportColumnEnum(Columns);
		}

		#region ClsReportColumnEnum

		public class ClsReportColumnEnum : IEnumerator<ClsReportColumn>
		{
			#region Fields

			private int Position = -1;
			private ClsReportColumn[] _Columns;

			#endregion		// #region Fields

			#region Constructors

			public ClsReportColumnEnum(Dictionary<string, ClsReportColumn> aList)
			{
				int i = 0;
				_Columns = new ClsReportColumn[aList.Count];
				foreach( ClsReportColumn ca in aList.Values ) _Columns[i++] = ca;
			}
			#endregion		// #region Constructors

			#region Helper Methods

			private ClsReportColumn GetCurrent()
			{
				try
				{
					return _Columns[Position];
				}
				catch( IndexOutOfRangeException )
				{
					throw new InvalidOperationException();
				}
			}
			#endregion		// #region Helper Methods

			#region IEnumerator/IEnumerator<ClsReportColumn> Members

			public ClsReportColumn Current { get { return GetCurrent(); } }
			object IEnumerator.Current { get { return GetCurrent(); } }

			public bool MoveNext()
			{
				Position++;
				return Position < _Columns.Length;
			}

			public void Reset()
			{
				Position = -1;
			}
			#endregion		// #region IEnumerator/IEnumerator<ClsReportColumn> Members

			#region IDisposable Members

			public void Dispose()
			{
				Position = -1;
				_Columns = null;
			}
			#endregion		// #region IDisposable Members
		}
		#endregion		// #region ClsReportColumnEnum

		#endregion		// #region IEnumerable/IEnumerable<ClsReportColumn> Members
	}
	#endregion		// #region ClsReportOptions

	#region Janus Format Conditions

	public class ClsFormatCondition
	{
		public string LeftColumn { get; set; }
		public string ConditionalOperator { get; set; }
		public string RightColumn { get; set; }
		public object RightValue { get; set; }
		public bool UseValue { get; set; }
		public bool ApplyToRightCol { get; set; }

		public Color? BackColor { get; set; }
		public Color? ForeColor { get; set; }
		public bool IsBold { get; set; }
		public bool IsItalic { get; set; }

		private ClsFormatCondition(string lcol, string condOp, bool useBold, bool useItalic)
		{
			LeftColumn = lcol;
			ConditionalOperator = condOp;
			IsBold = useBold;
			IsItalic = useItalic;
		}

		public ClsFormatCondition(string lcol, string condOp, string rcol, bool useBold, bool useItalic)
			: this(lcol, condOp, useBold, useItalic)
		{
			RightColumn = rcol;
			ApplyToRightCol = true;
			RightValue = null;
			UseValue = false;
		}

		public ClsFormatCondition(string lcol, string condOp, object rVal, bool useBold, bool useItalic)
			: this(lcol, condOp, useBold, useItalic)
		{
			RightValue = rVal;
			UseValue = true;
			RightColumn = null;
			ApplyToRightCol = false;
		}
	}
	#endregion		// #region Janus Format Conditions

	#region ClsReportColumn

	public class ClsReportColumn
	{
		#region Fields/Properties

		private string _ColumnName;
		public string ColumnName { get { return _ColumnName; } }

		private string _ColumnCaption;
		public string ColumnCaption
		{
			get { return _ColumnCaption; }
			set { _ColumnCaption = value; }
		}

		private string _AggregateType;
		public string AggregateType
		{
			get { return string.IsNullOrEmpty(_AggregateType) ? "None" : _AggregateType; }
			set { _AggregateType = value; }
		}

		private string _HeaderAlignment;
		public string HeaderAlignment
		{
			get { return string.IsNullOrEmpty(_HeaderAlignment) ? "Empty" : _HeaderAlignment; }
			set { _HeaderAlignment = value; }
		}

		private string _ColumnType;
		public string ColumnType
		{
			get {return string.IsNullOrEmpty(_ColumnType) ? "Text" : _ColumnType;}
			set { _ColumnType = value; }
		}

		public object CheckboxFalseValue { get; set; }
		public object CheckboxTrueValue { get; set; }

		private string _TextAlignment;
		public string TextAlignment
		{
			get { return string.IsNullOrEmpty(_TextAlignment) ? "Empty" : _TextAlignment; }
			set { _TextAlignment = value; }
		}

		private string _FormatString;
		public string FormatString
		{
			get { return _FormatString; }
			set { _FormatString = value; }
		}

		private string _NullText;
		public string NullText
		{
			get { return _NullText; }
			set { _NullText = value; }
		}

		private bool _CollapseWhenGrouped;
		public bool CollapseWhenGrouped
		{
			get { return _CollapseWhenGrouped; }
			set { _CollapseWhenGrouped = value; }
		}

		private bool _HideWhenGrouped;
		public bool HideWhenGrouped
		{
			get { return _HideWhenGrouped; }
			set { _HideWhenGrouped = value; }
		}

		private bool _ExcludeColumn;
		public bool ExcludeColumn
		{
			get { return _ExcludeColumn; }
			set { _ExcludeColumn = value; }
		}

		private bool _HideColumn;
		public bool HideColumn
		{
			get { return _HideColumn; }
			set { _HideColumn = value; }
		}

		private string _GroupInterval;
		public string GroupInterval
		{
			get { return _GroupInterval; }
			set { _GroupInterval = value; }
		}
		#endregion		// #region Fields/Properties

		#region Constructors/Initialization

		public ClsReportColumn(string col)
		{
			Reset();

			string tmp = col != null ? col.Trim() : null;
			if( string.IsNullOrEmpty(tmp) )
				throw new Exception("Blank column name was specified");

			_ColumnName = tmp;
			ColumnCaption = tmp;

			if( tmp.EndsWith("Amt", StringComparison.InvariantCultureIgnoreCase) ||
				tmp.EndsWith("Amount", StringComparison.InvariantCultureIgnoreCase) )
				FormatString = "c";
			else if( tmp.EndsWith("Cnt", StringComparison.InvariantCultureIgnoreCase) ||
				tmp.EndsWith("Count", StringComparison.InvariantCultureIgnoreCase) )
				FormatString = "0";
			else if( tmp.EndsWith("Dt", StringComparison.InvariantCultureIgnoreCase) ||
				tmp.EndsWith("Date", StringComparison.InvariantCultureIgnoreCase) )
				FormatString = ClsConfig.DateFormat;
		}

		public ClsReportColumn(string col, string caption)
			: this(col)
		{
			if( !string.IsNullOrEmpty(caption) ) ColumnCaption = caption;
		}

		public ClsReportColumn(string col, string caption, string totalType)
			: this(col, caption)
		{
			AggregateType = totalType;
		}

		public ClsReportColumn(string col, string caption, string totalType, string format,
			string txtAlign, string hdrAlign, string txtNull, string columnType)
			: this(col, caption, totalType)
		{
			SetFormat(format, txtAlign, hdrAlign, txtNull, columnType);
		}

		public ClsReportColumn(string col, string caption, string totalType, bool hideCol,
			bool excludeCol, bool collapseGrp, bool hideIfGrouped)
			: this(col, caption, totalType)
		{
			SetVisibility(hideCol, excludeCol, collapseGrp, hideIfGrouped);
		}

		public ClsReportColumn(string col, string caption, string totalType, string format,
			string txtAlign, string hdrAlign, string txtNull, bool hideCol, bool excludeCol,
			bool collapseGrp, bool hideIfGrouped, string columnType)
			: this(col, caption, totalType)
		{
			SetFormat(format, txtAlign, hdrAlign, txtNull, columnType);
			SetVisibility(hideCol, excludeCol, collapseGrp, hideIfGrouped);
		}

		public void Reset()
		{
			_ColumnCaption = null;

			_AggregateType = null;

			_HeaderAlignment = "Empty";
			_TextAlignment = "Empty";
			_FormatString = _NullText = null;
			_ColumnType = "Text";
			CheckboxFalseValue = "N";
			CheckboxTrueValue = "Y";

			_HideWhenGrouped = true;
			_CollapseWhenGrouped = _ExcludeColumn = _HideColumn = false;

			_GroupInterval = null;
		}
		#endregion		// #region Constructors/Initialization

		#region Public Methods

		public void SetVisibility(bool hideCol, bool excludeCol, bool collapseGrp, bool hideIfGrouped)
		{
			CollapseWhenGrouped = collapseGrp;
			HideWhenGrouped = hideIfGrouped;
			ExcludeColumn = excludeCol;
			HideColumn = hideCol;
		}
		public void SetFormat(string format, string txtAlign, string hdrAlign, string txtNull, string columnType)
		{
			HeaderAlignment = hdrAlign;
			TextAlignment = txtAlign;
			FormatString = format;
			NullText = txtNull;
			ColumnType = columnType;
		}
		#endregion		// #region Public Methods
	}
	#endregion		// #region ClsReportColumn
}