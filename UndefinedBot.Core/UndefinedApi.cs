using System.Reflection;
using Newtonsoft.Json;
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
    public class UndefinedAPI
    {
        public readonly string PluginName;
        public readonly Logger Logger;
        public readonly HttpApi Api;
        public readonly HttpRequest Request;
        public readonly ConfigManager Config;
        public readonly string RootPath;
        public readonly string CachePath;
        public readonly List<CommandInstance> _commandInstances = [];
        public UndefinedAPI(string pluginName)
        {
            PluginName = pluginName;
            Logger = new(pluginName);
            Api = new(Core.GetConfigManager().GetHttpPostUrl());
            Request = new();
            Config = new();
            RootPath = Environment.CurrentDirectory;
            CachePath = Path.Join(Core.GetCoreRoot(), "Cache", pluginName);
        }
        public void SubmitCommand()
        {
            List<CommandInstance> CommandRef = [];
            string CommandRefPath = Path.Join(RootPath, "CommandReference", $"{PluginName}.reference.json");
            foreach (var commandInstance in _commandInstances)
            {
                if (commandInstance.CommandAction != null)
                {
                    CommandHandler.Event.OnCommand += async (ArgSchematics args) => {
                        if (commandInstance.Name.Equals(args.Command) || commandInstance.CommandAlias.Contains(args.Command))
                        {
                            this.Logger.Info(commandInstance.Name, "Command Triggered");
                            await commandInstance.CommandAction(args);
                            this.Logger.Info(commandInstance.Name, "Command Completed");
                        }
                    };
                    CommandRef.Add(commandInstance);
                    this.Logger.Info(commandInstance.Name, "Successful Load Command");
                }
            }
            FileIO.WriteAsJSON(CommandRefPath, CommandRef);
        }
        public MsgBuilder GetMessageBuilder()
        {
            return MsgBuilder.GetInstance();
        }
        public CommandInstance RegisterCommand(string commandName)
        {
            CommandInstance ci = new(commandName);
            _commandInstances.Add(ci);
            return ci;
        }
    }
    public class CommandInstance(string commandName)
    {
        [JsonProperty("name")] public readonly string Name = commandName;
        [JsonProperty("alias")] public List<string> CommandAlias { get; private set; } = [];
        [JsonProperty("description")] public string? CommandDescription { get; private set; }
        [JsonProperty("short_description")] public string? CommandShortDescription { get; private set; }
        [JsonProperty("example")] public string? CommandExample { get; private set; }
        [JsonIgnore] public CommandEventHandler? CommandAction { get; private set; }
        public CommandInstance Alias(IEnumerable<string> alias)
        {
            foreach (var item in alias)
            {
                if (!CommandAlias.Contains(item))
                {
                    CommandAlias.Add(item);
                }
            }
            return this;
        }
        public CommandInstance Description(string description)
        {
            CommandDescription = description;
            return this;
        }
        public CommandInstance ShortDescription(string shortDescription)
        {
            CommandShortDescription = shortDescription;
            return this;
        }
        public CommandInstance Example(string example)
        {
            CommandExample = example;
            return this;
        }
        public void Action(CommandEventHandler action)
        {
            CommandAction = action;
        }
    }
}
