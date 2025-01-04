using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.NetWork;

public sealed class HttpRequest(string pluginName)
{
    private readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private readonly CommandLogger _httpRequestLogger = new(pluginName, "HttpRequest");

    public async Task<string> Post([StringSyntax("Uri")] string url, object? content = null)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync(url, content == null
                ? null
                : new StringContent(
                    JsonSerializer.Serialize(content),
                    Encoding.UTF8,
                    "application/json"
                ));
            return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : "";
        }
        catch (TaskCanceledException)
        {
            _httpRequestLogger.Error("Task Canceled: ");
            return "";
        }
        catch (Exception ex)
        {
            PrintExceptionInfo(ex);
            return "";
        }
    }

    public async Task<string> Get([StringSyntax("Uri")] string url)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : "";
        }
        catch (TaskCanceledException)
        {
            _httpRequestLogger.Error("Task Canceled: ");
            return "";
        }
        catch (Exception ex)
        {
            PrintExceptionInfo(ex);
            return "";
        }
    }

    public async Task<byte[]> GetBinary([StringSyntax("Uri")] string url)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            return response.IsSuccessStatusCode ? await response.Content.ReadAsByteArrayAsync() : [];
        }
        catch (TaskCanceledException)
        {
            _httpRequestLogger.Error("Task Canceled: ");
            return [];
        }
        catch (Exception ex)
        {
            PrintExceptionInfo(ex);
            return [];
        }
    }

    private void PrintExceptionInfo(Exception ex)
    {
        _httpRequestLogger.Error(ex, "Error Occured, Error Information:");
    }
}