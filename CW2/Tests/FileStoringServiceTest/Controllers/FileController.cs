using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileStoringService.Application.DTOs;
using FileStoringService.Application.Interfaces;
using FileStoringService.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FileStoringService.Controllers
{
    public class FilesControllerTests
    {
        private readonly Mock<IFileStoringService> _serviceMock;
        private readonly FilesController _controller;

        public FilesControllerTests()
        {
            _serviceMock = new Mock<IFileStoringService>();
            _controller = new FilesController(_serviceMock.Object);
        }

        [Fact]
        public async Task Upload_Returns_BadRequest_When_File_Is_Null()
        {
            var dto = new FileUploadDto { File = null };
            var result = await _controller.Upload(dto, CancellationToken.None);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Upload_Returns_BadRequest_When_File_Is_Empty()
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(0);
            var dto = new FileUploadDto { File = fileMock.Object };
            var result = await _controller.Upload(dto, CancellationToken.None);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Upload_Returns_CreatedAtAction_When_Successful()
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(10);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var dto = new FileUploadDto { File = fileMock.Object };
            var resultDto = new FileDto { Id = Guid.NewGuid() };

            _serviceMock.Setup(s => s.SaveFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultDto);

            var result = await _controller.Upload(dto, CancellationToken.None);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(FilesController.GetMetadata), created.ActionName);
            Assert.Equal(resultDto, created.Value);
        }

        [Fact]
        public async Task GetAllMetadata_Returns_Ok_With_List()
        {
            var list = new List<FileDto> { new FileDto() };
            _serviceMock.Setup(s => s.GetAllFilesMetadataAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            var result = await _controller.GetAllMetadata(CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(list, ok.Value);
        }

        [Fact]
        public async Task Download_Returns_File_When_Found()
        {
            var id = Guid.NewGuid();
            var content = new byte[] { 1, 2, 3 };
            var name = "file.txt";
            _serviceMock.Setup(s => s.GetFileAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((content, name));

            var result = await _controller.Download(id, CancellationToken.None);

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/octet-stream", fileResult.ContentType);
            Assert.Equal(name, fileResult.FileDownloadName);
            Assert.Equal(content, fileResult.FileContents);
        }

        [Fact]
        public async Task Download_Returns_NotFound_When_KeyNotFound()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetFileAsync(id, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.Download(id, CancellationToken.None);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetMetadata_Returns_Ok_When_Found()
        {
            var id = Guid.NewGuid();
            var meta = new FileDto();
            _serviceMock.Setup(s => s.GetFileMetadataAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(meta);

            var result = await _controller.GetMetadata(id, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(meta, ok.Value);
        }

        [Fact]
        public async Task GetMetadata_Returns_NotFound_When_KeyNotFound()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetFileMetadataAsync(id, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.GetMetadata(id, CancellationToken.None);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Returns_NoContent_When_Removed()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.DeleteFileAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.Delete(id, CancellationToken.None);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_Returns_NotFound_When_Not_Removed()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.DeleteFileAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _controller.Delete(id, CancellationToken.None);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}