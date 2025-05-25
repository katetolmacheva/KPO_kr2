using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileAnalysisService.Application.DTOs;
using FileAnalysisService.Application.Interfaces;
using FileAnalysisService.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public class AnalyzeControllerTests
{
    private readonly Mock<IAnalyzeFileService> _serviceMock;
    private readonly AnalyzeController _controller;
    private readonly Mock<ILogger<AnalyzeController>> _loggerMock; 

    public AnalyzeControllerTests()
    {
        _serviceMock = new Mock<IAnalyzeFileService>();
        _loggerMock = new Mock<ILogger<AnalyzeController>>();
        _controller = new AnalyzeController(_serviceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAsync_ReturnsOk_WhenFileExists()
    {
        var fileId = Guid.NewGuid();
        var response = new AnalyzeFileResponse();
        _serviceMock.Setup(s => s.AnalyzeAsync(
                It.IsAny<AnalyzeFileRequest>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(response);

        var result = await _controller.GetAsync(fileId, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task GetAsync_ReturnsNotFound_WhenFileNotFound()
    {
        var fileId = Guid.NewGuid();
        _serviceMock.Setup(s => s.AnalyzeAsync(
                It.IsAny<AnalyzeFileRequest>(),
                It.IsAny<CancellationToken>()
            ))
            .ThrowsAsync(new FileNotFoundException());

        var result = await _controller.GetAsync(fileId, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }
}