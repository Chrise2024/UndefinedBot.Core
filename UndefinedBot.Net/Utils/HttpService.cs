using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Net.Utils
{
    internal class HttpServer
    {

        private readonly HttpListener _httpListener = new();

        private readonly Logger _httpServerLogger = new("HttpServer");

        public HttpServer(string prefix)
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
                    HttpListenerContext context = await _httpListener.GetContextAsync().WaitAsync(new CancellationToken());
                    _ = HandleRequestAsync(context);
                    //catch { }
                }
                catch(Exception ex)
                {
                    _httpServerLogger.Error("Error Occured, Error Information:");
                    _httpServerLogger.Error(ex.Message);
                    _httpServerLogger.Error(ex.StackTrace ?? "");
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
                string tempString = await sr.ReadToEndAsync();//.Replace("\\u",";/.-u").Replace("\\", "-:/&]").Replace(";/.-u","\\u");
                string reqString = Regex.Unescape(tempString);
                sr.Close();
                context.Response.StatusCode = 200;
                context.Response.Close();
                await CommandHandler.HandleMsg(JObject.Parse(reqString/*.Replace("-:/&]", "\\")*/));
            }
            catch (Exception ex)
            {
                _httpServerLogger.Error("Error Occured, Error Information:");
                _httpServerLogger.Error(ex.Message);
                _httpServerLogger.Error(ex.StackTrace ?? "");
            }
        }
    }
}
