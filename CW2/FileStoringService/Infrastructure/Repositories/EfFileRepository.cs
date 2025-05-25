using FileStoringService.Application.Interfaces;
using FileStoringService.Domain.Model.Entities;
using FileStoringService.Entities;
using FileStoringService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FileStoringService.Infrastructure.Repositories;

public class EfFileRepository : IFileRepository
{
    private readonly AppDbContext _context;
    public EfFileRepository(AppDbContext context) => _context = context;

    public async Task<Document?> GetByHashAsync(string hash, CancellationToken ct = default)
        => await _context.Files.FirstOrDefaultAsync(f => f.Hash == hash, ct);

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Files.FindAsync(new object[] { id }, ct);

    public async Task<IEnumerable<Document>> ListAsync(CancellationToken ct = default)
        => await _context.Files.ToListAsync(ct);

    public async Task AddAsync(Document file, CancellationToken ct = default)
        => await _context.Files.AddAsync(file, ct);

    public Task DeleteAsync(Document file, CancellationToken ct = default)
    {
        _context.Files.Remove(file);
        return Task.CompletedTask;
    }
}