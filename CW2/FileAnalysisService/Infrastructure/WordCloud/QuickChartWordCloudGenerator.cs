using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FileAnalysisService.Domain.Interfaces;

namespace FileAnalysisService.Infrastructure.WordCloud;

public class QuickChartWordCloudGenerator : IWordCloudGenerator
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;

    public QuickChartWordCloudGenerator(HttpClient http)
    {
        _http = http;
        _baseUrl = "https://quickchart.io/wordcloud";
    }

    public async Task<string> GenerateWordCloudAsync(string text, string savePath)
    {
        using var httpClient = new HttpClient();
        var payload = new
        {
            text = text,
            format = "png",
            width = 800,
            height = 600,
            fontFamily = "Arial",
            fontWeight = "bold",
            backgroundColor = "#ffffff",
            colors = new[] { "#1f77b4", "#ff7f0e", "#2ca02c" },
            fontScale = 15,
            scale = "linear",
            removeStopwords = true,
            minWordLength = 4,
            maxNumWords = 100,
            rotation = 0,
            padding = 5,
            language = "en"
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("https://quickchart.io/wordcloud", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"QuickChart error: {response.StatusCode}, {error}");
        }

        var bytes = await response.Content.ReadAsByteArrayAsync();
        await File.WriteAllBytesAsync(savePath, bytes);

        return savePath;
    }
}