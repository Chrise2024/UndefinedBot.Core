using Newtonsoft.Json;
using UndefinedBot.Core.Command.CommandResult;
using UndefinedBot.Core.Command.CommandNodes;
using UndefinedBot.Core.Command.Arguments;

namespace UndefinedBot.Core.Command;

public class CommandInstance
{
    [JsonProperty("name")] public string Name { get; private set; }
    [JsonProperty("alias")] public List<string> CommandAlias { get; private set; } = [];
    [JsonProperty("description")] public string? CommandDescription { get; private set; }
    [JsonProperty("short_description")] public string? CommandShortDescription { get; private set; }
    [JsonProperty("usage")] public string? CommandUsage { get; private set; }
    [JsonProperty("example")] public string? CommandExample { get; private set; }
    [JsonIgnore] private RootCommandNode RootNode { get; set; }

    internal CommandInstance(string commandName)
    {
        Name = commandName;
        RootNode = new RootCommandNode(commandName);
    }

    internal async Task<ICommandResult> Run(CommandContext ctx, List<ParsedToken> tokens)
    {
        return await RootNode.ExecuteSelf(ctx, tokens);
    }

    /// <summary>
    /// Add command alias
    /// </summary>
    /// <param name="alias">Array of aliases</param>
    /// <returns>self</returns>
    public CommandInstance Alias(IEnumerable<string> alias)
    {
        foreach (string item in alias)
        {
            if (!CommandAlias.Contains(item))
            {
                CommandAlias.Add(item);
            }
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
    public ICommandNode Execute(Func<CommandContext, Task> action)
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
}

public struct CommandProperty
{
    [JsonProperty("name")] public string Name;
    [JsonProperty("alias")] public List<string> CommandAlias;
    [JsonProperty("description")] public string? CommandDescription;
    [JsonProperty("short_description")] public string? CommandShortDescription;
    [JsonProperty("usage")] public string? CommandUsage;
    [JsonProperty("example")] public string? CommandExample;
}
