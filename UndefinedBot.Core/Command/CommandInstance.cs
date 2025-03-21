﻿using System.Text.Json;
using UndefinedBot.Core.Command.CommandResult;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.CommandNode;
using UndefinedBot.Core.Command.CommandUtils;
using UndefinedBot.Core.Message;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Command;

public sealed class CommandInstance : IDisposable
{
    public const CommandAttribFlags DefaultCommandAttrib = CommandAttribFlags.AllowAlias |
                                                           CommandAttribFlags.ActiveInFriend |
                                                           CommandAttribFlags.ActiveInGroup |
                                                           CommandAttribFlags.ActiveInGuild |
                                                           CommandAttribFlags.IgnoreRequirement;

    internal string[] TargetAdapterId { get; }
    internal string PluginId { get; }
    internal string Name { get; }
    private RootCommandNode RootNode { get; }
    internal CacheManager Cache { get; }
    private ILogger Logger { get; }

    internal CommandInstance(string commandName, string pluginId, string[] targetAdapterId,ILogger parentLogger)
    {
        TargetAdapterId = targetAdapterId;
        PluginId = pluginId;
        Name = commandName;
        Logger = parentLogger.Extend(["Command",commandName]);
        Cache = new CacheManager(pluginId, Logger);
        RootNode = new RootCommandNode(commandName);
        RootNode.SetCommandAttrib(CommandAttrib);
    }

    //For internal invoke command
    internal async Task<ICommandResult> RunAsync(CommandContext ctx, BaseMessageSource source, ParsedToken[] tokens)
    {
        RateManager.UpdateLastExecute(ctx.Content);
        source.SetCurrentCommandAttrib(CommandAttrib);
        return await RootNode.ExecuteSelfAsyncAsync(ctx, source, tokens);
    }

    #region CommandTargetJudgment

    internal bool IsTargetCommand(CommandContent content, BaseMessageSource source)
    {
        return IsProperEnvironment(content) && IsRequirementMet(content, source) && IsNameMatch(content) && IsAuthorized(source);
    }

    internal bool IsTargetCommandLiteral(CommandContent content, string commandName)
    {
        return IsProperEnvironment(content) && IsNameMatch(commandName);
    }

    private bool IsProperEnvironment(MessageProperty cip)
    {
        switch (cip.SubType)
        {
            case MessageSubType.Friend when !CommandAttrib.HasFlag(CommandAttribFlags.ActiveInFriend):
            case MessageSubType.Group when !CommandAttrib.HasFlag(CommandAttribFlags.ActiveInGroup):
            case MessageSubType.Guild when !CommandAttrib.HasFlag(CommandAttribFlags.ActiveInGuild):
                return false;
        }

        return true;
    }

    private bool IsRequirementMet(CommandContent cip, BaseMessageSource source)
    {
        return CommandAttrib.HasFlag(CommandAttribFlags.IgnoreRequirement) || CommandRequire is null ||
               !CommandRequire(cip, source);
    }

    private bool IsNameMatch(CommandContent cip)
    {
        return IsNameMatch(cip.CalledCommandName);
    }

    private bool IsNameMatch(string commandName)
    {
        StringComparison comparison = CommandAttrib.HasFlag(CommandAttribFlags.IgnoreCase)
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        bool allowAlias = CommandAttrib.HasFlag(CommandAttribFlags.AllowAlias);
        return Name.Equals(commandName, comparison) ||
               (allowAlias && CommandAlias.FindIndex(x => x.Equals(commandName, comparison)) != -1);
    }
    
    private bool IsAuthorized(BaseMessageSource source)
    {
        return CommandAttrib.HasFlag(CommandAttribFlags.IgnoreAuthority) || source.HasAuthorityLevel(CommandAuthority);
    }

    #endregion

    #region Properties

    private List<string> CommandAlias { get; } = [];
    private string? CommandDescription { get; set; }
    private string? CommandShortDescription { get; set; }
    private string? CommandUsage { get; set; }
    private string? CommandExample { get; set; }
    private MessageSourceAuthority CommandAuthority { get; set; } = MessageSourceAuthority.Admin;
    private CommandAttribFlags CommandAttrib { get; set; } = DefaultCommandAttrib;

    /// <summary>
    /// Add command alias
    /// </summary>
    /// <param name="alias">Array of aliases</param>
    /// <returns>self</returns>
    public CommandInstance Alias(IEnumerable<string> alias)
    {
        CommandAlias.AddRange(alias.Where(item => !CommandAlias.Contains(item)));
        Logger.Trace($"Added alias, current is [{string.Join(",", CommandAlias)}]");
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
        Logger.Trace($"Description set to {description}");
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
        Logger.Trace($"ShortDescription set to \"{shortDescription}\"");
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
        Logger.Trace($"Usage set to \"{usage}\"");
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
        Logger.Trace($"Example set to \"{example}\"");
        return this;
    }
    
