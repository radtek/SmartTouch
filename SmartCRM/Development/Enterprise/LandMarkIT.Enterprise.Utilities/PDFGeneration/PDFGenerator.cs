using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Data.OleDb;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Threading.Tasks;
using LandmarkIT.Enterprise.Utilities.Logging;

namespace LandmarkIT.Enterprise.Utilities.PDFGeneration
{
    public class PDFGenerator
    {
        public byte[] GenerateFormSubmissionPDF(Dictionary<string, Dictionary<string, string>> data, string accountName, string formName, string accLogoURL, int categoryID = 0)
        {
            Document document = new Document();
            MemoryStream msPDFData = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, msPDFData);
            int columns = 3;
            int rows = data.Count;
            document.Open();
            document.AddTitle("Form Submission Details: ");

            PdfPTable pdfTable = new PdfPTable(columns);
            pdfTable.TotalWidth = 98;

            if (!string.IsNullOrEmpty(accountName))
                document.Add(new Paragraph("Account Name: " + accountName));
            if (!string.IsNullOrEmpty(formName))
                document.Add(new Paragraph("Form Name: " + formName));
            if (!string.IsNullOrEmpty(accLogoURL))
            {
                Image jpg = Image.GetInstance(new Uri(accLogoURL));
                jpg.ScalePercent(24f);
                jpg.SetAbsolutePosition(document.PageSize.Width - 36f - 70f, document.PageSize.Height - 36f - 50f);
                document.Add(jpg);
            }
            try
            {
                PdfPCell[] cells = GetFormColumns();
                for (int i = 0; i < columns; i++)
                {
                    pdfTable.AddCell(cells[i]);
                }
                Font RowFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
                List<string> propertyNames = data.Keys.ToList();
                foreach (var prop in propertyNames)
                {
                    Dictionary<string, string> formValues = new Dictionary<string, string>();
                    data.TryGetValue(prop, out formValues);
                    if (formValues != null && formValues.Count > 0)
                    {
                        PdfPCell cell = new PdfPCell();
                        Chunk chuck = new Chunk(prop, RowFont);
                        cell.Phrase = new Phrase(chuck);
                        pdfTable.AddCell(cell);

                        cell = new PdfPCell();
                        chuck = new Chunk(formValues.ElementAt(0).Value, RowFont);
                        cell.Phrase = new Phrase(chuck);
                        pdfTable.AddCell(cell);

                        cell = new PdfPCell();
                        chuck = new Chunk(formValues.ElementAt(1).Value, RowFont);
                        cell.Phrase = new Phrase(chuck);
                        pdfTable.AddCell(cell);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error while generating PDF for form submission", ex);
            }
            PdfPTable outerTable = new PdfPTable(1);
            PdfPCell outerCell = new PdfPCell(pdfTable);
            outerCell.Padding = 2;
            outerTable.SpacingAfter = 20f;
            outerTable.SpacingBefore = 30f;
            outerTable.HorizontalAlignment = 0;
            outerTable.WidthPercentage = 100f;
            outerTable.AddCell(outerCell);

            document.Add(outerTable);
            document.Close();

            return msPDFData.ToArray();
        }

        private PdfPCell[] GetFormColumns()
        {
            PdfPCell[] cells = new PdfPCell[3];
            Font ColFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, Font.NORMAL);
            PdfPCell cellCols = new PdfPCell();
            cellCols.BackgroundColor = Color.LIGHT_GRAY;

            Chunk chunkCols = new Chunk("Field Name", ColFont);
            cellCols.Phrase = new Phrase(chunkCols);
            cells[0] = cellCols;

            cellCols = new PdfPCell();
            cellCols.BackgroundColor = Color.LIGHT_GRAY;
            chunkCols = new Chunk("Old Value", ColFont);
            cellCols.Phrase = new Phrase(chunkCols);
            cells[1] = cellCols;

            cellCols = new PdfPCell();
            cellCols.BackgroundColor = Color.LIGHT_GRAY;
            chunkCols = new Chunk("New Value", ColFont);
            cellCols.Phrase = new Phrase(chunkCols);
            cells[2] = cellCols;

            return cells;
        }

        public byte[] GetNotificationPDF(ILookup<string, string> data, string accountName, string formName, string accLogoURL, int categoryID = 0)
        {
            Document document = new Document();
            MemoryStream msPDFData = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, msPDFData);
            int columns = 2;
            int rows = data.Count;
            document.Open();
            document.AddTitle("Notification Details: ");

            PdfPTable pdfTable = new PdfPTable(columns);
            pdfTable.TotalWidth = 98;

            if (!string.IsNullOrEmpty(accountName))
                document.Add(new Paragraph("Account Name: " + accountName));
            if (!string.IsNullOrEmpty(formName))
                document.Add(new Paragraph("Form Name: " + formName));
            if (!string.IsNullOrEmpty(accLogoURL))
            {
                Image jpg = Image.GetInstance(new Uri(accLogoURL));
                jpg.ScalePercent(24f);
                jpg.SetAbsolutePosition(document.PageSize.Width - 36f - 70f, document.PageSize.Height - 36f - 50f);
                document.Add(jpg);
            }
            try
            {
                PdfPCell[] cells = GetNotificationColumns();
                for (int i = 0; i < columns; i++)
                {
                    pdfTable.AddCell(cells[i]);
                }
                Font RowFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
                List<string> propertyNames = data.Select(s => s.Key).ToList();
                foreach (var prop in propertyNames)
                {
                    string formValues = data[prop].FirstOrDefault();
                    if (!string.IsNullOrEmpty(formValues))
                    {
                        PdfPCell cell = new PdfPCell();
                        Chunk chuck = new Chunk(prop, RowFont);
                        cell.Phrase = new Phrase(chuck);
                        pdfTable.AddCell(cell);

                        cell = new PdfPCell();
                        chuck = new Chunk(formValues, RowFont);
                        cell.Phrase = new Phrase(chuck);
                        pdfTable.AddCell(cell);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error while generating PDF for workflow notification", ex);
            }
            PdfPTable outerTable = new PdfPTable(1);
            PdfPCell outerCell = new PdfPCell(pdfTable);
            outerCell.Padding = 2;
            outerTable.SpacingAfter = 20f;
            outerTable.SpacingBefore = 30f;
            outerTable.HorizontalAlignment = 0;
            outerTable.WidthPercentage = 100f;
            outerTable.AddCell(outerCell);
            document.Close();

            return msPDFData.ToArray();
        }

        private PdfPCell[] GetNotificationColumns()
        {
            PdfPCell[] cells = new PdfPCell[2];
            Font ColFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, Font.NORMAL);
            PdfPCell cellCols = new PdfPCell();
            cellCols.BackgroundColor = Color.LIGHT_GRAY;

            Chunk chunkCols = new Chunk("Field Name", ColFont);
            cellCols.Phrase = new Phrase(chunkCols);
            cells[0] = cellCols;

            cellCols = new PdfPCell();
            cellCols.BackgroundColor = Color.LIGHT_GRAY;
            chunkCols = new Chunk("Value", ColFont);
            cellCols.Phrase = new Phrase(chunkCols);
            cells[1] = cellCols;

            return cells;
        }
    }
}
