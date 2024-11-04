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
        private static readonly Logger s_mainLogger = new("Program");

        private static readonly string s_programRoot = Environment.CurrentDirectory;

        private static readonly string s_programCahce = Path.Join(s_programRoot, "Cache");

        private static readonly HttpServer s_httpServer = new(new ConfigManager().GetHttpServerUrl());

        private static List<PluginPropertieSchematics> s_pluginReference = [];

        private static Dictionary<string, CommandInstance> s_commandReference = [];
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            s_pluginReference = Initializer.LoadPlugins();
            s_commandReference = Initializer.GetCommandReference();
            FileIO.WriteAsJSON(Path.Join(s_programRoot, "plugin_reference.json"),s_pluginReference);
            FileIO.WriteAsJSON(Path.Join(s_programRoot, "command_reference.json"), s_commandReference);
            s_mainLogger.Info("Main","Bot Launched");
            Task.Run(s_httpServer.Start);
            //CommandHandler.Event.Trigger(new ArgSchematics("q", ["1234"],0,0,0,true));
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
            s_mainLogger.Info("Main", "Bot Colsed");
            Console.ReadKey();
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