    public CommandInstance Authority(MessageSourceAuthority authority)
    {
        CommandAuthority = authority;
        Logger.Trace($"Authority set to \"{authority}\"");
        return this;
    }

    /// <summary>
    /// Add command attrib
    /// </summary>
    /// <param name="attr">Command's attrib</param>
    /// <returns>self</returns>
    public CommandInstance Attrib(CommandAttribFlags attr)
    {
        if (attr.HasFlag(CommandAttribFlags.RateLimit)) RateManager.SetMode(CommandRateManagerMode.Individual);
        RateManager.SetMode(CommandRateManagerMode.Disable);
        CommandAttrib = attr;
        Logger.Trace($"Example set to \"{(int)attr}\"");
        return this;
    }

    #endregion

    #region RateLimit

    private CommandRateManager RateManager { get; } = new();

    /// <summary>
    /// Add command example
    /// </summary>
    /// <param name="limit">Rate Limit</param>
    /// <param name="global">Rate limit will be operated global or environment individual</param>
    /// <returns>self</returns>
    public CommandInstance RateLimit(TimeSpan limit, bool global = false)
    {
        RateManager.SetMode(global ? CommandRateManagerMode.Global : CommandRateManagerMode.Individual);
        RateManager.SetRateLimit(limit);
        Logger.Trace($"Rate limit set to {limit.TotalSeconds} seconds");
        return this;
    }

    internal bool IsReachRateLimit(CommandContent content)
    {
        return RateManager.IsReachRateLimit(content);
    }

    #endregion

    #region UserFunctional

    private Func<CommandContent, BaseMessageSource, bool>? CommandRequire { get; set; }

    /// <summary>
    /// Add command requirement
    /// </summary>
    /// <param name="predicate">requirement</param>
    /// <returns>self</returns>
    public CommandInstance Require(Func<CommandContent, BaseMessageSource, bool> predicate)
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
    public CommandNode.CommandNode Execute(Func<CommandContext, BaseMessageSource, CancellationToken, Task> action)
    {
        //RootNode.SetAction(action);
        return RootNode.Execute(action);
    }

    /// <summary>
    /// Add child node to this node
    /// </summary>
    /// <param name="nextNode"><see cref="SubCommandNode"/> or <see cref="VariableNode"/></param>
    /// <returns>This node self</returns>
    /// <example>
    /// <code>
    /// </code>
    /// </example>
    public CommandNode.CommandNode Then(CommandNode.CommandNode nextNode)
    {
        return RootNode.Then(nextNode);
    }

    #endregion

    #region InternalUsage

    internal string GetFullHelpText(CommandContent content)
    {
        return string.Format(
            "---------------help---------------\n{0}{1}{2}\n可用指令别名: \n{3}",
            CommandDescription == null ? "" : $"{Name} - {CommandDescription}\n",
            CommandUsage == null ? "" : $"使用方法: \n{string.Format(CommandUsage, content.CommandPrefix)}\n",
            CommandExample == null ? "" : $"e.g.\n{string.Format(CommandExample, content.CommandPrefix)}\n",
            JsonSerializer.Serialize(CommandAlias)
        );
    }

    internal string GetShortHelpText(CommandContent content)
    {
        return $"{content.CommandPrefix}{Name} - {CommandShortDescription ?? "NULL"}\n";
    }

    internal bool IsHidden()
    {
        return CommandAttrib.HasFlag(CommandAttribFlags.Hidden);
    }

    internal ILogger AcquireLogger()
    {
        return Logger;
    }

    #endregion

    public void Dispose()
    {
        CommandAlias.Clear();
        Cache.Dispose();
        RootNode.Dispose();
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
    ActiveInFriend = 0b_0000_0000_0000_0001,

    /// <summary>
    /// The command can be triggered in group chat
    /// </summary>
    ActiveInGroup = 0b_0000_0000_0000_0010,

    /// <summary>
    /// The command can be triggered in guild chat
    /// </summary>
    ActiveInGuild = 0b_0000_0000_0000_0100,

    /// <summary>
    /// The command can be triggered without authority check
    /// </summary>
    IgnoreRequirement = 0b_0000_0000_0000_1000,

    /// <summary>
    /// The command's trigger rate will be limited.If command not have this attrib,rate limit set in <see cref="CommandInstance"/> will be ignored
    /// </summary>
    RateLimit = 0b_0000_0000_0001_0000,

    /// <summary>
    /// The command will be hidden in help command
    /// </summary>
    Hidden = 0b_0000_0000_0010_0000,

    /// <summary>
    /// The command can be triggered with ignoring case
    /// </summary>
    IgnoreCase = 0b_0000_0000_0100_0000,

    /// <summary>
    /// Allow using alias to trigger the command
    /// </summary>
    AllowAlias = 0b_0000_0000_1000_0000,

    /// <summary>
    /// The command can be triggered with ignoring user authority level
    /// </summary>
    IgnoreAuthority = 0b_0000_0001_0000_0000
}