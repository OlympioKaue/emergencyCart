using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.AccountContext.UseCases.Inspections.Get.GetHistory;
using EmergencyCart.Domain.AccountContext.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using System.Text;

namespace EmergencyCart.Infrastructure.AccountContext.Repositories.Services;

internal class HistoryFileGenerator : IHistoryFileGenerator
{
    private static readonly string[] Headers =
         ["Carrinho", "Setor", "Funcionário", "Finalizada em (UTC)", "Itens", "Faltantes"];

    static HistoryFileGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(ExportFormat format, IReadOnlyCollection<HistoryItemResponse> items, string filterLabel, DateTime generatedAtUtc)
        => format switch
        {
            ExportFormat.Csv => BuildCsv(items),
            ExportFormat.Pdf => BuildPdf(items, filterLabel, generatedAtUtc),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };

    private static string[] ToRow(HistoryItemResponse item) =>
    [
        $"Carrinho {item.CartCode}",
        item.SectorName,
        item.EmployeeName,
        item.FinishedAt.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
        item.TotalItems.ToString(CultureInfo.InvariantCulture),
        item.MissingItems.ToString(CultureInfo.InvariantCulture)
    ];

    private static byte[] BuildCsv(IReadOnlyCollection<HistoryItemResponse> items)
    {
        var builder = new StringBuilder();
        builder.Append(string.Join(';', Headers.Select(CsvCell))).Append("\r\n");

        foreach (var item in items)
            builder.Append(string.Join(';', ToRow(item).Select(CsvCell))).Append("\r\n");

        var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        return utf8.GetPreamble().Concat(utf8.GetBytes(builder.ToString())).ToArray();
    }

    private static string CsvCell(string text)
    {
        if (text.Length > 0 && "=+-@".Contains(text[0]))
            text = "'" + text;

        return $"\"{text.Replace("\"", "\"\"")}\"";
    }

    private static byte[] BuildPdf(IReadOnlyCollection<HistoryItemResponse> items, string filterLabel, DateTime generatedAtUtc)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(column =>
                {
                    column.Item().Text("EmergencyCart - Histórico de conferências").FontSize(16).Bold();
                    column.Item().Text($"Filtro: {filterLabel}");
                    column.Item().Text($"Gerado em: {generatedAtUtc:dd/MM/yyyy HH:mm} (UTC)");
                });

                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        foreach (var title in Headers)
                            header.Cell().Background(Colors.Blue.Darken2).Padding(4)
                                .Text(title).FontColor(Colors.White).Bold();
                    });

                    foreach (var item in items)
                        foreach (var value in ToRow(item))
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(value);
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Página ");
                    text.CurrentPageNumber();
                    text.Span(" de ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf();
    }
}
