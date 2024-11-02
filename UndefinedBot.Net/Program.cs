using System.Text;
using UndefinedBot.Core;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.NetWork;
using Newtonsoft.Json;

namespace UndefinedBot.Net
{
    internal class Program
    {
        private static readonly UndefinedAPI s_mainApi = new("Program","Main");

        private static readonly Logger s_mainLogger = new("Program");

        private static readonly string s_programRoot = Environment.CurrentDirectory;

        private static readonly string s_programCahce = Path.Join(s_programRoot, "Cache");

        private static readonly HttpServer s_httpServer = new(s_mainApi.Config.GetHttpServerUrl());

        private static readonly Dictionary<string, CommandInstanceSchematics> s_commandReference = Initializer.LoadPlugins();
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            FileIO.WriteFile(Path.Join(s_programRoot,"command_reference.json"),JsonConvert.SerializeObject(s_commandReference,Formatting.Indented));
            s_mainLogger.Info("Bot Launched");
            Task.Run(s_httpServer.Start);
            string TempString;
            while (true)
            {
                TempString = Console.ReadLine() ?? "";
                if (TempString.Equals("stop"))
                {
                    s_httpServer.Stop();
                    break;
                }
            }
            s_mainLogger.Info("Bot Colsed");
            Console.ReadKey();
        }
        internal static UndefinedAPI GetApi()
        {
            return s_mainApi;
        }
        public static string GetProgramRoot()
        {
            return s_programRoot;
        }
        public static string GetProgramCahce()
        {
            return s_programCahce;
        }
    }
}
