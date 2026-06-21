namespace Sudoku.Server.Models;

public class SudokuGridRequest
{
    public int[][] Grid { get; set; } = Array.Empty<int[]>();
}

public class NewSudokuRequest
{
    public string? Difficulty { get; set; }
}

public class BookletRequest
{
    public int Count { get; set; } = 1;
    public string? Difficulty { get; set; }
}

public class SudokuResponse
{
    public int[][] Puzzle { get; set; } = Array.Empty<int[]>();
    public int[][]? Solution { get; set; }
    public bool Solved { get; set; }
}

public class PdfRequest
{
    public int[][] Grid { get; set; } = Array.Empty<int[]>();
    public string? Title { get; set; }
}
