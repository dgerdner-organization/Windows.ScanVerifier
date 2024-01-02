using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Reflection;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using CS2010.Common;

namespace ExportToExcel
{
	//
	//  November 2013
	//  http://www.mikesknowledgebase.com
	//
	//  Note: if you plan to use this in an ASP.Net application, remember to add a reference to "System.Web", and to uncomment
	//  the "INCLUDE_WEB_FUNCTIONS" definition at the top of this file.
	//
	//  Release history
	//   - Nov 2013: 
	//        Changed "CreateExcelDocument(DataTable dt, string xlsxFilePath)" to remove the DataTable from the DataSet after creating the Excel file.
	//        You can now create an Excel file via a Stream (making it more ASP.Net friendly)
	//   - Jan 2013: Fix: Couldn't open .xlsx files using OLEDB  (was missing "WorkbookStylesPart" part)
	//   - Nov 2012: 
	//        List<>s with Nullable columns weren't be handled properly.
	//        If a value in a numeric column doesn't have any data, don't write anything to the Excel file (previously, it'd write a '0')
	//   - Jul 2012: Fix: Some worksheets weren't exporting their numeric data properly, causing "Excel found unreadable content in '___.xslx'" errors.
	//   - Mar 2012: Fixed issue, where Microsoft.ACE.OLEDB.12.0 wasn't able to connect to the Excel files created using this class.
	//

	public class CreateExcelFile
	{
		public static bool CreateExcelDocument<T>(List<T> list, string xlsxFilePath)
		{
			DataSet ds = new DataSet();
			ds.Tables.Add(ListToDataTable(list));

			return CreateExcelDocument(ds, xlsxFilePath);
		}
		#region HELPER_FUNCTIONS
		//  This function is adapated from: http://www.codeguru.com/forum/showthread.php?t=450171
		//  My thanks to Carl Quirion, for making it "nullable-friendly".
		public static DataTable ListToDataTable<T>(List<T> list)
		{
			DataTable dt = new DataTable();

			foreach (PropertyInfo info in typeof(T).GetProperties())
			{
				dt.Columns.Add(new DataColumn(info.Name, GetNullableType(info.PropertyType)));
			}
			foreach (T t in list)
			{
				DataRow row = dt.NewRow();
				foreach (PropertyInfo info in typeof(T).GetProperties())
				{
					if (!IsNullableType(info.PropertyType))
						row[info.Name] = info.GetValue(t, null);
					else
						row[info.Name] = (info.GetValue(t, null) ?? DBNull.Value);
				}
				dt.Rows.Add(row);
			}
			return dt;
		}
		private static Type GetNullableType(Type t)
		{
			Type returnType = t;
			if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
			{
				returnType = Nullable.GetUnderlyingType(t);
			}
			return returnType;
		}
		private static bool IsNullableType(Type type)
		{
			return (type == typeof(string) ||
					type.IsArray ||
					(type.IsGenericType &&
					 type.GetGenericTypeDefinition().Equals(typeof(Nullable<>))));
		}

		public static bool CreateExcelDocument(DataTable dt, string xlsxFilePath, ExportExcelOptions options = null)
		{
			DataSet ds = new DataSet();
			ds.Tables.Add(dt);
			try
			{
				bool result = CreateExcelDocument(ds, xlsxFilePath, options);
				return result;
			}
			finally
			{
				ds.Tables.Remove(dt);
			}
		}
		#endregion

#if INCLUDE_WEB_FUNCTIONS
		/// <summary>
		/// Create an Excel file, and write it out to a MemoryStream (rather than directly to a file)
		/// </summary>
		/// <param name="dt">DataTable containing the data to be written to the Excel.</param>
		/// <param name="filename">The filename (without a path) to call the new Excel file.</param>
		/// <param name="Response">HttpResponse of the current page.</param>
		/// <returns>True if it was created succesfully, otherwise false.</returns>
		public static bool CreateExcelDocument(DataTable dt, string filename, System.Web.HttpResponse Response)
		{
			try
			{
				DataSet ds = new DataSet();
				ds.Tables.Add(dt);
				CreateExcelDocumentAsStream(ds, filename, Response);
				ds.Tables.Remove(dt);
				return true;
			}
			catch (Exception ex)
			{
				Trace.WriteLine("Failed, exception thrown: " + ex.Message);
				return false;
			}
		}
 
