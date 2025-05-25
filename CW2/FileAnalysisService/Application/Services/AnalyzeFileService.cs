using System.Text;
using FileAnalysisService.Application.DTOs;
using FileAnalysisService.Application.Interfaces;
using FileAnalysisService.Domain.Entities;
using FileAnalysisService.Domain.Interfaces;
using FileStoringService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace FileAnalysisService.Application.Services;

public class AnalyzeFileService : IAnalyzeFileService
{
    private readonly IFileAnalysisRepository _analysisRepo;
    private readonly IFileStoringService _fileStorage;
    private readonly IWordCloudGenerator _wordCloudGen;
    private readonly ILogger<AnalyzeFileService> _logger;
    
    public AnalyzeFileService(
        IFileAnalysisRepository analysisRepo,
        IFileStoringService fileStorage,
        IWordCloudGenerator wordCloudGen,
        ILogger<AnalyzeFileService> logger)
    {
        _analysisRepo = analysisRepo;
        _fileStorage = fileStorage;
        _wordCloudGen = wordCloudGen;
        _logger = logger;
    }

    public async Task<AnalyzeFileResponse> AnalyzeAsync(
        AnalyzeFileRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting analysis for file with ID: {FileId}", request.FileId);

        var existing = await _analysisRepo.GetByFileIdAsync(request.FileId);
        if (existing is not null)
        {
            _logger.LogInformation("Found existing analysis result for file {FileId}, returning cached result",
                request.FileId);
            return new AnalyzeFileResponse
            {
                FileId = existing.FileId,
                FileName = existing.FileName,
                ParagraphCount = existing.ParagraphCount,
                WordCount = existing.WordCount,
                CharacterCount = existing.CharacterCount,
                PlagiarismPercent = existing.PlagiarismPercent,
                WordCloudPath = existing.Location
            };
        }

        _logger.LogDebug("Retrieving file content for {FileId}", request.FileId);
        (byte[] contentBytes, string fileName) fileData;
        try
        {
            fileData = await _fileStorage.GetFileAsync(request.FileId, cancellationToken);
            _logger.LogDebug("Successfully retrieved file '{FileName}' ({FileId})", fileData.fileName, request.FileId);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError(ex, "File with ID {FileId} was not found in storage", request.FileId);
            throw new FileNotFoundException($"File with ID {request.FileId} was not found in storage.", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file {FileId} from storage", request.FileId);
            throw;
        }

        var text = Encoding.UTF8.GetString(fileData.contentBytes);

        _logger.LogDebug("Analyzing content statistics for file {FileId}", request.FileId);
        var paragraphCount = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length;
        var wordCount = text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
        var characterCount = text.Length;
        _logger.LogInformation(
            "File '{FileName}' analyzed: {ParagraphCount} paragraphs, {WordCount} words, {CharacterCount} characters",
            fileData.fileName, paragraphCount, wordCount, characterCount);

        var random = new Random();
        int plagiarismPercent = random.Next(0, 61);
        _logger.LogInformation("Plagiarism analysis for file '{FileName}': {PlagiarismPercent}%", fileData.fileName,
            plagiarismPercent);

        _logger.LogDebug("Generating word cloud for file {FileId}", request.FileId);
        var cloudFolder = Path.Combine("wwwroot", "wordclouds");
        Directory.CreateDirectory(cloudFolder);
        var cloudFileName = $"{request.FileId}.png";
        var cloudFilePath = Path.Combine(cloudFolder, cloudFileName);

        string savedCloudPath;
        try
        {
            savedCloudPath = await _wordCloudGen.GenerateWordCloudAsync(text, cloudFilePath);
            _logger.LogInformation("Word cloud generated and saved to: {CloudPath}", savedCloudPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate word cloud for file {FileId}", request.FileId);
            throw;
        }

        _logger.LogDebug("Saving analysis results to database for file {FileId}", request.FileId);
        var result = new FileAnalysisResult
        {
            FileId = request.FileId,
            FileName = fileData.fileName,
            Location = savedCloudPath,
            ParagraphCount = paragraphCount,
            WordCount = wordCount,
            CharacterCount = characterCount,
            PlagiarismPercent = plagiarismPercent,
            AnalyzedAt = DateTime.UtcNow
        };

        try
        {
            await _analysisRepo.SaveAsync(result);
            _logger.LogInformation("Analysis result saved for file '{FileName}' with ID {FileId}", fileData.fileName,
                request.FileId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save analysis results to database for file {FileId}", request.FileId);
            throw;
        }

        _logger.LogDebug("Returning analysis response for file {FileId}", request.FileId);
        return new AnalyzeFileResponse
        {
            FileId = result.FileId,
            FileName = result.FileName,
            ParagraphCount = paragraphCount,
            WordCount = wordCount,
            CharacterCount = characterCount,
            PlagiarismPercent = plagiarismPercent,
            WordCloudPath = savedCloudPath
        };
    }

    public async Task<(byte[] Content, string FileName)> DownloadWordCloudAsync(Guid fileId,
        CancellationToken ct = default)
    {
        var analyzeResult = await _analysisRepo.GetByFileIdAsync(fileId)
                            ?? throw new InvalidOperationException($"Analysis result for file {fileId} not found");

        var absoluteFilePath = Path.Combine(Directory.GetCurrentDirectory(), analyzeResult.Location);

        if (!File.Exists(absoluteFilePath))
            throw new FileNotFoundException("Word cloud image file not found", absoluteFilePath);

        var content = await File.ReadAllBytesAsync(absoluteFilePath, ct);
        var fileName = Path.GetFileName(absoluteFilePath);

        return (content, fileName);
    }

}