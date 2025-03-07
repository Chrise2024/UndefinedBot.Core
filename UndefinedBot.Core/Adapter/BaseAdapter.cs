﻿using UndefinedBot.Core.Adapter.ActionParam;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.CommandSource;
using UndefinedBot.Core.Command.CommandUtils;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Adapter;

public interface IAdapterInstance : IDisposable
{
    string Id { get; }
    string Name { get; }
    string Platform { get; }
    string Protocol { get; }
    public long[] GroupId { get; }
    public string CommandPrefix { get; }
    internal ILogger AcquireLogger();
    internal void ExternalInvokeCommand(CommandInformation information, BaseCommandSource source);
    internal void MountCommands(List<CommandInstance> commandInstances);

    /// <summary>
    /// Handle default action invoked by command
    /// </summary>
    Task<byte[]?> HandleActionAsync(ActionType action, string? target = null, IActionParam? parameter = null);

    void Initialize();
}

public abstract class BaseAdapter : IAdapterInstance
{
    /// <summary>
    /// The identifier of the adapter, must be unique
    /// </summary>
    public abstract string Id { get; }

    public abstract string Name { get; }

    /// <summary>
    /// The platform adapter will be used
    /// </summary>
    public abstract string Platform { get; }

    /// <summary>
    /// The protocol adapter will used
    /// </summary>
    public abstract string Protocol { get; }

    /// <summary>
    /// Groups adapter will work on
    /// </summary>
    public long[] GroupId { get; }

    /// <summary>
    /// Message prefix to be seen as command
    /// </summary>
    public string CommandPrefix { get; }

    protected ILogger Logger { get; }
    protected IReadonlyConfig AdapterConfig { get; }

    /// <summary>
    /// The location of the adapter folder
    /// </summary>
    protected string AdapterPath => Path.GetDirectoryName(GetType().Assembly.Location) ?? Path.Join();

    private CommandManager? CommandManager { get; set; }

    /// <summary>
    /// Handle Custom Action Invoked by Command
    /// </summary>
    /// <param name="action">Action Name</param>
    /// <param name="target">Target of action</param>
    /// <param name="parameter">parameters</param>
    /// <returns></returns>
    public abstract Task<byte[]?> HandleActionAsync(ActionType action, string? target = null,
        IActionParam? parameter = null);

    public abstract void Initialize();

    ILogger IAdapterInstance.AcquireLogger()
    {
        return Logger;
    }

    void IAdapterInstance.MountCommands(List<CommandInstance> commandInstances)
    {
        CommandManager = new CommandManager(this, commandInstances);
    }

    protected BaseAdapter(AdapterDependencyCollection dependencyCollection)
    {
        Logger = dependencyCollection.LoggerFactory.CreateCategoryLogger(GetType());
        AdapterConfig = dependencyCollection.AdapterConfig;
        GroupId = AdapterConfig.GetValue<long[]>("GroupId") ?? throw new Exception("GroupId not found");
        CommandPrefix = AdapterConfig.GetValue<string>("CommandPrefix") ?? throw new Exception("CommandPrefix not found");
    }

    /// <summary>
    /// After processing message, use it to submit this event
    /// </summary>
    /// <param name="information">Command's basic information</param>
    /// <param name="source">Command Source</param>
    /// <param name="tokens">Tokens, the body of the command</param>
    protected void SubmitCommandEvent(
        CommandInformation information,
        BaseCommandSource source,
        ParsedToken[] tokens
    )
    {
        if (CommandManager is null)
        {
            Logger.Warn("CommandManager not initialized");
            return;
        }

        Logger.Trace(
            $"Command submitted, command {information.CalledCommandName} called by {information.SenderId} in {information.SubType.ToString()} {information.SourceId}");
        CommandManager.InvokeCommandAsync(information, source, tokens);
    }

    void IAdapterInstance.ExternalInvokeCommand(CommandInformation information, BaseCommandSource source)
    {
        CommandManager?.InvokeCommandAsync(information, source, information.Tokens);
    }

    public virtual void Dispose()
    {
        CommandManager?.Dispose();
        Array.Clear(GroupId);
        GC.SuppressFinalize(this);
    }
}

internal class AdapterLoadFailedException(string? message) : Exception(message);