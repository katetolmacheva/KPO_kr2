using FileStoringService.Domain.Model.Entities;
using FileStoringService.Entities;

namespace FileStoringService.Application.Interfaces;

public interface IFileRepository
{
    Task<Document?> GetByHashAsync(string hash, CancellationToken ct = default);

    Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IEnumerable<Document>> ListAsync(CancellationToken ct = default);

    Task AddAsync(Document file, CancellationToken ct = default);

    Task DeleteAsync(Document file, CancellationToken ct = default);
}