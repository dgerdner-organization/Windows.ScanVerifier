using System;

namespace CS2010.Common
{
	/// <summary>Used to format the report in the Grid Preview and the Crystal Preview</summary>
	public interface IReportFormat
	{
		/// <summary>Report title</summary>
		string Title { get; set; }

		/// <summary>How to present the report</summary>
		PushType ReportDisplayType { get; set; }

		/// <summary>Crystal report file name</summary>
		string Crystal_File_Nm { get; set; }
		/// <summary>Name of company that will appear on the report</summary>
		string Company_Nm { get; set; }
		/// <summary>Determines whether the title should be remain where it was positioned</summary>
		bool IsTitleStatic { get; set; }
		/// <summary>Determines whether the report starts out collapsed</summary>
		bool IsCrystalCollapsed { get; set; }
		/// <summary>Determines whether the Expand/Collapse button is visible</summary>
		bool IsExpandCollapseVisible { get; set; }
		/// <summary> Used to force the report to show a crystal preview when the display type is
		/// Print (i.e. to allow developers to preview before/instead of printing)</summary>
		bool PreviewCrystalBeforePrint { get; set; }

		/// <summary>Optional name of the grid layout</summary>
		string Grid_Layout_Key { get; set; }
		/// <summary>Determines whether the grid displays its table caption</summary>
		bool IsTableCaptionVisible { get; set; }
		/// <summary>Determines whether grid columns are hidden when they are grouped</summary>
		bool HideColumnsWhenGrouped { get; set; }

		/// <summary>Clear all columns</summary>
		void ClearColumns();
		/// <summary>Adds a column with various attributes</summary>
		ClsReportColumn AddColumn(ClsReportColumn rc);
		/// <summary>Adds a column with the specified attributes</summary>
		ClsReportColumn AddColumn(string colName, string colCaption, string totalType);
		/// <summary>Gets/Sets columns using the array indexer</summary>
		ClsReportColumn this[string col] { get; set; }

		/// <summary>Clear all groups</summary>
		void ClearGroups();
		/// <summary>Specify columns that will be grouped (clears all existing groups)</summary>
		void DefineGroups(params string[] args);
		/// <summary>Specify columns that will be grouped (adds to existing groups)</summary>
		void AddGroups(params string[] args);
		/// <summary>Get a string array of all columns that are grouped</summary>
		string[] GetGroups();
		/// <summary>Used to determine if a given column is grouped</summary>
		bool IsGrouped(string colName);
	}
}