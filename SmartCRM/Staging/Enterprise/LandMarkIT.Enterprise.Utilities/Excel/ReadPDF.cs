using System;
using System.Data;
using System.Data.OleDb;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace LandmarkIT.Enterprise.Utilities.Excel
{
    public class ReadPDF
    {
        public byte[] ExportToPdf(DataTable dt, string searchTitle)
        {
            Document document;
            if (dt.Columns.Count > 4)
                document = new Document(PageSize.A4.Rotate());
            else
                document = new Document();
            MemoryStream msPDFData = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, msPDFData);            
            int columns = dt.Columns.Count;
            int rows = dt.Rows.Count;
            document.Open();
            Font ColFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, Font.NORMAL);
            if (!string.IsNullOrEmpty(searchTitle))
            {
                Table searchTable = GetSearchTitle(searchTitle, ColFont);
                document.Add(searchTable);
            }
            Table pdfTable = new Table(columns, rows);
            pdfTable.BorderWidth = 1;
            pdfTable.Width = 100;
            pdfTable.Padding = 1;
            pdfTable.Spacing = 1;
            for (int i = 0; i < columns; i++)
            {
                Cell cellCols = new Cell();
                Chunk chunkCols = new Chunk(dt.Columns[i].ColumnName, ColFont);
                cellCols.Add(chunkCols);
                pdfTable.AddCell(cellCols);
            }
            Font RowFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
            for (int k = 0; k < rows; k++)
            {
                for (int j = 0; j < columns; j++)
                {
                    Cell cellRows = new Cell();
                    Chunk chunkRows = new Chunk(dt.Rows[k][j].ToString(), RowFont);
                    cellRows.Add(chunkRows);
                    pdfTable.AddCell(cellRows);
                }
                if (rows >= 1000 && k % 1000 == 0 && k != 0)
                {
                    Logging.Logger.Current.Informational("Adding  " + k + "K records");
                    document.Add(pdfTable);
                    Logging.Logger.Current.Informational("Done adding " + k + "K records");
                    pdfTable = new Table(columns, rows);
                    pdfTable.BorderWidth = 1;
                    pdfTable.Width = 100;
                    pdfTable.Padding = 1;
                    pdfTable.Spacing = 1;
                }
            }
            if (rows < 1000)
                document.Add(pdfTable);
            document.Close();
            return msPDFData.ToArray();
        }

        private Table GetSearchTitle(string searchDescription, Font font)
        {
            Table table = new Table(1, 1);
            table.BorderWidth = 1;
            table.Width = 100;
            table.Padding = 1;
            table.Spacing = 1;
            Cell description = new Cell();
            Chunk descriptionChunk = new Chunk(searchDescription, font);
            description.Add(descriptionChunk);
            table.AddCell(description);
            return table;
        }
    }
}
