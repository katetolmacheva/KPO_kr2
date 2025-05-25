namespace FileStoringService.Application.DTOs;

public class FileDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string Hash { get; init; } = null!;
    public string Location { get; init; } = null!;
    public DateTime UploadDate { get; init; }
}