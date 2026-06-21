using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Sudoku.Server.Services;

public class PdfService
{
    private const string AccentColor = "#C71585";
    private const string HeaderColor = "#333333";

    public PdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateSingle(int[][] grid, string? title = null)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                ConfigurePage(page);
                page.Content().Column(column =>
                {
                    column.Item().PaddingBottom(20).Element(c => RenderHeader(c));
                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        column.Item().PaddingBottom(10).Text(title).FontFamily(Fonts.Courier).FontSize(14);
                    }
                    column.Item().Element(c => RenderGrid(c, grid));
                });
            });
        }).GeneratePdf();
    }

    public byte[] GenerateBooklet(IReadOnlyList<int[][]> puzzles)
    {
        return Document.Create(container =>
        {
            for (int i = 0; i < puzzles.Count; i++)
            {
                int pageNumber = i + 1;
                var puzzle = puzzles[i];

                container.Page(page =>
                {
                    ConfigurePage(page);
                    page.Content().Column(column =>
                    {
                        column.Item().PaddingBottom(20).Element(c => RenderHeader(c));
                        column.Item().PaddingBottom(8).Text($"Puzzle {pageNumber}")
                            .FontFamily(Fonts.Courier).FontSize(14).FontColor(Colors.Grey.Darken2);
                        column.Item().Element(c => RenderGrid(c, puzzle));
                    });
                });
            }
        }).GeneratePdf();
    }

    private static void ConfigurePage(PageDescriptor page)
    {
        page.Size(PageSizes.A4);
        page.Margin(40);
        page.DefaultTextStyle(x => x.FontFamily(Fonts.Courier).FontSize(12));
    }

    private static void RenderHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.AutoItem().Text("My").FontFamily(Fonts.Courier).FontSize(28).Bold().FontColor(AccentColor);
            row.AutoItem().Text("Sudoku").FontFamily(Fonts.Courier).FontSize(28).Bold().FontColor(HeaderColor);
        });
    }

    private static void RenderGrid(IContainer container, int[][] grid)
    {
        container.AlignCenter().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                for (int i = 0; i < 9; i++)
                {
                    columns.ConstantColumn(32);
                }
            });

            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    int value = grid[row][col];
                    bool thickTop = row % 3 == 0;
                    bool thickLeft = col % 3 == 0;
                    bool thickBottom = row == 8;
                    bool thickRight = col == 8;

                    table.Cell().BorderTop(thickTop ? 2 : 0.5f)
                        .BorderLeft(thickLeft ? 2 : 0.5f)
                        .BorderBottom(thickBottom ? 2 : 0.5f)
                        .BorderRight(thickRight ? 2 : 0.5f)
                        .BorderColor(Colors.Black)
                        .Height(32)
                        .AlignCenter()
                        .AlignMiddle()
                        .Text(value == 0 ? " " : value.ToString())
                        .FontFamily(Fonts.Courier)
                        .FontSize(16);
                }
            }
        });
    }
}
