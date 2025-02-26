using System.Text.Json;
using System.Text.Json.Nodes;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.CommandSource;
using UndefinedBot.Core.Plugin;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Utils.Logging;

namespace UndefinedBot.Core.Adapter;

public interface IAdapterInstance : IDisposable
{
    string Id { get; }
    string Name { get; }
    string Platform { get; }
    string Protocol { get; }
    public long[] GroupId { get; }
    public string CommandPrefix { get; }

    /// <summary>
    /// Handle custom action invoked by command
    /// </summary>
    byte[]? HandleCustomAction(string action, CustomActionParameterWrapper? paras);

    /// <summary>
    /// Handle default action invoked by command
    /// </summary>
    byte[]? HandleDefaultAction(DefaultActionType action, DefaultActionParameterWrapper? paras);
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

    protected AdapterLogger Logger => new(Name);
    protected AdapterConfigData AdapterConfig { get; }

    /// <summary>
    /// The location of the adapter folder
    /// </summary>
    protected string AdapterPath => Path.GetDirectoryName(GetType().Assembly.Location) ?? "/";

    protected BaseAdapter()
    {
        AdapterConfig = GetAdapterConfig();
        GroupId = AdapterConfig.GroupId;
        CommandPrefix = AdapterConfig.CommandPrefix;
    }

    private AdapterConfigData GetAdapterConfig()
    {
        JsonNode originJson = FileIO.ReadAsJson(Path.Join(AdapterPath, "adapter.json")) ??
                              throw new AdapterLoadFailedException("Config File Not Exist");
        AdapterConfigData? adapterConfigData = originJson.Deserialize<AdapterConfigData>();
        if (adapterConfigData is null || !adapterConfigData.IsValid())
        {
            throw new AdapterLoadFailedException("Invalid Config File");
        }

        adapterConfigData.Implement(originJson);
        return adapterConfigData;
    }

    /// <summary>
    /// After processing message, use it to submit this event
    /// </summary>
    /// <param name="backgroundEnvironment">Command's basic information</param>
    /// <param name="source">Command Source</param>
    /// <param name="tokens">Tokens, the body of the command</param>
    protected async void SubmitCommandEvent(
        CommandBackgroundEnvironment backgroundEnvironment,
        BaseCommandSource source,
        ParsedToken[] tokens
    )
    {
        CommandInvokeResult result =
            await CommandManager.InvokeCommand(
                backgroundEnvironment.Implement(Id, Platform, Protocol, tokens, CommandPrefix),
                source);
        switch (result)
        {
            case CommandInvokeResult.SuccessInvoke:
                Logger.Info("Successful Invoke Command");
                break;
            case CommandInvokeResult.NoSuchCommand:
                Logger.Warn("No Such Command");
                break;
            case CommandInvokeResult.NoCommandRelateToAdapter:
                Logger.Warn("No Command Bind to Adapter");
                break;
            case CommandInvokeResult.CommandRateLimited:
                Logger.Warn("Command Reach Rate Limit");
                break;
        }
    }

    /// <summary>
    /// Handle Custom Action Invoked by Command
    /// </summary>
    /// <param name="action">Action Name</param>
    /// <param name="paras">Parameters</param>
    /// <returns></returns>
    public abstract byte[]? HandleCustomAction(string action, CustomActionParameterWrapper? paras);

    /// <summary>
    /// Handle Default Action Invoked by Command
    /// </summary>
    /// <param name="action">Action Type</param>
    /// <param name="paras">Parameters</param>
    /// <returns></returns>
    public abstract byte[]? HandleDefaultAction(DefaultActionType action, DefaultActionParameterWrapper? paras);

    public virtual void Dispose()
    {
        Array.Clear(GroupId);
        Logger.Dispose();
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