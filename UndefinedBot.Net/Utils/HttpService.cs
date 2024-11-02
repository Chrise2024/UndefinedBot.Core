using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;

namespace UndefinedBot.Core.NetWork
{
    internal class HttpServer
    {

        private readonly HttpListener _httpListener = new();

        private readonly Logger _httpServerLogger = new("HttpServer");

        public HttpServer(string Prefixe)
        {
            _httpListener.Prefixes.Add(Prefixe);
        }
        public async Task Start()
        {
            _httpListener.Start();
            _httpServerLogger.Info("Http Server Started");
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
                string TempString = sr.ReadToEnd().Replace("\\u",";/.-u").Replace("\\", "-:/&]").Replace(";/.-u","\\u");
                string ReqString = Regex.Unescape(TempString);
                sr.Close();
                context.Response.StatusCode = 200;
                context.Response.Close();
                await CommandHandler.HandleMsg(JsonConvert.DeserializeObject<MsgBodySchematics>(ReqString.Replace("-:/&]", "\\")));
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
