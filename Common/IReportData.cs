using System;
using System.Data;
using System.Collections.Generic;

namespace CS2010.Common
{
	/// <summary>Used to pass an object to a method that will generate data for a report</summary>
	public interface IReportData
	{
		/// <summary>The report's underlying data table</summary>
		DataTable Report_Data { get; set; }
		/// <summary>Parameters used to generate/filter the datatable</summary>
		Dictionary<string, object> Parameters { get; set; }
		/// <summary>Readable description of the parameters used to generate the data</summary>
		string Parameters_Dsc { get; set; }
		/// <summary>Clears the report data table (and should set it to NULL)</summary>
		void ClearReportData();
		/// <summary>Clear all parameters</summary>
		void ClearParameters();
		/// <summary>Add a parameter as a key value pair</summary>
		object AddParameter(string key, object val);
		/// <summary>Retrieve the given parameter as a string</summary>
		string GetStringParam(string key);
		/// <summary>Retrieve the given parameter as a nullable int</summary>
		int? GetIntParam(string key);
		/// <summary>Retrieve the given parameter as a nullable decimal</summary>
		decimal? GetDecimalParam(string key);
		/// <summary>Retrieve the given parameter as a nullable DateTime</summary>
		DateTime? GetDateParam(string key);
		/// <summary>Retrieve the given parameter as a DateRange object</summary>
		DateRange GetDateRangeParam(string keyFrom, string keyTo);
	}

	public enum PushType { None, Grid, Crystal, Print };
}