using System.Collections.Specialized;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Net.NetWork;

internal class HttpServer(
    ILogger<HttpServer> logger
) : BackgroundService
{
    private readonly HttpServiceOptions _options = ConfigManager.GetConfig().HttpServer;

    private readonly ILogger _logger = logger;

    private readonly HttpListener _httpListener = new();

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        string prefix = $"http://{_options.Host}:{_options.Port}/";
        try
        {
            _httpListener.Prefixes.Add(prefix);
            _httpListener.Start();
            _logger.LogInformation("Http Server Started With Prefix: {prefix}",prefix);
        }
        catch (Exception ex)
        {
            _logger.LogCritical("Http Server Start Failed");
            PrintExceptionInfo(ex);
        }

        await ReceiveLoop(token);

        if (_httpListener.IsListening)
        {
            try
            {
                //_httpListener.Stop();
                _httpListener.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError("Http Server Stop Failed");
                PrintExceptionInfo(ex);
                return;
            }
        }
        _logger.LogInformation("Http Server Stopped");
    }
    private async Task ReceiveLoop(CancellationToken token)
    {
        while (_httpListener.IsListening && !token.IsCancellationRequested)
        {
            try
            {
                var context = await _httpListener.GetContextAsync().WaitAsync(token);
                _ = HandleRequestAsync(context,token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                PrintExceptionInfo(ex);
            }
        }
    }

    private async Task HandleRequestAsync(HttpListenerContext context,CancellationToken token = default)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;
        NameValueCollection query = request.QueryString;
        try
        {
            if (!string.IsNullOrEmpty(_options.AccessToken))
            {
                string? authorization = request.Headers.Get("Authorization") ??
                                        (query["access_token"] is { } accessToken ? $"Bearer {accessToken}" : null);
                if (authorization is null)
                {
                    _logger.LogError("Unauthorized Request");
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Headers.Add("WWW-Authenticate", "Bearer");
                    response.Close();
                    return;
                }

                if (authorization != $"Bearer {_options.AccessToken}")
                {
                    _logger.LogError("Authorization Failed");
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    response.Close();
                    return;
                }
            }

            if (request.HttpMethod != "POST")
            {
                _logger.LogError("Invalid Http Method");
                response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                response.Close();
                return;
            }

            if (!MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaType))
            {
                _logger.LogError("Unknown Media Type");
                response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
                response.Close();
                return;
            }

            if (mediaType.MediaType != "application/json")
            {
                _logger.LogError("Unsupported Content Type");
                response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                response.Close();
                return;
            }

            StreamReader sr = new(context.Request.InputStream);
            string tempString = await sr.ReadToEndAsync(token);
            string reqString = Regex.Unescape(tempString);//.Replace(@"\u0022", "\\\"").Replace(@"\0", "0");
            sr.Close();
            context.Response.StatusCode = 200;
            context.Response.Close();
            CommandHandler.HandleMsg(JsonNode.Parse(reqString) ?? throw new Exception("Parse Failed"));
        }
        catch (Exception ex)
        {
            PrintExceptionInfo(ex);
        }
    }
    private void PrintExceptionInfo(Exception ex)
    {
        _logger.LogError("Error Occured, Error Information:");
        _logger.LogError(ex.Message);
        _logger.LogError(ex.StackTrace ?? "");
    }
}
