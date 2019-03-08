using Excel;
using System;
using System.Data;
using System.IO;
using ClosedXML.Excel;
using LandmarkIT.Enterprise.Utilities.Logging;
using System.Text;
using System.Data.OleDb;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace LandmarkIT.Enterprise.Utilities.Excel
{
    public class ReadExcel
    {
        public DataSet ToDataSet(string filePath, bool isFirstRowAsColumnNames)
        {
            try
            {
                Logger.Current.Verbose("Get Rows content for the uploaded excel file");
                var fileInfo = new FileInfo(filePath);
                var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
                var excelReader = (string.Equals(".xls", fileInfo.Extension, StringComparison.OrdinalIgnoreCase)) ? ExcelReaderFactory.CreateBinaryReader(stream) : ExcelReaderFactory.CreateOpenXmlReader(stream);
                if (!string.IsNullOrEmpty(excelReader.ExceptionMessage))
                {
                    excelReader.Close();
                    var streamData = File.Open(filePath, FileMode.Open, FileAccess.Read);
                    excelReader = ExcelReaderFactory.CreateBinaryReader(streamData);
                }
                excelReader.IsFirstRowAsColumnNames = isFirstRowAsColumnNames;
                var result = excelReader.AsDataSet();
                excelReader.Close();
                return result;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while converting excel to DataSet", ex);
                throw;
            }
        }

        /*Method to convert Data Set to Excel File*/
        public byte[] ConvertDataSetToExcel(DataTable dt, string searchCriteria)
        {
            byte[] array = null;
            try
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    /*Create the worksheet*/
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("DataSheet");
                    /*Load the datatable into the sheet, starting from cell A1. Print the column names on row 1*/
                    ws.Cells["A1"].LoadFromDataTable(dt, true);
                    /*Format the header for column 1-3*/
                    //using (ExcelRange rng = ws.Cells["A1:M1"])
                    //{
                    //    rng.Style.Font.Bold = true;
                    //    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //    rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
                    //    rng.Style.Font.Color.SetColor(Color.White);
                    //}

                    /*Example how to Format Column 1 as numeric*/
                    using (ExcelRange col = ws.Cells[2, 1, 2 + dt.Rows.Count, 1])
                    {
                        col.Style.Numberformat.Format = "#,##0.00";
                        col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    }
                    array = pck.GetAsByteArray();
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error has occured when converting the dataset to excell", ex);
            }
            return array;
        }

        public byte[] ConvertDataSetToCSV(DataTable dt, string searchCriteria)
        {
            var result = new StringBuilder();
            if (!string.IsNullOrEmpty(searchCriteria))
                result.Append("Criteria : " + searchCriteria + "\n");
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                result.Append(dt.Columns[i].ColumnName);
                result.Append(i == dt.Columns.Count - 1 ? "\n" : ",");
            }
            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    string rowValue = row[i].ToString();
                    if (!string.IsNullOrEmpty(rowValue) && rowValue.Contains(","))
                        rowValue = rowValue.Replace(",", "  ");
                    result.Append(rowValue);
                    result.Append(i == dt.Columns.Count - 1 ? "\n" : ",");
                }
            }
            byte[] bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(result.ToString());
            return bytes;
        }

        public Guid UploadDataTableToExcel(DataTable contacts, string localfilepath, Guid uniqueidentifier)
        {
            string filename = uniqueidentifier.ToString() + ".xlsx";
            string strFinalFileName = Path.Combine(localfilepath, filename);
            try
            {
                XLWorkbook wb = new XLWorkbook();
                wb.Worksheets.Add(contacts, "DataSheet");
                wb.SaveAs(strFinalFileName);
            }
            catch (Exception)
            {
                throw;
            }
            return uniqueidentifier;
        }
    }
}
