using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.NetWork;

public sealed class HttpRequest(string pluginName, ILogger logger) : IDisposable
{
    private readonly HttpClient _httpClient = new()
    {
        Timeout = DefaultTimeout,
        MaxResponseContentBufferSize = MaxBufferSize
    };

    private readonly ILogger _logger = logger.Extend("HttpRequest");
    private static TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(10);
    private static long MaxBufferSize { get; set; } = 0x10000000;

    internal static void SetConfig(string? timeoutString, string? maxBufferSizeString)
    {
        if (int.TryParse(timeoutString, out int timeoutMs)) DefaultTimeout = TimeSpan.FromMicroseconds(timeoutMs);
        if (int.TryParse(maxBufferSizeString, out int maxBufferSize)) MaxBufferSize = maxBufferSize;
    }

    public async Task<string> PostAsync([StringSyntax("Uri")] string url, object? content = null)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync(url,
                content is null
                    ? null
                    : new StringContent(
                        JsonSerializer.Serialize(content),
                        Encoding.UTF8,
                        "application/json"
                    )
            );
            return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : "";
        }
        catch (TaskCanceledException)
        {
            _logger.Error("Task Canceled: ");
            return "";
        }
        catch (Exception ex)
        {
            PrintExceptionInfo(ex);
            return "";
        }
    }

    public async Task<string?> GetAsync([StringSyntax("Uri")] string url)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : null;
        }
        catch (TaskCanceledException)
        {
            _logger.Error("Http request timeout");
        }
        catch (Exception ex)
        {
            PrintExceptionInfo(ex);
        }

        return null;
    }

    public async Task<byte[]> GetBytesAsync([StringSyntax("Uri")] string url)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            return response.IsSuccessStatusCode ? await response.Content.ReadAsByteArrayAsync() : [];
        }
        catch (TaskCanceledException)
        {
            _logger.Error("Http request timeout");
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
        _logger.Error(ex, "Error Occured, Error Information:");
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}