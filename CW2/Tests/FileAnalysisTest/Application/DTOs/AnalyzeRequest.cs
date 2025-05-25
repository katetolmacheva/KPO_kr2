using FileAnalysisService.Application.DTOs;

namespace FileAnalysisTest.Application.DTOs
{
    public class AnalyzeFileRequestTests
    {
        [Fact]
        public void Can_Set_And_Get_FileId()
        {
            var id = Guid.NewGuid();
            var request = new AnalyzeFileRequest { FileId = id };
            Assert.Equal(id, request.FileId);
        }

        [Fact]
        public void Default_FileId_Is_Empty()
        {
            var request = new AnalyzeFileRequest();
            Assert.Equal(Guid.Empty, request.FileId);
        }
    }
}