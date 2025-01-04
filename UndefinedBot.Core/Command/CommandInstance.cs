using System.Text.Json.Serialization;
using UndefinedBot.Core.Command.CommandResult;
using UndefinedBot.Core.Command.CommandNodes;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.CommandSource;

namespace UndefinedBot.Core.Command;

public sealed class CommandInstance
{
    [JsonPropertyName("target_adapter_id")]  internal string TargetAdapterId { get; set; }
    [JsonPropertyName("plugin_id")] internal string PluginId { get; }
    [JsonPropertyName("name")] internal string Name { get; set; }
    [JsonPropertyName("alias")] private List<string> CommandAlias { get; set; } = [];
    [JsonPropertyName("description")] private string? CommandDescription { get; set; }
    [JsonPropertyName("short_description")]private string? CommandShortDescription { get; set; }
    [JsonPropertyName("usage")] private string? CommandUsage { get; set; }
    [JsonPropertyName("example")] private string? CommandExample { get; set; }
    [JsonIgnore] private RootCommandNode RootNode { get; set; }

    internal CommandInstance(string commandName, string pluginId, string targetAdapterId)
    {
        TargetAdapterId = targetAdapterId;
        PluginId = pluginId;
        Name = commandName;
        RootNode = new RootCommandNode(commandName);
    }

    internal bool IsTargetCommand(string commandName)
    {
        return Name == commandName || CommandAlias.Contains(commandName);
    }

    internal async Task<ICommandResult> Run(CommandContext ctx, BaseCommandSource source, List<ParsedToken> tokens)
    {
        return await RootNode.ExecuteSelf(ctx, source, tokens);
    }

    /// <summary>
    /// Add command alias
    /// </summary>
    /// <param name="alias">Array of aliases</param>
    /// <returns>self</returns>
    public CommandInstance Alias(List<string> alias)
    {
        foreach (string item in alias.Where(item => !CommandAlias.Contains(item)))
        {
            CommandAlias.Add(item);
        }

        return this;
    }

    /// <summary>
    /// <para>Add command desc</para>
    /// <para>This desc will be displayed in help command</para>
    /// </summary>
    /// <param name="description">description</param>
    /// <returns></returns>
    public CommandInstance Description(string description)
    {
        CommandDescription = description;
        return this;
    }

    /// <summary>
    /// <para>Add command short desc</para>
    /// <para>This desc will be displayed in help command summary</para>
    /// </summary>
    /// <param name="shortDescription">short description</param>
    /// <returns>self</returns>
    public CommandInstance ShortDescription(string shortDescription)
    {
        CommandShortDescription = shortDescription;
        return this;
    }

    /// <summary>
    /// Add command usage pattern
    /// </summary>
    /// <param name="usage">usage</param>
    /// <returns>self</returns>
    public CommandInstance Usage(string usage)
    {
        CommandUsage = usage;
        return this;
    }

    /// <summary>
    /// Add command example
    /// </summary>
    /// <param name="example">example</param>
    /// <returns>self</returns>
    public CommandInstance Example(string example)
    {
        CommandExample = example;
        return this;
    }

    /// <summary>
    /// Add command action
    /// </summary>
    /// <param name="action"><see cref="UndefinedBot.Core.Command.CommandNodes.CommandNodeAction"/></param>
    /// <example>
    /// <code>
    ///     this.Execute(async (ctx) => {
    ///         ...
    ///     });
    /// </code>
    /// </example>
    /// <remarks>While action added,control flow will goto command tree building.</remarks>
    /// <returns>self</returns>
    public ICommandNode Execute(Func<CommandContext, BaseCommandSource, Task> action)
    {
        //RootNode.SetAction(action);
        return RootNode.Execute(action);
    }

    /// <summary>
    /// Add child node to this node
    /// </summary>
    /// <param name="nextNode"><see cref="UndefinedBot.Core.Command.CommandNodes.SubCommandNode"/> or <see cref="UndefinedBot.Core.Command.CommandNodes.VariableNode"/></param>
    /// <returns>This node self</returns>
    /// <example>
    /// <code>
    /// </code>
    /// </example>
    public ICommandNode Then(ICommandNode nextNode)
    {
        return RootNode.Then(nextNode);
    }

    internal CommandProperties ExportToCommandProperties()
    {
        return new CommandProperties
        {
            Name = Name,
            CommandDescription = CommandDescription,
            CommandShortDescription = CommandShortDescription,
            CommandAlias = CommandAlias,
            CommandExample = CommandExample,
            CommandUsage = CommandUsage
        };
    }
}

[Serializable]
public sealed class CommandProperties
{
    [JsonPropertyName("name")] public string Name { get; init; } = "";
    [JsonPropertyName("alias")] public List<string> CommandAlias { get; init; } = [];
    [JsonPropertyName("description")] public string? CommandDescription { get; init; }

    [JsonPropertyName("short_description")]
    public string? CommandShortDescription { get; init; }

    [JsonPropertyName("usage")] public string? CommandUsage { get; init; }
    [JsonPropertyName("example")] public string? CommandExample { get; init; }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Name);
    }
}