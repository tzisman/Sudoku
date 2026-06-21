using Microsoft.AspNetCore.Mvc;
using Sudoku.Server.Models;
using Sudoku.Server.Services;

namespace Sudoku.Server.Controllers;

[ApiController]
[Route("api/sudoku")]
public class SudokuController : ControllerBase
{
    private readonly SudokuService _sudokuService;
    private readonly PdfService _pdfService;

    public SudokuController(SudokuService sudokuService, PdfService pdfService)
    {
        _sudokuService = sudokuService;
        _pdfService = pdfService;
    }

    [HttpPost("new")]
    public ActionResult<SudokuResponse> New([FromBody] NewSudokuRequest? request)
    {
        var response = _sudokuService.CreateNew(request?.Difficulty);
        return Ok(response);
    }

    [HttpPost("solve")]
    public ActionResult<SudokuResponse> Solve([FromBody] SudokuGridRequest request)
    {
        if (request.Grid.Length != 9 || request.Grid.Any(row => row.Length != 9))
        {
            return BadRequest("Grid must be 9x9.");
        }

        var response = _sudokuService.Solve(request.Grid);
        return Ok(response);
    }

    [HttpPost("fix")]
    public ActionResult<SudokuResponse> Fix([FromBody] SudokuGridRequest request)
    {
        if (request.Grid.Length != 9 || request.Grid.Any(row => row.Length != 9))
        {
            return BadRequest("Grid must be 9x9.");
        }

        try
        {
            var response = _sudokuService.Fix(request.Grid);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("upload")]
    public async Task<ActionResult<SudokuResponse>> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var grid = _sudokuService.ParseFile(stream);
            var response = _sudokuService.Solve(grid);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("pdf")]
    public ActionResult DownloadPdf([FromBody] PdfRequest request)
    {
        if (request.Grid.Length != 9 || request.Grid.Any(row => row.Length != 9))
        {
            return BadRequest("Grid must be 9x9.");
        }

        var pdf = _pdfService.GenerateSingle(request.Grid, request.Title);
        return File(pdf, "application/pdf", "sudoku.pdf");
    }

    [HttpPost("booklet")]
    public ActionResult DownloadBooklet([FromBody] BookletRequest request)
    {
        if (request.Count < 1 || request.Count > 15)
        {
            return BadRequest("Booklet count must be between 1 and 15.");
        }

        var puzzles = _sudokuService.CreateBookletPuzzles(request.Count, request.Difficulty);
        var pdf = _pdfService.GenerateBooklet(puzzles);
        return File(pdf, "application/pdf", "sudoku-booklet.pdf");
    }
}
