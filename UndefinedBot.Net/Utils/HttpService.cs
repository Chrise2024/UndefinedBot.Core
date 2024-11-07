using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
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
            _httpServerLogger.Info("Listener", "Http Server Started");
            while (_httpListener.IsListening)
            {
                try
                {
                    var context = await _httpListener.GetContextAsync().WaitAsync(new CancellationToken());
                    _ = HandleRequestAsync(context);
                    //catch { }
                }
                catch(Exception ex)
                {
                    _httpServerLogger.Error("Listener", "Error Occured, Error Information:");
                    _httpServerLogger.Error("Listener", ex.Message);
                    _httpServerLogger.Error("Listener", ex.StackTrace ?? "");
                }
            }
        }
        public void Stop()
        {
            _httpServerLogger.Info("Listener", "Http Server Stopped");
            _httpListener.Stop();
            _httpListener.Close();
        }
        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            try
            {
                StreamReader sr = new(context.Request.InputStream);
                string tempString = sr.ReadToEnd().Replace("\\u",";/.-u").Replace("\\", "-:/&]").Replace(";/.-u","\\u");
                string reqString = Regex.Unescape(tempString);
                sr.Close();
                context.Response.StatusCode = 200;
                context.Response.Close();
                await CommandHandler.HandleMsg(JsonConvert.DeserializeObject<MsgBodySchematics>(reqString.Replace("-:/&]", "\\")));
            }
            catch (Exception ex)
            {
                _httpServerLogger.Error("Listener", "Error Occured, Error Information:");
                _httpServerLogger.Error("Listener", ex.Message);
                _httpServerLogger.Error("Listener", ex.StackTrace ?? "");
            }
        }
    }
}
