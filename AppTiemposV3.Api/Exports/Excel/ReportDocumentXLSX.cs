using AppTiemposV3.SharedClases.DTOs.Reports;
using ClosedXML.Excel;

namespace AppTiemposV3.Api.Exports.Excel
{
    public class ReportDocumentXLSX
    {
        public ListReportDto Reporte { get; }

        public ReportDocumentXLSX(ListReportDto reporte)
        {
            Reporte = reporte;
        }

        public byte[] GenerateExcel()
        {
            using (IXLWorkbook workbook = new XLWorkbook())
            {
                IXLWorksheet? worksheet = workbook.Worksheets.Add("Reporte");

                if (Reporte.DataResult != null && Reporte.DataResult.Any())
                {
                    Dictionary<string, object?>? firstRow = Reporte.DataResult.First();
                    List<string>? headers = firstRow.Keys.ToList();

                    for (int i = 0; i < headers.Count; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = headers[i];
                        worksheet.Cell(1, i + 1).Style.Font.Bold = true; 
                    }

                    int currentRow = 2;
                    foreach (Dictionary<string, object?>? item in Reporte.DataResult)
                    {
                        for (int i = 0; i < headers.Count; i++)
                        {
                            object? value = item[headers[i]];
                            worksheet.Cell(currentRow, i + 1).Value = XLCellValue.FromObject(value);
                        }
                        currentRow++;
                    }

                    IXLRange? range = worksheet.Range(1, 1, currentRow - 1, headers.Count);
                    range.CreateTable();
                }

                worksheet.Columns().AdjustToContents();

                using (MemoryStream stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

    }
}
