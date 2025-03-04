using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.NetWork;

public sealed class HttpRequest : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(10);
    private readonly long _maxBufferSize = 0x10000000;
    private readonly ILogger _logger;

    public HttpRequest(ILogger logger)
    {
        _logger = logger.Extend("HttpRequest");
        if (int.TryParse(Shared.RootConfig["HttpRequest:TimeoutMS"], out int timeoutMs))
            _defaultTimeout = TimeSpan.FromMicroseconds(timeoutMs);
        if (int.TryParse(Shared.RootConfig["HttpRequest:MaxBufferSizeByte"], out int maxBufferSize))
            _maxBufferSize = maxBufferSize;
        _httpClient = new()
        {
            Timeout = _defaultTimeout,
            MaxResponseContentBufferSize = _maxBufferSize
        };
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
        _logger.Error(ex, "Error occured, error information:");
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}