		public static bool CreateExcelDocument<T>(List<T> list, string filename, System.Web.HttpResponse Response)
		{
			try
			{
				DataSet ds = new DataSet();
				ds.Tables.Add(ListToDataTable(list));
				CreateExcelDocumentAsStream(ds, filename, Response);
				return true;
			}
			catch (Exception ex)
			{
				Trace.WriteLine("Failed, exception thrown: " + ex.Message);
				return false;
			}
		}
 
		/// <summary>
		/// Create an Excel file, and write it out to a MemoryStream (rather than directly to a file)
		/// </summary>
		/// <param name="ds">DataSet containing the data to be written to the Excel.</param>
		/// <param name="filename">The filename (without a path) to call the new Excel file.</param>
		/// <param name="Response">HttpResponse of the current page.</param>
		/// <returns>Either a MemoryStream, or NULL if something goes wrong.</returns>
		public static bool CreateExcelDocumentAsStream(DataSet ds, string filename, System.Web.HttpResponse Response)
		{
			try
			{
				System.IO.MemoryStream stream = new System.IO.MemoryStream();
				using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true))
				{
					WriteExcelFile(ds, document);
				}
				stream.Flush();
				stream.Position = 0;
 
				Response.ClearContent();
				Response.Clear();
				Response.Buffer = true;
				Response.Charset = "";
 
				//  NOTE: If you get an "HttpCacheability does not exist" error on the following line, make sure you have
				//  manually added System.Web to this project's References.
 
				Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
				Response.AddHeader("content-disposition", "attachment; filename=" + filename);
				Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
				byte[] data1 = new byte[stream.Length];
				stream.Read(data1, 0, data1.Length);
				stream.Close();
				Response.BinaryWrite(data1);
				Response.Flush();
				Response.End();
 
				return true;
			}
			catch (Exception ex)
			{
				Trace.WriteLine("Failed, exception thrown: " + ex.Message);
				return false;
			}
		}
