namespace FileAnalysisService.Application.DTOs;

public class AnalyzeFileResponse
{
    public Guid FileId { get; set; }
    public string FileName { get; set; }           
    public int ParagraphCount { get; set; }        
    public int WordCount { get; set; }             
    public int CharacterCount { get; set; }        
    public int PlagiarismPercent { get; set; }
    public string WordCloudPath { get; set; }

}