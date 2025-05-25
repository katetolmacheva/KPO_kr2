using FileStoringService.Application.DTOs;

namespace FileStoringService.Application.Interfaces;

public interface IFileStoringService
{
    Task<FileDto> SaveFileAsync(string sourceFilePath, CancellationToken ct = default);
    Task<(byte[] Content, string Name)> GetFileAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<FileDto>> GetAllFilesMetadataAsync(CancellationToken ct = default);
    Task<bool> DeleteFileAsync(Guid id, CancellationToken ct = default);
    Task<FileDto> GetFileMetadataAsync(Guid id, CancellationToken ct = default);
}