using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Net.Utils;

internal class HttpServer
{

    private readonly HttpListener _httpListener = new();

    private readonly GeneralLogger _httpServerLogger = new("HttpServer");

    public HttpServer([StringSyntax("Uri")]string prefix)
    {
        _httpListener.Prefixes.Add(prefix);
    }
    public async Task Start()
    {
        _httpListener.Start();
        _httpServerLogger.Info("Http Server Started");
        while (_httpListener.IsListening)
        {
            try
            {
                HttpListenerContext context = await _httpListener.GetContextAsync();
                _ = HandleRequestAsync(context);
            }
            catch(Exception ex)
            {
                PrintExceptionInfo(ex);
            }
        }
    }
    public void Stop()
    {
        _httpServerLogger.Info("Http Server Stopped");
        _httpListener.Stop();
        _httpListener.Close();
    }
    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        try
        {
            StreamReader sr = new(context.Request.InputStream);
            string tempString = await sr.ReadToEndAsync();
            string reqString = Regex.Unescape(tempString);
            sr.Close();
            context.Response.StatusCode = 200;
            context.Response.Close();
            await CommandHandler.HandleMsg(JsonNode.Parse(reqString) ?? throw new Exception("Parse Failed"));
        }
        catch (Exception ex)
        {
            PrintExceptionInfo(ex);
        }
    }
    private void PrintExceptionInfo(Exception ex)
    {
        _httpServerLogger.Error("Error Occured, Error Information:");
        _httpServerLogger.Error(ex.Message);
        _httpServerLogger.Error(ex.StackTrace ?? "");
    }
}
