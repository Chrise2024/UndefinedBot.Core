﻿using System.Collections.Specialized;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.CommandSource;
using UndefinedBot.Core.Utils.Logging;

namespace Adapter.OneBot11;

public sealed class HttpServiceOptions(string host, uint port, string? accessToken = null)
{
    public static HttpServiceOptions CreateFromConfig(AdapterConfigData adapterConfig)
    {
        JsonNode? serverConfig = adapterConfig.OriginalConfig["Server"];
        if (serverConfig is null)
        {
            throw new Exception("Server Properties Not Implemented");
        }

        return serverConfig.Deserialize<HttpServiceOptions>() ?? throw new Exception("Invalid Server Properties");
    }

    public static HttpServiceOptions Default()
    {
        return new HttpServiceOptions("", 16384);
    }

    public string Host { get; } = host;
    public uint Port { get; } = port;
    public string? AccessToken { get; } = accessToken;
}

internal sealed class HttpServer(
    AdapterConfigData adapterConfig,
    Action<CommandInformation, BaseCommandSource, ParsedToken[]> submitter,
    AdapterLogger parentLogger
    )
{
    private readonly HttpListener _httpListener = new();
    private readonly HttpServiceOptions _options = HttpServiceOptions.CreateFromConfig(adapterConfig);
    private readonly MsgHandler _handler = new(adapterConfig,parentLogger);
    private AdapterLogger Logger => parentLogger.Extend("HttpServer");
    private Action<CommandInformation, BaseCommandSource, ParsedToken[]> Submitter => submitter;

    public async Task ExecuteAsync(CancellationToken token)
    {
        string prefix = $"http://{_options.Host}:{_options.Port}/";
        try
        {
            _httpListener.Prefixes.Add(prefix);
            _httpListener.Start();
            Logger.Info($"Http Server Started With Prefix: {prefix}");
        }
        catch (Exception ex)
        {
            Logger.Error("Http Server Start Failed");
            PrintExceptionInfo(ex);
        }

        await ReceiveLoop(token);

        if (_httpListener.IsListening)
        {
            try
            {
                _httpListener.Stop();
                _httpListener.Close();
            }
            catch (Exception ex)
            {
                Logger.Error("Http Server Stop Failed");
                PrintExceptionInfo(ex);
                return;
            }
        }
        Logger.Info("Http Server Stopped");
    }

    private async Task ReceiveLoop(CancellationToken token)
    {
        Logger.Info("Http Server Receive Loop Started");
        while (_httpListener.IsListening && !token.IsCancellationRequested)
        {
            try
            {
                var context = await _httpListener.GetContextAsync().WaitAsync(token);
                _ = HandleRequestAsync(context, token);
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
        Logger.Info("Http Server Receive Loop Ended");
    }

    private async Task HandleRequestAsync(HttpListenerContext context, CancellationToken token = default)
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
                    Logger.Error("Unauthorized Request");
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Headers.Add("WWW-Authenticate", "Bearer");
                    response.Close();
                    return;
                }

                if (authorization != $"Bearer {_options.AccessToken}")
                {
                    Logger.Error("Authorization Failed");
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    response.Close();
                    return;
                }
            }

            if (request.HttpMethod != "POST")
            {
                Logger.Error("Invalid Http Method");
                response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                response.Close();
                return;
            }

            if (!MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaType))
            {
                Logger.Error("Unknown Media Type");
                response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
                response.Close();
                return;
            }

            if (mediaType.MediaType != "application/json")
            {
                Logger.Error("Unsupported Content Type");
                response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                response.Close();
                return;
            }

            StreamReader sr = new(context.Request.InputStream);
            string tempString = await sr.ReadToEndAsync(token);
            string reqString = Regex.Unescape(tempString); //.Replace(@"\u0022", "\\\"").Replace(@"\0", "0");
            sr.Close();
            context.Response.StatusCode = 200;
            context.Response.Close();
            (CommandInformation? cip, BaseCommandSource? ucs, ParsedToken[]? tokens) =
                _handler.HandleMsg(JsonNode.Parse(reqString) ?? throw new Exception("Parse Failed"));
            if (cip is not null && ucs is not null && tokens is not null)
            {
                Submitter(cip, ucs, tokens);
                Logger.Info("Handle Complete");
            }
        }
        catch (Exception ex)
        {
            PrintExceptionInfo(ex);
        }
    }

    private void PrintExceptionInfo(Exception ex)
    {
        Logger.Error(ex, "Error Occured, Error Information:");
    }
}