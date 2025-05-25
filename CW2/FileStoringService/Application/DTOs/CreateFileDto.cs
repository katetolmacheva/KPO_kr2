namespace FileStoringService.Application.DTOs;

public class CreateFileDto
{
    public string Content { get; set; } = null!;
    public string FileName { get; set; } = null!;
}