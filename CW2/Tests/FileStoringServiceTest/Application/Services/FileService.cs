using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileStoringService.Application.DTOs;
using FileStoringService.Application.Interfaces;
using FileStoringService.Domain.Model.Entities;
using Moq;
using Xunit;

public class FileStorageServiceTests
{
    private readonly Mock<IFileRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly string _storageDir;

    public FileStorageServiceTests()
    {
        _storageDir = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
        if (!Directory.Exists(_storageDir))
            Directory.CreateDirectory(_storageDir);
    }

    [Fact]
    public async Task SaveFileAsync_ReturnsExisting_WhenHashExists()
    {
        var filePath = Path.GetTempFileName();
        await File.WriteAllTextAsync(filePath, "content");
        var hash = Document.CreateFromContent("content");
        var doc = Document.Create("file.txt", filePath, "content");
        typeof(Document).GetProperty("Hash")!.SetValue(doc, hash);

        _repo.Setup(r => r.GetByHashAsync(hash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doc);

        var service = new FileStorageService(_repo.Object, _uow.Object);
        var dto = await service.SaveFileAsync(filePath);

        Assert.Equal(doc.Name, dto.Name);
        Assert.Equal(doc.Hash, dto.Hash);
        File.Delete(filePath);
    }

    [Fact]
    public async Task SaveFileAsync_SavesNewFile_WhenNotExists()
    {
        var filePath = Path.GetTempFileName();
        await File.WriteAllTextAsync(filePath, "unique content");
        var hash = Document.CreateFromContent("unique content");

        _repo.Setup(r => r.GetByHashAsync(hash, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        _repo.Setup(r => r.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(0));;

        var service = new FileStorageService(_repo.Object, _uow.Object);
        var dto = await service.SaveFileAsync(filePath);

        Assert.Equal("unique content", await File.ReadAllTextAsync(dto.Location));
        File.Delete(filePath);
        File.Delete(dto.Location);
    }

    [Fact]
    public async Task GetFileAsync_ReturnsContentAndName_WhenExists()
    {
        var filePath = Path.GetTempFileName();
        var content = "abc";
        await File.WriteAllTextAsync(filePath, content);
        var doc = Document.Create("file.txt", filePath, content);

        _repo.Setup(r => r.GetByIdAsync(doc.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doc);

        var service = new FileStorageService(_repo.Object, _uow.Object);
        var (bytes, name) = await service.GetFileAsync(doc.Id);

        Assert.Equal(content, System.Text.Encoding.UTF8.GetString(bytes));
        Assert.Equal("file.txt", name);
        File.Delete(filePath);
    }

    [Fact]
    public async Task GetFileAsync_Throws_WhenNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        var service = new FileStorageService(_repo.Object, _uow.Object);

        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            service.GetFileAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetFileMetadataAsync_ReturnsDto_WhenExists()
    {
        var doc = Document.Create("file.txt", "/path", "abc");
        _repo.Setup(r => r.GetByIdAsync(doc.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doc);

        var service = new FileStorageService(_repo.Object, _uow.Object);
        var dto = await service.GetFileMetadataAsync(doc.Id);

        Assert.Equal(doc.Id, dto.Id);
        Assert.Equal(doc.Name, dto.Name);
    }

    [Fact]
    public async Task GetFileMetadataAsync_Throws_WhenNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        var service = new FileStorageService(_repo.Object, _uow.Object);

        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            service.GetFileMetadataAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAllFilesMetadataAsync_ReturnsAllDtos()
    {
        var docs = new List<Document>
        {
            Document.Create("a.txt", "/a", "a"),
            Document.Create("b.txt", "/b", "b")
        };
        _repo.Setup(r => r.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(docs);

        var service = new FileStorageService(_repo.Object, _uow.Object);
        var result = await service.GetAllFilesMetadataAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task DeleteFileAsync_Deletes_WhenExists()
    {
        var filePath = Path.GetTempFileName();
        await File.WriteAllTextAsync(filePath, "abc");
        var doc = Document.Create("file.txt", filePath, "abc");

        _repo.Setup(r => r.GetByIdAsync(doc.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doc);
        _repo.Setup(r => r.DeleteAsync(doc, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(0));;

        var service = new FileStorageService(_repo.Object, _uow.Object);
        var result = await service.DeleteFileAsync(doc.Id);

        Assert.True(result);
        File.Delete(filePath);
    }

    [Fact]
    public async Task DeleteFileAsync_ReturnsFalse_WhenNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        var service = new FileStorageService(_repo.Object, _uow.Object);
        var result = await service.DeleteFileAsync(Guid.NewGuid());

        Assert.False(result);
    }
}