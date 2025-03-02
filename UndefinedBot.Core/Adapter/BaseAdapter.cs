using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using UndefinedBot.Core.Adapter.ActionParam;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.CommandSource;
using UndefinedBot.Core.Plugin;
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
    internal void SetUp(ILoggerFactory loggerFactory,ICommandManager commandManager);
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
    [AllowNull] protected ILogger Logger { get; private set; }
    [AllowNull] private ILoggerFactory LoggerFactory { get; set; }
    protected AdapterConfigData AdapterConfig { get; }

    /// <summary>
    /// The location of the adapter folder
    /// </summary>
    protected string AdapterPath => Path.GetDirectoryName(GetType().Assembly.Location) ?? Path.Join();

    [AllowNull] private ICommandManager CommandManager { get; set; }

    ILogger IAdapterInstance.AcquireLogger()
    {
        return Logger;
    }

    void IAdapterInstance.SetUp(ILoggerFactory loggerFactory,ICommandManager commandManager)
    {
        LoggerFactory = loggerFactory;
        CommandManager = commandManager;
        Logger = LoggerFactory.CreateCategoryLogger(GetType());
        Initialize();
        Logger.Info("Adapter initialized");
    }

    protected BaseAdapter()
    {
        AdapterConfig = GetAdapterConfig();
        GroupId = AdapterConfig.GroupId;
        CommandPrefix = AdapterConfig.CommandPrefix;
    }

    private AdapterConfigData GetAdapterConfig()
    {
        JsonNode originJson = FileIO.ReadAsJson(Path.Join(AdapterPath, "adapter.json")) ??
                              throw new AdapterLoadFailedException("Config file not exist");
        AdapterConfigData? adapterConfigData = originJson.Deserialize<AdapterConfigData>();
        if (adapterConfigData is null || !adapterConfigData.IsValid())
            throw new AdapterLoadFailedException("Invalid config file");

        adapterConfigData.Implement(originJson);
        return adapterConfigData;
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
        Logger.Trace($"Command submitted, command {information.CalledCommandName} called by {information.SenderId} in {information.SubType.ToString()} {information.SourceId}");
        CommandManager.InvokeCommandAsync(information, source, tokens);
    }

    void IAdapterInstance.ExternalInvokeCommand(CommandInformation information, BaseCommandSource source)
    {
        CommandManager.InvokeCommandAsync(information, source, information.Tokens);
    }

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

    public virtual void Dispose()
    {
        CommandManager.Dispose();
        Array.Clear(GroupId);
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// This Class records what write in config file
/// </summary>
[Serializable]
public sealed class AdapterConfigData
{
    public string EntryFile { get; init; } = "";
    public string Description { get; init; } = "";
    public string CommandPrefix { get; init; } = "!";
    public long[] GroupId { get; init; } = [];
    public JsonNode OriginalConfig { get; private set; } = JsonNode.Parse("{}")!;

    internal void Implement(JsonNode oc)
    {
        OriginalConfig = oc;
    }

    public bool IsValid()
    {
        return !(string.IsNullOrEmpty(EntryFile) || string.IsNullOrEmpty(CommandPrefix));
    }
}

internal class AdapterLoadFailedException(string? message) : Exception(message);