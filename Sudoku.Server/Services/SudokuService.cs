using Sudoku;
using Sudoku.Server.Models;

namespace Sudoku.Server.Services;

public class SudokuService
{
    private const int ParallelAttempts = 3;
    private static readonly TimeSpan AttemptTimeout = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan TotalBudget = TimeSpan.FromSeconds(6);

    private static readonly int[][][] FallbackPuzzles =
    [
        new int[][]
        {
            new[] {5,3,0,0,7,0,0,0,0}, new[] {6,0,0,1,9,5,0,0,0}, new[] {0,9,8,0,0,0,0,6,0},
            new[] {8,0,0,0,6,0,0,0,3}, new[] {4,0,0,8,0,3,0,0,1}, new[] {7,0,0,0,2,0,0,0,6},
            new[] {0,6,0,0,0,0,2,8,0}, new[] {0,0,0,4,1,9,0,0,5}, new[] {0,0,0,0,8,0,0,7,9}
        },
        new int[][]
        {
            new[] {0,0,0,2,6,0,7,0,1}, new[] {6,8,0,0,0,0,0,0,0}, new[] {0,0,0,0,0,0,0,9,8},
            new[] {0,0,0,0,0,0,0,0,0}, new[] {0,5,0,0,0,0,0,0,0}, new[] {0,0,0,0,0,0,0,0,0},
            new[] {0,0,0,0,0,0,0,0,0}, new[] {0,0,0,0,0,0,0,0,0}, new[] {0,0,0,0,0,0,0,0,0}
        },
        new int[][]
        {
            new[] {0,0,0,6,0,0,4,0,0}, new[] {7,0,0,0,0,3,6,0,0}, new[] {0,0,0,0,9,1,0,0,0},
            new[] {0,0,0,0,0,0,0,0,0}, new[] {0,0,0,0,0,0,0,0,0}, new[] {0,0,0,0,0,0,0,0,0},
            new[] {0,0,0,0,0,0,0,0,0}, new[] {0,0,0,0,0,0,0,0,0}, new[] {0,0,0,0,0,0,0,0,0}
        },
    ];

    public SudokuResponse CreateNew(string? difficulty)
    {
        var game = CreateSudokuWithRetry();
        ApplyDifficulty(game, difficulty);

        return new SudokuResponse
        {
            Puzzle = GetMergedBoard(game),
            Solved = false
        };
    }

    public SudokuResponse Solve(int[][] grid)
    {
        var puzzle = CloneGrid(grid);
        var game = new SudokuGame(0, 0, 0);
        game.Replace(ToMatrix(puzzle));
        game.Mat = CloneToMatrix(puzzle);

        bool solved = game.SolveSudoku();
        var solution = GetMergedBoard(game);

        return new SudokuResponse
        {
            Puzzle = puzzle,
            Solution = solution,
            Solved = solved
        };
    }

    public int[][] ParseFile(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var text = reader.ReadToEnd();
        var digits = new List<int>();

        foreach (char c in text)
        {
            if (char.IsDigit(c))
            {
                digits.Add(c - '0');
            }
        }

        if (digits.Count != 81)
        {
            throw new ArgumentException("File must contain exactly 81 digits (0 = empty cell).");
        }

        var grid = new int[9][];
        for (int i = 0; i < 9; i++)
        {
            grid[i] = new int[9];
            for (int j = 0; j < 9; j++)
            {
                int value = digits[i * 9 + j];
                if (value < 0 || value > 9)
                {
                    throw new ArgumentException("Each cell must be a digit from 0 to 9.");
                }
                grid[i][j] = value;
            }
        }

        return grid;
    }

    public List<int[][]> CreateBookletPuzzles(int count, string? difficulty)
    {
        count = Math.Clamp(count, 1, 15);
        var puzzles = new List<int[][]>();

        for (int i = 0; i < count; i++)
        {
            var response = CreateNew(difficulty);
            puzzles.Add(response.Puzzle);
        }

        return puzzles;
    }

    private static SudokuGame CreateSudokuWithRetry()
    {
        var deadline = DateTime.UtcNow + TotalBudget;

        while (DateTime.UtcNow < deadline)
        {
            var attempts = new (SudokuGame Game, Task<bool> Task)[ParallelAttempts];
            for (int i = 0; i < ParallelAttempts; i++)
            {
                var game = new SudokuGame(0, 0, 0);
                attempts[i] = (game, Task.Run(() => game.CreateSudoku()));
            }

            var pending = attempts.Select(a => a.Task).Cast<Task>().ToList();
            while (pending.Count > 0 && DateTime.UtcNow < deadline)
            {
                var waitMs = (int)Math.Max(1, (deadline - DateTime.UtcNow).TotalMilliseconds);
                var delay = Task.Delay(Math.Min(waitMs, (int)AttemptTimeout.TotalMilliseconds));
                var finished = Task.WhenAny(pending.Append(delay)).GetAwaiter().GetResult();

                if (finished == delay)
                {
                    break;
                }

                pending.Remove(finished);

                if (finished is Task<bool> { IsCompletedSuccessfully: true, Result: true } success)
                {
                    var match = attempts.First(a => a.Task == success);
                    return match.Game;
                }
            }
        }

        return LoadFallbackPuzzle();
    }

    private static SudokuGame LoadFallbackPuzzle()
    {
        var puzzle = FallbackPuzzles[Random.Shared.Next(FallbackPuzzles.Length)];
        var game = new SudokuGame(0, 0, 0);
        var matrix = ToMatrix(puzzle);
        game.Replace(matrix);
        game.Mat = (int[,])matrix.Clone();
        return game;
    }

    private static void ApplyDifficulty(SudokuGame game, string? difficulty)
    {
        switch (difficulty?.ToLowerInvariant())
        {
            case "mid":
                game.MidLevel();
                break;
            case "easy":
                game.EasyLevel();
                break;
            case "veryeasy":
                game.VeryEasyLevel();
                break;
        }
    }

    private static int[][] GetMergedBoard(SudokuGame game)
    {
        var board = new int[9, 9];
        game.ReturnReplace(board);

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (game.Mat[i, j] != 0)
                {
                    board[i, j] = game.Mat[i, j];
                }
            }
        }

        return ToJagged(board);
    }

    private static int[,] ToMatrix(int[][] grid)
    {
        var matrix = new int[9, 9];
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                matrix[i, j] = grid[i][j];
            }
        }
        return matrix;
    }

    private static int[,] CloneToMatrix(int[][] grid) => ToMatrix(grid);

    private static int[][] ToJagged(int[,] matrix)
    {
        var grid = new int[9][];
        for (int i = 0; i < 9; i++)
        {
            grid[i] = new int[9];
            for (int j = 0; j < 9; j++)
            {
                grid[i][j] = matrix[i, j];
            }
        }
        return grid;
    }

    private static int[][] CloneGrid(int[][] grid)
    {
        var clone = new int[9][];
        for (int i = 0; i < 9; i++)
        {
            clone[i] = (int[])grid[i].Clone();
        }
        return clone;
    }
}
