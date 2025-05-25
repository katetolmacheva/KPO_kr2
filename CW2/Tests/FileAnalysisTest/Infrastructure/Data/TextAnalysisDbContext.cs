using FileAnalysisService.Domain.Entities;
using FileAnalysisService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FileAnalysisTest.Infrastructure.Data
{
    public class TextAnalysisDbContextTests
    {
        private DbContextOptions<TextAnalysisDbContext> CreateOptions()
        {
            return new DbContextOptionsBuilder<TextAnalysisDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public void Can_Create_DbContext_And_Set_And_Get_Entity()
        {
            var options = CreateOptions();
            var fileId = Guid.NewGuid();
            var entity = new FileAnalysisResult
            {
                FileId = fileId,
                FileName = "file.txt",
                Location = "/files/file.txt",
                PlagiarismPercent = 50,
                AnalyzedAt = DateTime.UtcNow
            };

            using (var context = new TextAnalysisDbContext(options))
            {
                context.FileAnalysisResults.Add(entity);
                context.SaveChanges();
            }

            using (var context = new TextAnalysisDbContext(options))
            {
                var loaded = context.FileAnalysisResults.Single(e => e.FileId == fileId);
                Assert.Equal(entity.FileName, loaded.FileName);
                Assert.Equal(entity.Location, loaded.Location);
                Assert.Equal(entity.PlagiarismPercent, loaded.PlagiarismPercent);
                Assert.Equal(entity.AnalyzedAt, loaded.AnalyzedAt);
            }
        }

        [Fact]
        public void Model_Has_Expected_Configuration()
        {
            var options = CreateOptions();
            using var context = new TextAnalysisDbContext(options);
            var model = context.Model.FindEntityType(typeof(FileAnalysisResult));
            Assert.NotNull(model);
            Assert.Equal("file_analysis_results", model.GetTableName());
            Assert.Equal("file_id", model.FindProperty(nameof(FileAnalysisResult.FileId)).GetColumnName());
            Assert.Equal("file_name", model.FindProperty(nameof(FileAnalysisResult.FileName)).GetColumnName());
            Assert.Equal("location", model.FindProperty(nameof(FileAnalysisResult.Location)).GetColumnName());
            Assert.Equal("plagiarism_percent", model.FindProperty(nameof(FileAnalysisResult.PlagiarismPercent)).GetColumnName());
            Assert.Equal("analyzed_at", model.FindProperty(nameof(FileAnalysisResult.AnalyzedAt)).GetColumnName());
        }
    }
}