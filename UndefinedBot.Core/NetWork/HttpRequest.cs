﻿using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.NetWork;

public sealed class HttpRequest(string pluginName) : IDisposable
{
    private readonly HttpClient _httpClient = new()
    {
        Timeout = DefaultTimeout,
        MaxResponseContentBufferSize = MaxBufferSize
    };
    private static TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(10);
    private static long MaxBufferSize { get; set; } = 0x80000000;
    
    internal static void SetConfig(string? timeoutString,string? maxBufferSizeString)
    {
        if (int.TryParse(timeoutString, out int timeoutMs))
        {
            DefaultTimeout = TimeSpan.FromMicroseconds(timeoutMs);
        }
        if (int.TryParse(maxBufferSizeString, out int maxBufferSize))
        {
            MaxBufferSize = maxBufferSize;
        }
    }

    private FixedLogger HttpRequestLogger => new (["Network",pluginName, "HttpRequest"]);

    public async Task<string> Post([StringSyntax("Uri")] string url, object? content = null)
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
            HttpRequestLogger.Error("Task Canceled: ");
            return "";
        }
        catch (Exception ex)
        {
            PrintExceptionInfo(ex);
            return "";
        }
    }

    public async Task<string?> Get([StringSyntax("Uri")] string url)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : null;
        }
        catch (TaskCanceledException)
        {
            HttpRequestLogger.Error("Http request timeout");
        }
        catch (Exception ex)
        {
            PrintExceptionInfo(ex);
        }

        return null;
    }

    public async Task<byte[]> GetBytes([StringSyntax("Uri")] string url)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            return response.IsSuccessStatusCode ? await response.Content.ReadAsByteArrayAsync() : [];
        }
        catch (TaskCanceledException)
        {
            HttpRequestLogger.Error("Http request timeout");
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
        HttpRequestLogger.Error(ex, "Error Occured, Error Information:");
    }
    public void Dispose()
    {
        _httpClient.Dispose();
    }
}