namespace FileAnalysisService.Domain.Interfaces;

public interface IWordCloudGenerator
{
    Task<string> GenerateWordCloudAsync(string text, string savePath);
}