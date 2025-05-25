using FileAnalysisService.Application.DTOs;

namespace FileAnalysisTest.Application.DTOs
{
    public class AnalyzeFileResponseTests
    {
        [Fact]
        public void Can_Set_And_Get_Properties()
        {
            var id = Guid.NewGuid();
            var percent = 85;
            var path = "/clouds/wordcloud.png";

            var response = new AnalyzeFileResponse
            {
                FileId = id,
                PlagiarismPercent = percent,
                WordCloudPath = path
            };

            Assert.Equal(id, response.FileId);
            Assert.Equal(percent, response.PlagiarismPercent);
            Assert.Equal(path, response.WordCloudPath);
        }

        [Fact]
        public void Default_Constructor_Initializes_Properties_To_Defaults()
        {
            var response = new AnalyzeFileResponse();

            Assert.Equal(Guid.Empty, response.FileId);
            Assert.Equal(0, response.PlagiarismPercent);
            Assert.Null(response.WordCloudPath);
        }
    }
}