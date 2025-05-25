using FileStoringService.Application.DTOs;
using FileStoringService.Application.Interfaces;
using FileStoringService.Domain.Model.Entities;
using FileStoringService.Entities;
using System.IO;

public class FileStorageService : IFileStoringService
{
    private readonly IFileRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly string _storageDir;

    public FileStorageService(
        IFileRepository repo,
        IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
        _storageDir = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
        Directory.CreateDirectory(_storageDir);
    }

    public async Task<FileDto> SaveFileAsync(string sourceFilePath, CancellationToken ct = default)
    {
        var content = await File.ReadAllTextAsync(sourceFilePath, ct);
        var name = Path.GetFileName(sourceFilePath);
        var hash = Document.CreateFromContent(content);

        var existing = await _repo.GetByHashAsync(hash, ct);
        if (existing != null)
            return ToDto(existing);

        var id = Guid.NewGuid();
        var ext = Path.GetExtension(name);
        var destName = id + ext;
        var destPath = Path.Combine(_storageDir, destName);
        File.Copy(sourceFilePath, destPath, overwrite: true);

        var doc = Document.Create(name, destPath, content);
        typeof(Document).GetProperty("Id")!
            .SetValue(doc, id);

        await _repo.AddAsync(doc, ct);
        await _uow.SaveChangesAsync(ct);

        return ToDto(doc);
    }

    public async Task<(byte[] Content, string Name)> GetFileAsync(Guid id, CancellationToken ct = default)
    {
        var doc = await _repo.GetByIdAsync(id, ct)
            ?? throw new FileNotFoundException($"Document {id} not found");
        var bytes = await File.ReadAllBytesAsync(doc.Location, ct);
        return (bytes, doc.Name);
    }

    public async Task<FileDto> GetFileMetadataAsync(Guid id, CancellationToken ct = default)
    {
        var doc = await _repo.GetByIdAsync(id, ct)
            ?? throw new FileNotFoundException($"Document {id} not found");
        return ToDto(doc);
    }

    public async Task<IEnumerable<FileDto>> GetAllFilesMetadataAsync(CancellationToken ct = default)
    {
        var all = await _repo.ListAsync(ct);
        return all.Select(ToDto);
    }

    public async Task<bool> DeleteFileAsync(Guid id, CancellationToken ct = default)
    {
        var doc = await _repo.GetByIdAsync(id, ct);
        if (doc == null) return false;

        if (File.Exists(doc.Location))
            File.Delete(doc.Location);

        await _repo.DeleteAsync(doc, ct);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    private static FileDto ToDto(Document d)
        => new()
        {
            Id = d.Id,
            Name = d.Name,
            Hash = d.Hash,
            Location = d.Location,
            UploadDate = d.UploadDate
        };
}