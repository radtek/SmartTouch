using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using CsvHelper;
using LandmarkIT.Enterprise.Utilities.Logging;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;

namespace LandmarkIT.Enterprise.Utilities.Excel
{
    public class ReadCSV
    {
        public DataSet GetDataSetFromCSVFile(string csv_file_path)
        {
            Logger.Current.Verbose("Get Rows content for the uploaded csv file");
            DataSet csvData = new DataSet();
            OleDbConnection conn = new OleDbConnection
                   ("Provider=Microsoft.ACE.OLEDB.12.0; Data Source = " +
                     Path.GetDirectoryName(csv_file_path) +
                     "; Extended Properties = \"text;HDR=YES;\"");
            conn.Open();
            OleDbDataAdapter adapter = new OleDbDataAdapter
                    ("SELECT * FROM [" + Path.GetFileName(csv_file_path) + "]", conn);
            DataTable dtbCSV = new DataTable();
            adapter.Fill(dtbCSV);
            csvData.Tables.Add(dtbCSV);
            conn.Close();
            return csvData;
        }

        public DataSet GetDataSetFromCSVfileUsingCSVHelper(string csv_file_path)
        {
            try
            {
                Logger.Current.Verbose("Get Rows content for the uploaded csv file");
                DataSet csvdata = new DataSet();
                var dt = new DataTable();
                using (TextReader sr = new StreamReader(File.Open(csv_file_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    var csv = new CsvReader(sr);
                    var addColumns = true;
                    while (csv.Read())
                    {
                        var dr = dt.NewRow();
                        if (addColumns)
                        {
                            foreach (string header in csv.FieldHeaders)
                            {
                                addColumns = false;
                                var dc = new DataColumn(header);
                                dt.Columns.Add(dc);
                            }
                        }
                        for (int i = 0; i < csv.CurrentRecord.Length; i++)
                        {
                            dr[i] = csv.CurrentRecord[i];
                        }
                        dt.Rows.Add(dr);
                    }
                }
                csvdata.Tables.Add(dt);
                return csvdata;
            }
            catch (Exception ex) {
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                throw new UnsupportedOperationException("[|Duplicate Column Names in the Headers|]");
            }
           
        }
    }
}
