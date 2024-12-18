// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.CommandSource;
using UndefinedBot.Core.Utils;

namespace Adapter.OneBot11;

public class HttpServiceOptions
{
    public static HttpServiceOptions Create(AdapterConfigData adapterConfig)
    {
        JsonNode? serverConfig = adapterConfig.OriginalConfig["Server"];
        if (serverConfig == null)
        {
            throw new NotImplementedException();
        }

        return serverConfig.Deserialize<HttpServiceOptions>() ?? throw new NotImplementedException();
    }

    public static HttpServiceOptions Default()
    {
        return new HttpServiceOptions("", 16384);
    }
    private HttpServiceOptions(string host,uint port,string? accessToken = null)
    {
        Host = host;
        Port = port;
        AccessToken = accessToken;
    }
    public string Host { get; }
    public uint Port { get; }
    public string? AccessToken { get; }
}

internal class HttpServer(AdapterLogger logger,AdapterConfigData adapterConfig,Action<PrimeInvokeProperties,BaseCommandSource,List<ParsedToken>> submitter)
{

    private readonly HttpListener _httpListener = new();
    private readonly HttpServiceOptions _options = HttpServiceOptions.Create(adapterConfig);
    private readonly MsgHandler _handler = new(logger, adapterConfig);
    private AdapterLogger Logger => logger;
    private Action<PrimeInvokeProperties, BaseCommandSource, List<ParsedToken>> Submitter => submitter;
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
                //_httpListener.Stop();
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
        while (_httpListener.IsListening)
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
            string reqString = Regex.Unescape(tempString);//.Replace(@"\u0022", "\\\"").Replace(@"\0", "0");
            sr.Close();
            context.Response.StatusCode = 200;
            context.Response.Close();
            (PrimeInvokeProperties? pip,BaseCommandSource? ucs, List<ParsedToken>? tokens) = _handler.HandleMsg(JsonNode.Parse(reqString) ?? throw new Exception("Parse Failed"));
            if (pip != null && ucs != null && tokens != null)
            {
                Submitter(pip, ucs, tokens);
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
        Logger.Error("Error Occured, Error Information:");
        Logger.Error(ex.Message);
        Logger.Error(ex.StackTrace ?? "");
    }
}
