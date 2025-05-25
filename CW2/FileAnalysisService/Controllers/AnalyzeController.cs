using FileAnalysisService.Application.DTOs;
using FileAnalysisService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FileAnalysisService.Controllers;

[ApiController]
[Route("api/analyze")]
public class AnalyzeController : ControllerBase
{
    private readonly IAnalyzeFileService _service;
    private readonly ILogger<AnalyzeController> _logger;

    public AnalyzeController(IAnalyzeFileService service, ILogger<AnalyzeController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] Guid fileId, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("AnalyzeAsync called for fileId {FileId}", fileId);
            var response = await _service.AnalyzeAsync(new AnalyzeFileRequest { FileId = fileId }, ct);
            return Ok(response);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning(ex, "File not found during AnalyzeAsync for fileId {FileId}", fileId);
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AnalyzeAsync for fileId {FileId}", fileId);
            return StatusCode(500, "Internal server error during analysis");
        }
    }
    
    [HttpGet("wordcloud/{fileId}")]
    public async Task<IActionResult> DownloadWordCloud(Guid fileId)
    {
        try
        {
            _logger.LogInformation("DownloadWordCloud called for fileId {FileId}", fileId);
            var (content, fileName) = await _service.DownloadWordCloudAsync(fileId);
            
            return File(content, "image/png", fileName);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning(ex, "File not found during DownloadWordCloud for fileId {FileId}", fileId);
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading word cloud for fileId {FileId}", fileId);
            return StatusCode(500, "Internal server error downloading word cloud");
        }
    }
}