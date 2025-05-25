using FileStoringService.Application.DTOs;
using FileStoringService.Application.Interfaces;

namespace FileAnalysisService.Infrastructure.Clients;

public class FileStorageHttpClient : IFileStoringService
{
    private readonly HttpClient _httpClient;

    public FileStorageHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<FileDto> SaveFileAsync(string sourceFilePath, CancellationToken ct = default)
    {
        using var multipart = new MultipartFormDataContent();
        using var fileStream = System.IO.File.OpenRead(sourceFilePath);
        multipart.Add(new StreamContent(fileStream), "file", System.IO.Path.GetFileName(sourceFilePath));

        var response = await _httpClient.PostAsync("", multipart, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FileDto>(cancellationToken: ct)!;
    }

    public async Task<(byte[] Content, string Name)> GetFileAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"/api/files/{id}", ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            throw new FileNotFoundException();

        var content = await response.Content.ReadAsByteArrayAsync(ct);
        var name = response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                   ?? throw new InvalidOperationException("No filename in response headers");
        return (content, name);
    }

    public async Task<IEnumerable<FileDto>> GetAllFilesMetadataAsync(CancellationToken ct = default)
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<FileDto>>("", ct) ?? Array.Empty<FileDto>();
    }

    public async Task<bool> DeleteFileAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync($"/{id}", ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<FileDto> GetFileMetadataAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"/{id}/metadata", ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            throw new FileNotFoundException();

        return await response.Content.ReadFromJsonAsync<FileDto>(cancellationToken: ct)!;
    }
}
