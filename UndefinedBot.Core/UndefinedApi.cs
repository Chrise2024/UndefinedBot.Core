using UndefinedBot.Core.Command;
using UndefinedBot.Core.NetWork;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core
{
    internal class Core
    {
        private static readonly string s_programRoot = Environment.CurrentDirectory;

        private static readonly ConfigManager s_mainConfigManager = new();
        public static ConfigManager GetConfigManager()
        {
            return s_mainConfigManager;
        }
        public static string GetCoreRoot()
        {
            return s_programRoot;
        }
    }
    public class UndefinedAPI(string pluginName, string commandName)
    {
        public readonly string PluginName = pluginName;
        public readonly Logger Logger = new(pluginName, commandName);
        public readonly HttpApi Api = new(Core.GetConfigManager().GetHttpPostUrl());
        public readonly HttpRequest Request = new();
        public readonly ConfigManager Config = new();
        public readonly string RootPath = Environment.CurrentDirectory;
        public readonly string CachePath = Path.Join(Core.GetCoreRoot(),"Cache", pluginName);
        public CommandEvent CommandEvent = CommandHandler.Event;
        public MsgBuilder GetMessageBuilder()
        {
            return MsgBuilder.GetInstance();
        }
    }
}
