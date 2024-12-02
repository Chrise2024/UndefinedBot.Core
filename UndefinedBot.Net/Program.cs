using System.Text;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Utils;
using UndefinedBot.Net.Utils;

namespace UndefinedBot.Net
{
    internal class Program
    {
        private static readonly Logger s_mainLogger = new("Program");

        private static readonly string s_programRoot = Environment.CurrentDirectory;

        private static readonly string s_programCache = Path.Join(s_programRoot, "Cache");

        private static readonly HttpServer s_httpServer = new(new ConfigManager().GetHttpServerUrl());

        private static List<PluginProperty> s_pluginReference = [];

        private static Dictionary<string, CommandProperty> s_commandReference = [];
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            s_pluginReference = Initializer.LoadPlugins();
            s_commandReference = Initializer.GetCommandReference();
            FileIO.WriteAsJson(Path.Join(s_programRoot, "plugin_reference.json"),s_pluginReference);
            FileIO.WriteAsJson(Path.Join(s_programRoot, "command_reference.json"), s_commandReference);
            s_mainLogger.Info("Bot Launched");
            Task.Run(s_httpServer.Start);
            //CommandHandler.Trigger(new CallingProperty("test",0,0,0,"",0),["308","166.6"]);
            while (true)
            {
                string tempString = Console.ReadLine() ?? "";
                if (tempString.Equals("stop"))
                {
                    s_httpServer.Stop();
                    break;
                }
            }
            s_mainLogger.Info("Bot Closed");
            Console.ReadKey();
        }
        public static string GetProgramRoot()
        {
            return s_programRoot;
        }
        public static string GetProgramCache()
        {
            return s_programCache;
        }
    }
}
