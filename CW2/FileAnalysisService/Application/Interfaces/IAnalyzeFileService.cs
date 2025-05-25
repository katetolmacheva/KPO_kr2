using FileAnalysisService.Application.DTOs;

namespace FileAnalysisService.Application.Interfaces;

public interface IAnalyzeFileService
{
    Task<AnalyzeFileResponse> AnalyzeAsync(AnalyzeFileRequest request, CancellationToken cancellationToken = default);
    Task<(byte[] Content, string FileName)> DownloadWordCloudAsync(Guid fileId, CancellationToken cancellationToken = default);
}