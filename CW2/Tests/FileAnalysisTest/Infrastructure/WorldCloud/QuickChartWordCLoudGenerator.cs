using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileAnalysisService.Infrastructure.WordCloud;
using Moq;
using Moq.Protected;
using Xunit;

namespace FileAnalysisTest.Infrastructure.WorldCloud
{
    public class QuickChartWordCloudGeneratorTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly Mock<HttpMessageHandler> _mockHandler;

        public QuickChartWordCloudGeneratorTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);
            _mockHandler = new Mock<HttpMessageHandler>();
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        private HttpClient CreateClient()
        {
            return new HttpClient(_mockHandler.Object);
        }

        private void SetupMockResponse(HttpStatusCode status, byte[] content = null, string expectedText = null)
        {
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.AbsoluteUri == "https://quickchart.io/wordcloud" &&
                        (expectedText == null || req.Content.ReadAsStringAsync().GetAwaiter().GetResult().Contains(expectedText))),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = status,
                    Content = content != null ? new ByteArrayContent(content) : null
                });
        }
        
        [Fact]
        public async Task GenerateWordCloudAsync_ThrowsOnIoError()
        {
            var imagePath = "/root/forbidden.png";
            SetupMockResponse(HttpStatusCode.OK, new byte[] { 1 });

            var generator = new QuickChartWordCloudGenerator(CreateClient());
            await Assert.ThrowsAnyAsync<Exception>(() =>
                generator.GenerateWordCloudAsync("io", imagePath));
        }
    }
}