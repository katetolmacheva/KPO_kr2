using FileAnalysisService.Domain.Entities;
using FileAnalysisService.Domain.Interfaces;
using FileAnalysisService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FileAnalysisService.Infrastructure.Repositories;

public class FileAnalysisRepository : IFileAnalysisRepository
{
    private readonly TextAnalysisDbContext _db;

    public FileAnalysisRepository(TextAnalysisDbContext db)
    {
        _db = db;
    }

    public async Task<FileAnalysisResult?> GetByFileIdAsync(Guid fileId)
    {
        return await _db.FileAnalysisResults
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.FileId == fileId);
    }

    public async Task SaveAsync(FileAnalysisResult result)
    {
        var exists = await _db.FileAnalysisResults.AnyAsync(r => r.FileId == result.FileId);
        if (exists)
        {
            _db.FileAnalysisResults.Update(result);
        }
        else
        {
            await _db.FileAnalysisResults.AddAsync(result);
        }

        await _db.SaveChangesAsync();
    }
}