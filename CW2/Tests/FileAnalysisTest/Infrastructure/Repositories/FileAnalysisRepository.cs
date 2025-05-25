using FileAnalysisService.Domain.Entities;
using FileAnalysisService.Infrastructure.Data;
using FileAnalysisService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FileAnalysisTest.Infrastructure.Repositories
{
    public class FileAnalysisRepositoryTests
    {
        private TextAnalysisDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<TextAnalysisDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new TextAnalysisDbContext(options);
        }

        [Fact]
        public async Task GetByFileIdAsync_Returns_Entity_If_Exists()
        {
            var ctx = CreateContext();
            var fileId = Guid.NewGuid();
            var entity = new FileAnalysisResult
            {
                FileId = fileId,
                FileName = "file.txt",
                Location = "/files/file.txt",
                PlagiarismPercent = 10,
                AnalyzedAt = DateTime.UtcNow
            };
            ctx.FileAnalysisResults.Add(entity);
            ctx.SaveChanges();

            var repo = new FileAnalysisRepository(ctx);
            var result = await repo.GetByFileIdAsync(fileId);

            Assert.NotNull(result);
            Assert.Equal(fileId, result.FileId);
        }

        [Fact]
        public async Task GetByFileIdAsync_Returns_Null_If_Not_Exists()
        {
            var ctx = CreateContext();
            var repo = new FileAnalysisRepository(ctx);

            var result = await repo.GetByFileIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task SaveAsync_Adds_New_Entity_If_Not_Exists()
        {
            var ctx = CreateContext();
            var repo = new FileAnalysisRepository(ctx);
            var fileId = Guid.NewGuid();
            var entity = new FileAnalysisResult
            {
                FileId = fileId,
                FileName = "file.txt",
                Location = "/files/file.txt",
                PlagiarismPercent = 20,
                AnalyzedAt = DateTime.UtcNow
            };

            await repo.SaveAsync(entity);

            var loaded = await ctx.FileAnalysisResults.FindAsync(fileId);
            Assert.NotNull(loaded);
            Assert.Equal(entity.FileName, loaded.FileName);
        }

        [Fact]
        public async Task SaveAsync_Updates_Entity_If_Exists()
        {
            var ctx = CreateContext();
            var fileId = Guid.NewGuid();
            var entity = new FileAnalysisResult
            {
                FileId = fileId,
                FileName = "file.txt",
                Location = "/files/file.txt",
                PlagiarismPercent = 30,
                AnalyzedAt = DateTime.UtcNow
            };
            ctx.FileAnalysisResults.Add(entity);
            ctx.SaveChanges();

            var repo = new FileAnalysisRepository(ctx);
            entity.PlagiarismPercent = 99;
            await repo.SaveAsync(entity);

            var loaded = await ctx.FileAnalysisResults.FindAsync(fileId);
            Assert.Equal(99, loaded.PlagiarismPercent);
        }
    }
}