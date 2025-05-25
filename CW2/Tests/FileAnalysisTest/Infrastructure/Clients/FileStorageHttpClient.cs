using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FileAnalysisService.Infrastructure.Clients;
using FileStoringService.Application.DTOs;
using Moq;
using Moq.Protected;

namespace FileAnalysisTest.Infrastructure.Clients
{
    public class FileStorageHttpClientTests
    {
        private static HttpClient CreateHttpClient(HttpResponseMessage response, Action<HttpRequestMessage>? onRequest = null)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage req, CancellationToken ct) =>
                {
                    onRequest?.Invoke(req);
                    return response;
                });
            return new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost") };
        }

        [Fact]
        public async Task SaveFileAsync_Sends_File_And_Returns_Dto()
        {
            var fileDto = new FileDto { Id = Guid.NewGuid(), Name = "test.txt" };
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(fileDto)
            };
            var httpClient = CreateHttpClient(response);

            var tempFile = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempFile, "test content");

            var client = new FileStorageHttpClient(httpClient);
            var result = await client.SaveFileAsync(tempFile);

            Assert.Equal(fileDto.Id, result.Id);
            Assert.Equal(fileDto.Name, result.Name);

            File.Delete(tempFile);
        }

        [Fact]
        public async Task GetFileAsync_Returns_Content_And_Name()
        {
            var fileContent = new byte[] { 1, 2, 3 };
            var fileName = "file.txt";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(fileContent)
            };
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };

            var httpClient = CreateHttpClient(response);
            var client = new FileStorageHttpClient(httpClient);

            var (content, name) = await client.GetFileAsync(Guid.NewGuid());

            Assert.Equal(fileContent, content);
            Assert.Equal(fileName, name);
        }

        [Fact]
        public async Task GetFileAsync_Throws_If_NotFound()
        {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
            var httpClient = CreateHttpClient(response);
            var client = new FileStorageHttpClient(httpClient);

            await Assert.ThrowsAsync<FileNotFoundException>(() => client.GetFileAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetFileAsync_Throws_If_No_Filename()
        {
            var fileContent = new byte[] { 1, 2, 3 };
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(fileContent)
            };

            var httpClient = CreateHttpClient(response);
            var client = new FileStorageHttpClient(httpClient);

            await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetFileAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetAllFilesMetadataAsync_Returns_List()
        {
            var files = new[] { new FileDto { Id = Guid.NewGuid(), Name = "a.txt" } };
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(files)
            };
            var httpClient = CreateHttpClient(response);
            var client = new FileStorageHttpClient(httpClient);

            var result = await client.GetAllFilesMetadataAsync();

            Assert.Single(result);
            Assert.Equal(files[0].Id, ((List<FileDto>)result)[0].Id);
        }

        [Fact]
        public async Task GetAllFilesMetadataAsync_Returns_Empty_If_Null()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json")
            };
            var httpClient = CreateHttpClient(response);
            var client = new FileStorageHttpClient(httpClient);

            var result = await client.GetAllFilesMetadataAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task DeleteFileAsync_Returns_True_On_Success()
        {
            var response = new HttpResponseMessage(HttpStatusCode.NoContent);
            var httpClient = CreateHttpClient(response);
            var client = new FileStorageHttpClient(httpClient);

            var result = await client.DeleteFileAsync(Guid.NewGuid());

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteFileAsync_Returns_False_On_Failure()
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            var httpClient = CreateHttpClient(response);
            var client = new FileStorageHttpClient(httpClient);

            var result = await client.DeleteFileAsync(Guid.NewGuid());

            Assert.False(result);
        }

        [Fact]
        public async Task GetFileMetadataAsync_Returns_Dto()
        {
            var fileDto = new FileDto { Id = Guid.NewGuid(), Name = "meta.txt" };
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(fileDto)
            };
            var httpClient = CreateHttpClient(response);
            var client = new FileStorageHttpClient(httpClient);

            var result = await client.GetFileMetadataAsync(Guid.NewGuid());

            Assert.Equal(fileDto.Id, result.Id);
            Assert.Equal(fileDto.Name, result.Name);
        }

        [Fact]
        public async Task GetFileMetadataAsync_Throws_If_NotFound()
        {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
            var httpClient = CreateHttpClient(response);
            var client = new FileStorageHttpClient(httpClient);

            await Assert.ThrowsAsync<FileNotFoundException>(() => client.GetFileMetadataAsync(Guid.NewGuid()));
        }
    }
}