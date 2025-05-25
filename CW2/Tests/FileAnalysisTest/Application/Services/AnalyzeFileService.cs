using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileAnalysisService.Application.DTOs;
using FileAnalysisService.Application.Services;
using FileAnalysisService.Domain.Entities;
using FileAnalysisService.Domain.Interfaces;
using FileStoringService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FileAnalysisTest.Application.Services
{
    public class AnalyzeFileServiceTests
    {
        private readonly Mock<IFileAnalysisRepository> _repoMock = new();
        private readonly Mock<IFileStoringService> _storageMock = new();
        private readonly Mock<IWordCloudGenerator> _cloudGenMock = new();
        private readonly Mock<ILogger<AnalyzeFileService>> _loggerMock = new();

        private readonly AnalyzeFileService _service;

        public AnalyzeFileServiceTests()
        {
            _service = new AnalyzeFileService(
                _repoMock.Object, 
                _storageMock.Object, 
                _cloudGenMock.Object, 
                _loggerMock.Object);
        }

        [Fact]
        public async Task AnalyzeAsync_Returns_Existing_Analysis_If_Found()
        {
            var fileId = Guid.NewGuid();
            var existing = new FileAnalysisResult
            {
                FileId = fileId,
                PlagiarismPercent = 42,
                Location = "/clouds/wordcloud.png"
            };
            _repoMock.Setup(r => r.GetByFileIdAsync(fileId)).ReturnsAsync(existing);

            var request = new AnalyzeFileRequest { FileId = fileId };
            var result = await _service.AnalyzeAsync(request);

            Assert.Equal(existing.FileId, result.FileId);
            Assert.Equal(existing.PlagiarismPercent, result.PlagiarismPercent);
            Assert.Equal(existing.Location, result.WordCloudPath);
            _storageMock.Verify(s => s.GetFileAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AnalyzeAsync_Throws_FileNotFound_If_Storage_Missing()
        {
            var fileId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByFileIdAsync(fileId)).ReturnsAsync((FileAnalysisResult)null);
            _storageMock.Setup(s => s.GetFileAsync(fileId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException());

            var request = new AnalyzeFileRequest { FileId = fileId };

            await Assert.ThrowsAsync<FileNotFoundException>(() => _service.AnalyzeAsync(request));
        }

        [Fact]
        public async Task AnalyzeAsync_Analyzes_And_Saves_If_Not_Analyzed()
        {
            var fileId = Guid.NewGuid();
            var fileName = "test.txt";
            var fileContent = Encoding.UTF8.GetBytes("test content");
            var cloudPath = "/clouds/wordcloud.png";

            _repoMock.Setup(r => r.GetByFileIdAsync(fileId)).ReturnsAsync((FileAnalysisResult)null);
            _storageMock.Setup(s => s.GetFileAsync(fileId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((fileContent, fileName));
            _cloudGenMock.Setup(g => g.GenerateWordCloudAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(cloudPath);

            FileAnalysisResult savedResult = null;
            _repoMock.Setup(r => r.SaveAsync(It.IsAny<FileAnalysisResult>()))
                .Callback<FileAnalysisResult>(r => savedResult = r)
                .Returns(Task.CompletedTask);

            var request = new AnalyzeFileRequest { FileId = fileId };
            var result = await _service.AnalyzeAsync(request);

            Assert.Equal(fileId, result.FileId);
            Assert.InRange(result.PlagiarismPercent, 0, 60);
            Assert.Equal(cloudPath, result.WordCloudPath);

            Assert.NotNull(savedResult);
            Assert.Equal(fileId, savedResult.FileId);
            Assert.Equal(fileName, savedResult.FileName);
            Assert.Equal(cloudPath, savedResult.Location);
            Assert.InRange(savedResult.PlagiarismPercent, 0, 60);
            Assert.True((DateTime.UtcNow - savedResult.AnalyzedAt).TotalSeconds < 10);
        }
    }
}