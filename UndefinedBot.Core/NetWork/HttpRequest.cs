using System.Text;
using Newtonsoft.Json;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.NetWork;

public class HttpRequest
{
    private readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private readonly GeneralLogger _httpRequestLogger = new("HttpRequest");

    public async Task<string> Post(string url, object? content = null)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync(url, content == null ? null : new StringContent(
                JsonConvert.SerializeObject(content),
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
    public async Task<string> Get(string url)
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

    public async Task<byte[]> GetBinary(string url)
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
        _httpRequestLogger.Error("Error Occured, Error Information:");
        _httpRequestLogger.Error(ex.Message);
        _httpRequestLogger.Error(ex.StackTrace ?? "");
    }
}
