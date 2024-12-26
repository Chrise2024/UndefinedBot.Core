using System.Text.Json.Serialization;
using UndefinedBot.Core.Command.CommandResult;
using UndefinedBot.Core.Command.CommandNodes;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.CommandSource;

namespace UndefinedBot.Core.Command;

public class CommandInstance
{
    [JsonPropertyName("target_adapter_id")] public string TargetAdapterId { get; }
    [JsonPropertyName("name")] public string Name { get; private set; }
    [JsonPropertyName("alias")] public List<string> CommandAlias { get; private set; } = [];
    [JsonPropertyName("description")] public string? CommandDescription { get; private set; }
    [JsonPropertyName("short_description")] public string? CommandShortDescription { get; private set; }
    [JsonPropertyName("usage")] public string? CommandUsage { get; private set; }
    [JsonPropertyName("example")] public string? CommandExample { get; private set; }
    [JsonIgnore] private RootCommandNode RootNode { get; set; }
    internal CommandInstance(string commandName,string targetAdapterId)
    {
        TargetAdapterId = targetAdapterId;
        Name = commandName;
        RootNode = new RootCommandNode(commandName);
    }

    internal async Task<ICommandResult> Run(CommandContext ctx,BaseCommandSource source, List<ParsedToken> tokens)
    {
        return await RootNode.ExecuteSelf(ctx,source, tokens);
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
    public ICommandNode Execute(Func<CommandContext,BaseCommandSource, Task> action)
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

[Serializable] public class CommandProperties
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("alias")] public List<string> CommandAlias { get; set; } = [];
    [JsonPropertyName("description")] public string? CommandDescription { get; set; }
    [JsonPropertyName("short_description")] public string? CommandShortDescription { get; set; }
    [JsonPropertyName("usage")] public string? CommandUsage { get; set; }
    [JsonPropertyName("example")] public string? CommandExample { get; set; }
}
