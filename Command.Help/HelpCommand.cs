using Newtonsoft.Json;
using UndefinedBot.Core;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Utils;

namespace Command.Help
{

    public class HelpCommand : IBaseCommand
    {
        public HelpCommand()
        {
            string CommandText = "";
            foreach (var pair in _commandReference)
            {
                CommandText += pair.Value.Properties.ShortDescription;
            }
            _baseHelpText = "---------------help---------------\n指令列表：\n" +
                CommandText +
                "使用#help+具体指令查看使用方法\ne.g. #help help";

        }
        private readonly string _baseHelpText;

        private string _commandPrefix = "#";

        private readonly Dictionary<string, CommandInstanceSchematics> _commandReference = JsonConvert.DeserializeObject<Dictionary<string, CommandInstanceSchematics>>(File.ReadAllText(Path.Join(Environment.CurrentDirectory, "command_reference.json"))) ?? [];
        public UndefinedAPI CommandApi { get; private set; } = new("Program","help");
        public string CommandName { get; private set; } = "help";
        public Logger CommandLogger { get; private set; } = new("Program", "Help");
        public async Task Execute(ArgSchematics args)
        {
            if (args.Param.Count > 0)
            {
                if (_commandReference.TryGetValue(args.Param[0], out var Prop))
                {
                    await CommandApi.Api.SendGroupMsg(
                        args.GroupId,
                        CommandApi.GetMessageBuilder()
                            .Text(string.Format(Prop.Properties.Description, _commandPrefix)).Build()
                    );
                }
                else
                {
                    await CommandApi.Api.SendGroupMsg(
                        args.GroupId,
                        CommandApi.GetMessageBuilder()
                            .Text("咦，没有这个指令").Build()
                    );
                    CommandLogger.Warn($"Command Not Found: <{args.Param[0]}>");
                }
            }
            else
            {
                await CommandApi.Api.SendGroupMsg(
                            args.GroupId,
                            CommandApi.GetMessageBuilder()
                                .Text(_baseHelpText).Build()
                        );
            }
        }
        public async Task Handle(ArgSchematics args)
        {
            if (args.Command.Equals(CommandName))
            {
                CommandLogger.Info("Command Triggered");
                await Execute(args);
                CommandLogger.Info("Command Completed");
            }
        }
        public void Init()
        {
            CommandLogger = new("Command", CommandName);
            _commandPrefix = CommandApi.Config.GetCommandPrefix();
            CommandApi.CommandEvent.OnCommand += Handle;
            CommandLogger.Info("Command Loaded");
        }
    }
    internal struct CommandPropertieSchematics
    {
        [JsonProperty("name")] public string Name;
        [JsonProperty("description")] public string Description;
        [JsonProperty("short_description")] public string ShortDescription;
        [JsonProperty("entry_point")] public string EntryPoint;
    }
    internal struct CommandInstanceSchematics(string plugin, CommandPropertieSchematics prop, object instance)
    {
        [JsonProperty("plugin")] public string plugin = plugin;
        [JsonProperty("properties")] public CommandPropertieSchematics Properties = prop;
        [Newtonsoft.Json.JsonIgnore] public object Instance = instance;
    }
}
