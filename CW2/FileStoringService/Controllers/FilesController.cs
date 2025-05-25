using Microsoft.AspNetCore.Mvc;
using FileStoringService.Domain.Interfaces;
using FileStoringService.Application.DTOs;
using FileStoringService.Application.Interfaces;

namespace FileStoringService.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IFileStoringService _service;

    public FilesController(IFileStoringService service)
    {
        _service = service;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] FileUploadDto dto, CancellationToken ct)
    {
        var file = dto.File;

        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var tempPath = Path.GetTempFileName();
        await using (var stream = System.IO.File.Create(tempPath))
        {
            await file.CopyToAsync(stream, ct);
        }

        var resultDto = await _service.SaveFileAsync(tempPath, ct);
        return CreatedAtAction(nameof(GetMetadata), new { id = resultDto.Id }, resultDto);
    }



    [HttpGet]
    public async Task<IActionResult> GetAllMetadata(CancellationToken ct)
    {
        var list = await _service.GetAllFilesMetadataAsync(ct);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        try
        {
            var (content, name) = await _service.GetFileAsync(id, ct);
            return File(content, "application/octet-stream", name);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{id:guid}/metadata")]
    public async Task<IActionResult> GetMetadata(Guid id, CancellationToken ct)
    {
        try
        {
            var meta = await _service.GetFileMetadataAsync(id, ct);
            return Ok(meta);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var removed = await _service.DeleteFileAsync(id, ct);
        return removed ? NoContent() : NotFound();
    }
}
