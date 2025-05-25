using FileAnalysisService.Domain.Entities;

namespace FileAnalysisTest.Domain.Entities
{
    public class FileAnalysisResultTests
    {
        [Fact]
        public void Can_Create_And_Set_Properties()
        {
            var id = Guid.NewGuid();
            var name = "test.txt";
            var location = "/files/test.txt";
            var percent = 42;
            var analyzedAt = DateTime.UtcNow;

            var result = new FileAnalysisResult
            {
                FileId = id,
                FileName = name,
                Location = location,
                PlagiarismPercent = percent,
                AnalyzedAt = analyzedAt
            };

            Assert.Equal(id, result.FileId);
            Assert.Equal(name, result.FileName);
            Assert.Equal(location, result.Location);
            Assert.Equal(percent, result.PlagiarismPercent);
            Assert.Equal(analyzedAt, result.AnalyzedAt);
        }

        [Fact]
        public void Default_Constructor_Initializes_Properties_To_Defaults()
        {
            var result = new FileAnalysisResult();

            Assert.Equal(default, result.FileId);
            Assert.Null(result.FileName);
            Assert.Null(result.Location);
            Assert.Equal(0, result.PlagiarismPercent);
            Assert.Equal(default, result.AnalyzedAt);
        }
    }
}