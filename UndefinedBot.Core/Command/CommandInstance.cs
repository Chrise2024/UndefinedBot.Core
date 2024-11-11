using Newtonsoft.Json;
using UndefinedBot.Core.Command.Arguments.ArgumentType;

namespace UndefinedBot.Core.Command
{
    public class CommandInstance
    {
        [JsonProperty("name")] public string Name { get; private set; }
        [JsonProperty("alias")] public List<string> CommandAlias { get; private set; } = [];
        [JsonProperty("description")] public string? CommandDescription { get; private set; }
        [JsonProperty("short_description")] public string? CommandShortDescription { get; private set; }
        [JsonProperty("usage")] public string? CommandUsage { get; private set; }
        [JsonProperty("example")] public string? CommandExample { get; private set; }
        [JsonIgnore] public ICommandNode RootNode { get; private set; }
        public CommandInstance(string commandName)
        {
            Name = commandName;
            RootNode = new RootCommandNode(commandName, new StringArgument());
        }
        public async Task Run(CommandContext ctx)
        {
            await RootNode.ExecuteSelf(ctx);
        }
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
        public CommandInstance Usage(string usage)
        {
            CommandUsage = usage;
            return this;
        }
        public CommandInstance Example(string example)
        {
            CommandExample = example;
            return this;
        }
        public ICommandNode Execute(CommandNodeAction action)
        {
            RootNode.SetAction(action);
            return RootNode;
        }
    }

    public class CommandSyntaxException(string message) : Exception(message);
}