#endif      //  End of "INCLUDE_WEB_FUNCTIONS" section

		/// <summary>
		/// Create an Excel file, and write it to a file.
		/// </summary>
		/// <param name="ds">DataSet containing the data to be written to the Excel.</param>
		/// <param name="excelFilename">Name of file to be written.</param>
		/// <returns>True if successful, false if something went wrong.</returns>
		public static bool CreateExcelDocument(DataSet ds, string excelFilename, ExportExcelOptions options = null)
		{
			try
			{
				if (options == null) options = new ExportExcelOptions();
				using (SpreadsheetDocument document = SpreadsheetDocument.Create(excelFilename, SpreadsheetDocumentType.Workbook))
				{
					WriteExcelFile(ds, document, options);
				}
				Trace.WriteLine("Successfully created: " + excelFilename);
				return true;
			}
			catch (Exception ex)
			{
				Trace.WriteLine("Failed, exception thrown: " + ex.Message);
				return false;
			}
		}

		private static void WriteExcelFile(DataSet ds, SpreadsheetDocument spreadsheet,
			ExportExcelOptions options = null)
		{
			//  Create the Excel file contents.  This function is used when creating an Excel file either writing 
			//  to a file, or writing to a MemoryStream.
			spreadsheet.AddWorkbookPart();
			spreadsheet.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

			//  My thanks to James Miera for the following line of code (which prevents crashes in Excel 2010)
			spreadsheet.WorkbookPart.Workbook.Append(new BookViews(new WorkbookView()));

			//  If we don't add a "WorkbookStylesPart", OLEDB will refuse to connect to this .xlsx file !
			WorkbookStylesPart workbookStylesPart = spreadsheet.WorkbookPart.AddNewPart<WorkbookStylesPart>("rIdStyles");
			Stylesheet stylesheet = new Stylesheet();
			workbookStylesPart.Stylesheet = stylesheet;

			//  Loop through each of the DataTables in our DataSet, and create a new Excel Worksheet for each.
			uint worksheetNumber = 1;
			foreach (DataTable dt in ds.Tables)
			{
				//  For each worksheet you want to create
				string workSheetID = "rId" + worksheetNumber.ToString();
				string worksheetName = dt.TableName;

				WorksheetPart newWorksheetPart = spreadsheet.WorkbookPart.AddNewPart<WorksheetPart>();
				newWorksheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet();

				// create sheet data
				newWorksheetPart.Worksheet.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.SheetData());

				// save worksheet
				WriteDataTableToExcelWorksheet(dt, newWorksheetPart, worksheetNumber, options);
				newWorksheetPart.Worksheet.Save();

				// create the worksheet to workbook relation
				if (worksheetNumber == 1)
					spreadsheet.WorkbookPart.Workbook.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Sheets());

				spreadsheet.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>().AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Sheet()
				{
					Id = spreadsheet.WorkbookPart.GetIdOfPart(newWorksheetPart),
					SheetId = (uint)worksheetNumber,
					Name = dt.TableName
				});

				worksheetNumber++;
			}

			spreadsheet.WorkbookPart.Workbook.Save();
		}


		private static void WriteDataTableToExcelWorksheet(DataTable dt, WorksheetPart worksheetPart,
			uint tableNumber, ExportExcelOptions options = null)
		{
			var worksheet = worksheetPart.Worksheet;
			var sheetData = worksheet.GetFirstChild<SheetData>();

			string cellValue = "";

			//  Create a Header Row in our Excel file, containing one header for each Column of data in our DataTable.
			//
			//  We'll also create an array, showing which type each column of data is (Text or Numeric), so when we come to write the actual
			//  cells of data, we'll know if to write Text values or Numeric cell values.
			int numberOfColumns = dt.Columns.Count;
			bool[] IsNumericColumn = new bool[numberOfColumns];

			string[] excelColumnNames = new string[numberOfColumns];
			for (int n = 0; n < numberOfColumns; n++)
				excelColumnNames[n] = GetExcelColumnName(n);

			//
			//  Create the Header row in our Excel Worksheet
			//
			uint rowIndex = 1;

			if (options.FirstRows != null && options.FirstRows.Count > 0 &&
				(options.UseFirstRowsAllTables || tableNumber == 1))
			{
				foreach (string s in options.FirstRows.Keys)
				{
					object fval = options.FirstRows[s];
					bool isDate = false;
					string dateStr = null;
					if (fval != null)
					{
						Type ft = fval.GetType();
						isDate = (ft == typeof(DateTime));
						if (isDate)
						{
							DateTime dateVal = (DateTime)fval;
							dateStr = dateVal.ToShortDateString();
						}
					}

					var extraRow = new Row { RowIndex = rowIndex };
					sheetData.Append(extraRow);
					if (isDate)
					{
						AppendTextCell("A" + rowIndex.ToString(), s, extraRow);
						AppendTextCell("B" + rowIndex.ToString(), dateStr, extraRow);
					}
					else
					{
						string sval = (fval != null) ? fval.ToString() : null;
						AppendTextCell("A" + rowIndex.ToString(), s + " " + sval, extraRow);
					}

					++rowIndex;
				}
				++rowIndex;
			}

			var headerRow = new Row { RowIndex = rowIndex };  // add a row at the top of spreadsheet
			sheetData.Append(headerRow);

			for (int colInx = 0; colInx < numberOfColumns; colInx++)
			{
				DataColumn col = dt.Columns[colInx];
				AppendTextCell(excelColumnNames[colInx] + rowIndex.ToString(), col.ColumnName, headerRow);
				IsNumericColumn[colInx] = (col.DataType.FullName == "System.Decimal") || (col.DataType.FullName == "System.Int32");
			}

			string[] sumCols = options.GetSummaries();
			Dictionary<string, decimal> groupSums = new Dictionary<string, decimal>();
			Dictionary<string, decimal> totalSums = new Dictionary<string, decimal>();
			foreach (string s in sumCols)
			{
				DataColumn dc = dt.Columns[s];
				if (dc == null || !dc.DataType.IsValueType) continue;

				groupSums.Add(s, 0M);
				totalSums.Add(s, 0M);
			}

			//
			//  Now, step through each row of data in our DataTable...
			//
			double cellNumericValue = 0;
			DataRow drPrev = null;
			bool hasMultipleGroupVals = false;
			string[] groupCols = options.GetGroups();
			foreach (DataRow dr in dt.Rows)
			{
				foreach (string s in sumCols)
				{
					if (!dt.Columns.Contains(s)) continue;

					decimal? val = ClsConvert.ToDecimalNullable(dr[s]);
					decimal nval = totalSums[s] + val.GetValueOrDefault(0);

					totalSums[s] = nval;
				}

				// ...create a new row, and append a set of this row's data to it.
				++rowIndex;
				var newExcelRow = new Row { RowIndex = rowIndex };  // add a row at the top of spreadsheet
				sheetData.Append(newExcelRow);

				if (groupCols != null && groupCols.Length > 0)
				{
					if (drPrev != null)
					{
						bool gChange = false;
						foreach (string col in groupCols)
						{
							if (!object.Equals(drPrev[col], dr[col]))
							{
								gChange = true;
								break;
							}
						}

						if (gChange)
						{
							int gCol = 0;
							foreach (string s in sumCols)
							{
								if (!dt.Columns.Contains(s)) continue;

								decimal subVal = groupSums[s];
								AppendTextCell(excelColumnNames[gCol] + rowIndex.ToString(),
									"Sub Total " + s + ":", newExcelRow);
								AppendNumericCell(excelColumnNames[gCol+1] + rowIndex.ToString(),
									subVal.ToString(), newExcelRow);
								gCol += 2;

								groupSums[s] = 0;
							}

							hasMultipleGroupVals = true;
							++rowIndex;
							newExcelRow = new Row { RowIndex = rowIndex };  // add a row at the top of spreadsheet
							sheetData.Append(newExcelRow);
						}
					}

					foreach (string s in sumCols)
					{
						if (!dt.Columns.Contains(s)) continue;

						decimal? val = ClsConvert.ToDecimalNullable(dr[s]);
						groupSums[s] = groupSums[s] + val.GetValueOrDefault(0);
					}

					drPrev = dr;
				}

				for (int colInx = 0; colInx < numberOfColumns; colInx++)
				{
					cellValue = dr.ItemArray[colInx].ToString();

					// Create cell with data
					if (IsNumericColumn[colInx])
					{
						//  For numeric cells, make sure our input data IS a number, then write it out to the Excel file.
						//  If this numeric value is NULL, then don't write anything to the Excel file.
						cellNumericValue = 0;
						if (double.TryParse(cellValue, out cellNumericValue))
						{
							cellValue = cellNumericValue.ToString();
							AppendNumericCell(excelColumnNames[colInx] + rowIndex.ToString(), cellValue, newExcelRow);
						}
					}
					else
					{
						//  For text cells, just write the input data straight out to the Excel file.
						AppendTextCell(excelColumnNames[colInx] + rowIndex.ToString(), cellValue, newExcelRow);
					}
				}
			}

			if (groupCols != null && groupCols.Length > 0)
			{
				if (hasMultipleGroupVals)
				{
					++rowIndex;
					var lastGroupRow = new Row { RowIndex = rowIndex };
					sheetData.Append(lastGroupRow);

					int gCol = 0;
					foreach (string s in sumCols)
					{
						if (!dt.Columns.Contains(s)) continue;

						decimal subVal = groupSums[s];
						AppendTextCell(excelColumnNames[gCol] + rowIndex.ToString(),
							"Sub Total " + s + ":", lastGroupRow);
						AppendNumericCell(excelColumnNames[gCol + 1] + rowIndex.ToString(),
							subVal.ToString(), lastGroupRow);
						gCol += 2;
					}
				}
			}

			if (sumCols != null && sumCols.Length > 0)
			{
				++rowIndex;
				var totalRow = new Row { RowIndex = rowIndex };
				sheetData.Append(totalRow);

				int gCol = 1;
				foreach (string s in sumCols)
				{
					if (!dt.Columns.Contains(s)) continue;

					decimal subVal = totalSums[s];
					AppendTextCell(excelColumnNames[gCol] + rowIndex.ToString(),
						"Total " + s + ":", totalRow);
					AppendNumericCell(excelColumnNames[gCol + 1] + rowIndex.ToString(),
						subVal.ToString(), totalRow);
					gCol += 2;
				}
			}
		}

		private static void AppendTextCell(string cellReference, string cellStringValue, Row excelRow)
		{
			//  Add a new Excel Cell to our Row 
			Cell cell = new Cell() { CellReference = cellReference, DataType = CellValues.String };
			CellValue cellValue = new CellValue();
			cellValue.Text = cellStringValue;
			cell.Append(cellValue);
			excelRow.Append(cell);
		}

		private static void AppendNumericCell(string cellReference, string cellStringValue, Row excelRow)
		{
			//  Add a new Excel Cell to our Row 
			Cell cell = new Cell() { CellReference = cellReference };
			CellValue cellValue = new CellValue();
			cellValue.Text = cellStringValue;
			cell.Append(cellValue);
			excelRow.Append(cell);
		}

		private static string GetExcelColumnName(int columnIndex)
		{
			//  Convert a zero-based column index into an Excel column reference  (A, B, C.. Y, Y, AA, AB, AC... AY, AZ, B1, B2..)
			//
			//  eg  GetExcelColumnName(0) should return "A"
			//      GetExcelColumnName(1) should return "B"
			//      GetExcelColumnName(25) should return "Z"
			//      GetExcelColumnName(26) should return "AA"
			//      GetExcelColumnName(27) should return "AB"
			//      ..etc..
			//
			if (columnIndex < 26)
				return ((char)('A' + columnIndex)).ToString();

			char firstChar = (char)('A' + (columnIndex / 26) - 1);
			char secondChar = (char)('A' + (columnIndex % 26));

			return string.Format("{0}{1}", firstChar, secondChar);
		}
	}

	public class ExportExcelOptions
	{
		#region Groups

		private List<string> Groups;

		/// <summary>Clear all groups</summary>
		public void ClearGroups()
		{
			if (Groups != null) Groups.Clear();
		}

		/// <summary>Specify columns that will be grouped (clears all existing groups)</summary>
		public void DefineGroups(params string[] args)
		{
			if (args == null || args.Length <= 0)
			{
				if (Groups != null) Groups.Clear();
				Groups = null;
				return;
			}

			Groups = new List<string>(args);
		}

		/// <summary>Specify columns that will be grouped (adds to existing groups)</summary>
		public void AddGroups(params string[] args)
		{
			if (Groups == null)
			{
				DefineGroups(args);
				return;
			}

			Groups.AddRange(args);
		}

		/// <summary>Get a string array of all columns that are grouped</summary>
		public string[] GetGroups()
		{
			return Groups != null && Groups.Count > 0 ? Groups.ToArray() : new string[] { };
		}

		/// <summary>Used to determine if a given column is grouped</summary>
		public bool IsGrouped(string colName)
		{
			if (Groups == null || Groups.Count <= 0) return false;

			bool exists = Groups.Exists(delegate(string s)
			{ return string.Compare(colName, s, true) == 0; });

			return exists;
		}
		#endregion		// #region Groups

		#region Sums

		private List<string> Summaries;

		/// <summary>Clear all summaries</summary>
		public void ClearSummaries()
		{
			if (Summaries != null) Summaries.Clear();
		}

		/// <summary>Specify columns that will be summed (clears all existing summaries)</summary>
		public void DefineSummaries(params string[] args)
		{
			if (args == null || args.Length <= 0)
			{
				if (Summaries != null) Summaries.Clear();
				Summaries = null;
				return;
			}

			Summaries = new List<string>(args);
		}

		/// <summary>Specify columns that will be summed (adds to existing summaries)</summary>
		public void AddSummaries(params string[] args)
		{
			if (Summaries == null)
			{
				DefineSummaries(args);
				return;
			}

			Summaries.AddRange(args);
		}

		/// <summary>Get a string array of all columns that have sums</summary>
		public string[] GetSummaries()
		{
			return Summaries != null && Summaries.Count > 0 ? Summaries.ToArray() : new string[] { };
		}

		/// <summary>Used to determine if a given column is summarized</summary>
		public bool IsSummary(string colName)
		{
			if (Summaries == null || Summaries.Count <= 0) return false;

			bool exists = Summaries.Exists(delegate(string s)
			{ return string.Compare(colName, s, true) == 0; });

			return exists;
		}
		#endregion		// #region Summaries

		public bool UseFirstRowsAllTables { get; set; }
		public Dictionary<string, object> FirstRows { get; set; }
	}
}