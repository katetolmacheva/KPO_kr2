using FileAnalysisService.Domain.Entities;

namespace FileAnalysisService.Domain.Interfaces;


public interface IFileAnalysisRepository
{
    Task<FileAnalysisResult?> GetByFileIdAsync(Guid fileId);
    Task SaveAsync(FileAnalysisResult result);
}