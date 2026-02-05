using AppTiemposV3.SharedClases.DTOs.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using static AppTiemposV3.Api.Utilidades.SvgBase64ToPngBytes;

namespace AppTiemposV3.Api.Exports.PDF
{
    public class ReportDocumentPDF : IDocument
    {
        public ListReportDto Reporte { get; }
        public ReportDocumentPDF(ListReportDto reporte)
        {
            Reporte = reporte;
        }

        public void Compose(IDocumentContainer cnt)
        {
            cnt.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));
                page.Header().Element(ConstructHeader);
                page.Content().Column(col =>
                {
                    col.Item().Element(ConstructContent);

                    col.Item().PaddingTop(20);

                    col.Item().Element(ConstructTable);
                });
                page.Footer().Element(ConstructFooter);
            });
        }

        private void ConstructHeader(IContainer cnt)
        {
            string base64Image = "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIyNCIgaGVpZ2h0PSIyNCIgdmlld0JveD0iMCAwIDI0IDI0IiBmaWxsPSJub25lIiBzdHJva2U9IiNmZmZmZmYiIHN0cm9rZS13aWR0aD0iMiIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBjbGFzcz0ibHVjaWRlIGx1Y2lkZS1jbG9jay1pY29uIGx1Y2lkZS1jbG9jayI+PHBhdGggZD0iTTEyIDZ2Nmw0IDIiLz48Y2lyY2xlIGN4PSIxMiIgY3k9IjEyIiByPSIxMCIvPjwvc3ZnPg==";
            byte[]? pngBytes = ConvertSvgToPng(base64Image, 64, 64);

            cnt.Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Row(innerRow =>
                    {
                        innerRow.ConstantItem(35).Height(35)
                            .Container()
                            .CornerRadius(0.53f, Unit.Centimetre)
                            .Background("#FF6B35")
                            .Padding(8)
                            .AlignCenter()
                            .AlignMiddle()
                            .Image(pngBytes);

                        innerRow.RelativeItem().PaddingLeft(10).Column(textCol =>
                        {
                            textCol.Item()
                                .Text("TimeTracker")
                                .FontSize(20)
                                .SemiBold()
                                .FontColor(Color.FromHex("#2D3748"))
                                .FontFamily("Segoe UI");

                            textCol.Item().PaddingTop(-1)
                                .Text("Sistema de Gestión de Tiempos")
                                .FontSize(8)
                                .FontColor(Color.FromHex("#718096"))
                                .FontFamily("Segoe UI");
                        });
                    });

                    row.ConstantItem(100)
                        .AlignRight()
                        .AlignBottom()
                        .PaddingBottom(5)
                        .Container()
                        .CornerRadius(0.53f, Unit.Centimetre) 
                        .Background("#FF6B35")
                        .PaddingVertical(6)
                        .PaddingHorizontal(12)
                        .AlignCenter()
                        .Text("REPORTE")
                        .FontSize(8)
                        .ExtraBold()
                        .FontColor(Colors.White)
                        .FontFamily("Segoe UI");
                });

                col.Item().PaddingTop(10).LineHorizontal(2.5f).LineColor("#FF6B35");
            });
        }

        private void ConstructContent(IContainer cnt)
        {
            cnt.PaddingTop(10).Column(col =>
            {
                col.Item()
                    .Container()
                    .Background(Color.FromHex("#F7FAFC")) 
                    .BorderLeft(2)
                    .BorderColor(Color.FromHex("#4299E1")) 
                    .CornerRadius(8) 
                    .Padding(20)
                    .Column(innerCol =>
                    {
                        innerCol.Item()
                            .Text(Reporte.Name)
                            .FontSize(14)
                            .SemiBold()
                            .FontColor(Color.FromHex("#2D3748"));

                        innerCol.Item()
                            .PaddingTop(5)
                            .Text(Reporte.Description)
                            .FontSize(10)
                            .FontColor(Color.FromHex("#4A5568"));

                        innerCol.Item().PaddingTop(12).Row(row =>
                        {
                            row.AutoItem().Row(r => {
                                string calendarB64 = "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIyNCIgaGVpZ2h0PSIyNCIgdmlld0JveD0iMCAwIDI0IDI0IiBmaWxsPSJub25lIiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS13aWR0aD0iMiIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBjbGFzcz0ibHVjaWRlIGx1Y2lkZS1jYWxlbmRhci1kYXlzLWljb24gbHVjaWRlLWNhbGVuZGFyLWRheXMiPjxwYXRoIGQ9Ik04IDJ2NCIvPjxwYXRoIGQ9Ik0xNiAydjQiLz48cmVjdCB3aWR0aD0iMTgiIGhlaWdodD0iMTgiIHg9IjMiIHk9IjQiIHJ4PSIyIi8+PHBhdGggZD0iTTMgMTBoMTgiLz48cGF0aCBkPSJNOCAxNGguMDEiLz48cGF0aCBkPSJNMTIgMTRoLjAxIi8+PHBhdGggZD0iTTE2IDE0aC4wMSIvPjxwYXRoIGQ9Ik04IDE4aC4wMSIvPjxwYXRoIGQ9Ik0xMiAxOGguMDEiLz48cGF0aCBkPSJNMTYgMThoLjAxIi8+PC9zdmc+";
                                byte[]? calendarBytes = ConvertSvgToPng(calendarB64, 64, 64);
                                r.Spacing(4);
                                r.AutoItem()
                                    .PaddingTop(2)
                                    .Width(10)
                                    .Height(10)
                                    .AlignMiddle()
                                    .Image(calendarBytes);
                                r.AutoItem()
                                    .PaddingTop(.25f)
                                    .Text("Generado:")
                                    .FontSize(10)
                                    .SemiBold()
                                    .FontColor("#4A5568");
                                r.AutoItem()
                                    .PaddingTop(.25f)
                                    .Text(Reporte.CreatedAt.ToString("d 'de' MMMM 'de' yyyy, HH:mm",new CultureInfo("es-AR")))
                                    .FontSize(10)
                                    .FontColor("#718096");
                            });

                            row.ConstantItem(20); 

                            row.AutoItem().Row(r => {
                                string reportB64 = "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIyNCIgaGVpZ2h0PSIyNCIgdmlld0JveD0iMCAwIDI0IDI0IiBmaWxsPSJub25lIiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS13aWR0aD0iMiIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBjbGFzcz0ibHVjaWRlIGx1Y2lkZS1jaGFydC1jb2x1bW4taWNvbiBsdWNpZGUtY2hhcnQtY29sdW1uIj48cGF0aCBkPSJNMyAzdjE2YTIgMiAwIDAgMCAyIDJoMTYiLz48cGF0aCBkPSJNMTggMTdWOSIvPjxwYXRoIGQ9Ik0xMyAxN1Y1Ii8+PHBhdGggZD0iTTggMTd2LTMiLz48L3N2Zz4=";
                                byte[]? reportBytes = ConvertSvgToPng(reportB64, 64, 64);
                                r.Spacing(4);
                                r.AutoItem()
                                   .PaddingTop(2)
                                   .Width(10)
                                   .Height(10)
                                   .AlignMiddle()
                                   .Image(reportBytes);
                                r.AutoItem()
                                    .PaddingTop(.25f)
                                    .Text("Registros:")
                                    .FontSize(10)
                                    .SemiBold()
                                    .FontColor("#4A5568")
                                    .FontFamily("Segoe UI");
                                r.AutoItem()
                                    .Container()
                                    .CornerRadius(10)
                                    .Background("#FF6B35")
                                    .PaddingTop(.25f)
                                    .PaddingHorizontal(8)
                                    .Text(Reporte.QueryResult.ToString())
                                    .FontSize(9)
                                    .FontColor(Colors.White)
                                    .FontFamily("Segoe UI")
                                    .SemiBold();
                            });

                            row.ConstantItem(20); 

                            string mode = string.Empty;

                            if(Reporte.ReportMode == "visual")
                            {
                                mode = "Constructor visual";
                            }
                            else
                            {
                                mode = "SQL Personalizado";
                            }

                            row.AutoItem().Row(r =>
                            {
                                string modeB64 = "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIyNCIgaGVpZ2h0PSIyNCIgdmlld0JveD0iMCAwIDI0IDI0IiBmaWxsPSJub25lIiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS13aWR0aD0iMiIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIiBjbGFzcz0ibHVjaWRlIGx1Y2lkZS1wZW5jaWwtcnVsZXItaWNvbiBsdWNpZGUtcGVuY2lsLXJ1bGVyIj48cGF0aCBkPSJNMTMgNyA4LjcgMi43YTIuNDEgMi40MSAwIDAgMC0zLjQgMEwyLjcgNS4zYTIuNDEgMi40MSAwIDAgMCAwIDMuNEw3IDEzIi8+PHBhdGggZD0ibTggNiAyLTIiLz48cGF0aCBkPSJtMTggMTYgMi0yIi8+PHBhdGggZD0ibTE3IDExIDQuMyA0LjNjLjk0Ljk0Ljk0IDIuNDYgMCAzLjRsLTIuNiAyLjZjLS45NC45NC0yLjQ2Ljk0LTMuNCAwTDExIDE3Ii8+PHBhdGggZD0iTTIxLjE3NCA2LjgxMmExIDEgMCAwIDAtMy45ODYtMy45ODdMMy44NDIgMTYuMTc0YTIgMiAwIDAgMC0uNS44M2wtMS4zMjEgNC4zNTJhLjUuNSAwIDAgMCAuNjIzLjYyMmw0LjM1My0xLjMyYTIgMiAwIDAgMCAuODMtLjQ5N3oiLz48cGF0aCBkPSJtMTUgNSA0IDQiLz48L3N2Zz4=";
                                byte[]? modeBytes = ConvertSvgToPng(modeB64, 64, 64);
                                r.Spacing(4);
                                r.AutoItem()
                                  .PaddingTop(2)
                                  .Width(10)
                                  .Height(10)
                                  .AlignMiddle()
                                  .Image(modeBytes);
                                r.AutoItem()
                                    .PaddingTop(.25f)
                                    .Text("Modo:")
                                    .FontSize(10)
                                    .SemiBold()
                                    .FontColor("#4A5568")
                                    .FontFamily("Segoe UI");
                                r.AutoItem()
                                    .PaddingTop(.25f)
                                    .Text(mode)
                                    .FontSize(10)
                                    .FontColor("#718096")
                                    .FontFamily("Segoe UI");
                            });
                        });
                    });
            });
        }

        private void ConstructTable(IContainer cnt)
        {
            List<Dictionary<string, object?>>? data = Reporte.DataResult; 

            if (data == null || !data.Any())
            {
                cnt.PaddingTop(10)
                    .Text("No hay datos disponibles para mostrar.");
                return;
            }

            // Obtenemos los nombres de las columnas del primer registro
            List<string>? columnNames = data.First().Keys.ToList();

            cnt.PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    foreach (string? col in columnNames)
                    {
                        if (col.ToLower().Contains("descripcion") || col.ToLower().Contains("cliente") || col.ToLower().Contains("requerimiento"))
                            columns.RelativeColumn(3);
                        else if (col.ToLower().Contains("fecha") || col.ToLower().Contains("hora") || col.ToLower().Contains("reqid"))
                            columns.RelativeColumn(1);
                        else
                            columns.RelativeColumn();
                    }                    
                });

                table.Header(header =>
                {
                    foreach (string? colName in columnNames)
                    {
                        header.Cell()
                            .Border(0)
                            .Background(Color.FromHex("#4A5568"))
                            .Padding(5)
                            .AlignLeft()
                            .AlignMiddle()
                            .Text(colName.ToUpper())
                            .FontColor(Colors.White)
                            .SemiBold()
                            .FontSize(6)
                            .FontFamily("Segoe UI");
                    }
                });

                foreach (Dictionary<string, object?>? rowData in data)
                {
                    foreach (string? colName in columnNames)
                    {
                        object? value = rowData[colName];

                        table.Cell()
                            .Element(RowStyle)
                            .Text(FormatValue(value))
                            .FontSize(5)
                            .FontColor(Color.FromHex("#2D3748"))
                            .FontFamily("Segoe UI");
                    }

                    static IContainer RowStyle(IContainer container)
                    {
                        return container
                            .BorderBottom(1)
                            .BorderColor(Color.FromHex("#E2E8F0")) 
                            .PaddingVertical(8)
                            .PaddingHorizontal(5)
                            .AlignLeft();
                    }
                }
            });
        }

        private string FormatValue(object? value)
        {
            if (value == null) return "-";

            if (value is DateTime dt)
                return dt.ToString("dd/MM/yyyy");

            if (value is bool b)
                return b ? "Sí" : "No";

            return value.ToString() ?? "-";
        }

        private void ConstructFooter(IContainer cnt)
        {
            int year = DateTime.Now.Year;
            cnt.Element(EstiloFooter).AlignCenter().Text(t =>
            {
                t.Span("Generado por TimeTracker - Sistema de Gestión de Tiempos y Actividades").FontSize(8).FontColor(Color.FromHex("#a0aec0")).SemiBold().FontFamily("Segoe UI");
                t.EmptyLine();
                t.Span($"© {year} - Todos los derechos reservados").FontSize(8).FontColor(Color.FromHex("#a0aec0")).SemiBold().FontFamily("Segoe UI");
                t.EmptyLine();
                t.Span("Página ").FontSize(8).FontColor(Color.FromHex("#a0aec0")).FontFamily("Segoe UI");
                t.CurrentPageNumber().FontSize(8).FontColor(Color.FromHex("#a0aec0")).FontFamily("Segoe UI");
                t.Span(" de ").FontSize(8).FontColor(Color.FromHex("#a0aec0")).FontFamily("Segoe UI");
                t.TotalPages().FontSize(8).FontColor(Color.FromHex("#a0aec0")).FontFamily("Segoe UI");                
            });

            static IContainer EstiloFooter(IContainer cnt)
            {
                return cnt.BorderTop(2).BorderColor(Color.FromHex("#e2e8f0")).PaddingTop(20);
            }
        }
    }
}
