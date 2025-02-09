using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Command.CommandResult;
using UndefinedBot.Core.Command.CommandNodes;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.CommandSource;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Command;

public sealed class CommandInstance : IDisposable
{
    public const CommandAttribFlags DefaultCommandAttrib = CommandAttribFlags.AllowAlias |
                                                           CommandAttribFlags.ActiveInFriend |
                                                           CommandAttribFlags.ActiveInGroup |
                                                           CommandAttribFlags.ActiveInGuild |
                                                           CommandAttribFlags.IgnoreRequirement;
    internal string TargetAdapterId { get; }
    internal string PluginId { get; }
    internal string Name { get; }
    private List<string> CommandAlias { get; } = [];
    private string? CommandDescription { get; set; }
    private string? CommandShortDescription { get; set; }
    private string? CommandUsage { get; set; }
    private string? CommandExample { get; set; }
    private TimeSpan CommandRateLimit { get; set; } = TimeSpan.Zero;
    
    private Func<CommandBackgroundEnvironment, BaseCommandSource, bool>? CommandRequire { get; set; }
    private CommandAttribFlags CommandAttrib { get; set; } = DefaultCommandAttrib;

    private long _lastExecute;
    private RootCommandNode RootNode { get; }
    internal CacheManager Cache => new(PluginId);
    internal CommandInstance(string commandName, string pluginId, string targetAdapterId)
    {
        TargetAdapterId = targetAdapterId;
        PluginId = pluginId;
        Name = commandName;
        RootNode = new RootCommandNode(commandName);
        RootNode.SetCommandAttrib(CommandAttrib);
    }

    internal bool IsTargetCommand(CommandBackgroundEnvironment cip, BaseCommandSource source)
    {
        switch (cip.SubType)
        {
            case MessageSubType.Friend when !CommandAttrib.HasFlag(CommandAttribFlags.ActiveInFriend):
            case MessageSubType.Group when !CommandAttrib.HasFlag(CommandAttribFlags.ActiveInGroup):
            case MessageSubType.Guild when !CommandAttrib.HasFlag(CommandAttribFlags.ActiveInGuild):
                return false;
        }
        if (!CommandAttrib.HasFlag(CommandAttribFlags.IgnoreRequirement) && CommandRequire is not null && CommandRequire(cip, source))
        {
            return false;
        }
        
        StringComparison comparison = CommandAttrib.HasFlag(CommandAttribFlags.IgnoreCase)
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        bool allowAlias = CommandAttrib.HasFlag(CommandAttribFlags.AllowAlias);
        return Name.Equals(cip.Command, comparison) ||
               (allowAlias && CommandAlias.FindIndex(x => x.Equals(cip.Command, comparison)) != -1);
    }

    internal bool IsReachRateLimit(CommandBackgroundEnvironment ip)
    {
        return !CommandAttrib.HasFlag(CommandAttribFlags.RateLimit) &&
               CommandRateLimit != TimeSpan.Zero && ip.TimeStamp - _lastExecute < CommandRateLimit.TotalSeconds;
    }
    //For internal invoke command
    internal async Task<ICommandResult> Run(CommandContext ctx, BaseCommandSource source, ParsedToken[] tokens)
    {
        _lastExecute = ctx.BackgroundEnvironment.TimeStamp;
        source.SetCurrentCommandAttrib(CommandAttrib);
        return await RootNode.ExecuteSelf(ctx, source, tokens);
    }
    /// <summary>
    /// Add command attrib
    /// </summary>
    /// <param name="attr">Command's attrib</param>
    /// <returns>self</returns>
    public CommandInstance Attrib(CommandAttribFlags attr)
    {
        CommandAttrib = attr;
        return this;
    }
    /// <summary>
    /// Add command alias
    /// </summary>
    /// <param name="alias">Array of aliases</param>
    /// <returns>self</returns>
    public CommandInstance Alias(IEnumerable<string> alias)
    {
        CommandAlias.AddRange(alias.Where(item => !CommandAlias.Contains(item)));
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
    /// Add command example
    /// </summary>
    /// <param name="limit">Rate Limit</param>
    /// <returns>self</returns>
    public CommandInstance RateLimit(TimeSpan limit)
    {
        CommandRateLimit = limit;
        return this;
    }
    
    /// <summary>
    /// Add command requirement
    /// </summary>
    /// <param name="predicate">requirement</param>
    /// <returns>self</returns>
    public CommandInstance Require(Func<CommandBackgroundEnvironment, BaseCommandSource, bool> predicate)
    {
        CommandRequire = predicate;
        return this;
    }

    /// <summary>
    /// Add command action
    /// </summary>
    /// <param name="action"><see cref="System.Func{CommandContext, BaseCommandSource, Task}"/></param>
    /// <example>
    /// <code>
    ///     this.Execute(async (ctx,source) => {
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

    internal CommandProperties ExportToCommandProperties(Dictionary<string, IAdapterInstance> adapterList)
    {
        return new CommandProperties
        {
            Name = Name,
            IsHidden = CommandAttrib.HasFlag(CommandAttribFlags.Hidden),
            CommandDescription = CommandDescription,
            CommandShortDescription = CommandShortDescription,
            CommandAlias = CommandAlias,
            CommandPrefix = adapterList.TryGetValue(TargetAdapterId,out IAdapterInstance? v) ? v.CommandPrefix : "",
            CommandExample = CommandExample,
            CommandUsage = CommandUsage
        };
    }
    public void Dispose()
    {
        CommandAlias.Clear();
        Cache.Dispose();
        RootNode.Dispose();
    }
}
public readonly struct CommandProperties()
{
    public string Name { get; init; } = "";
    public bool IsHidden { get; init; }
    public List<string> CommandAlias { get; init; } = [];
    public string CommandPrefix { get; init; } = "";
    public string? CommandDescription { get; init; }
    public string? CommandShortDescription { get; init; }
    public string? CommandUsage { get; init; }
    public string? CommandExample { get; init; }
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Name);
    }
}

/// <summary>
/// Attrib of command,default is <see cref="CommandAttribFlags.AllowAlias"/> | <see cref="CommandAttribFlags.ActiveInFriend"/> | <see cref="CommandAttribFlags.ActiveInGroup"/> | <see cref="CommandAttribFlags.ActiveInGuild"/> | <see cref="IgnoreRequirement"/>
/// </summary>
[Flags]
public enum CommandAttribFlags
{
    /// <summary>
    /// The command can be triggered in friend chat
    /// </summary>
    ActiveInFriend  = 0b_0000_0000_0000_0001,
    /// <summary>
    /// The command can be triggered in group chat
    /// </summary>
    ActiveInGroup   = 0b_0000_0000_0000_0010,
    /// <summary>
    /// The command can be triggered in guild chat
    /// </summary>
    ActiveInGuild   = 0b_0000_0000_0000_0100,
    /// <summary>
    /// The command can be triggered without authority check
    /// </summary>
    IgnoreRequirement = 0b_0000_0000_0000_1000,
    /// <summary>
    /// The command's trigger rate will be limited.If command not have this attrib,rate limit set in <see cref="CommandInstance"/> will be ignored
    /// </summary>
    RateLimit       = 0b_0000_0000_0001_0000,
    /// <summary>
    /// The command will be hidden in help command
    /// </summary>
    Hidden          = 0b_0000_0000_0010_0000,
    /// <summary>
    /// The command can be triggered with ignoring case
    /// </summary>
    IgnoreCase      = 0b_0000_0000_0100_0000,
    /// <summary>
    /// Allow using alias to trigger the command
    /// </summary>
    AllowAlias      = 0b_0000_0000_1000_0000,
}