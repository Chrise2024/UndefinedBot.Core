﻿using System.Text.Json;
using System.Text.Json.Nodes;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.CommandSource;
using UndefinedBot.Core.Plugin;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Adapter;

public interface IAdapterInstance
{
    string Id { get; }
    string Name { get; }
    string Platform { get; }
    string Protocol { get; }
    public List<long> GroupId { get; }
    public string CommandPrefix { get; }

    /// <summary>
    /// Handle custom action invoked by command
    /// </summary>
    byte[]? HandleCustomAction(string action, byte[]? paras);

    /// <summary>
    /// Handle default action invoked by command
    /// </summary>
    byte[]? HandleDefaultAction(DefaultActionType action, byte[]? paras);
}

public abstract class BaseAdapter : IAdapterInstance
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract string Platform { get; }
    public abstract string Protocol { get; }
    public List<long> GroupId { get; }
    public string CommandPrefix { get; }
    protected ILogger Logger => new BaseLogger(["Adapter",Name]);
    protected AdapterConfigData AdapterConfig { get; }
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
    /// <param name="invokeProperties">Command's basic information</param>
    /// <param name="source">Command Source</param>
    /// <param name="tokens">Tokens, the body of the command</param>
    protected async void SubmitCommandEvent(
        CommandInvokeProperties invokeProperties,
        BaseCommandSource source,
        List<ParsedToken> tokens
    )
    {
        CommandInvokeResult result = await CommandInvokeManager.InvokeCommand(invokeProperties.Implement(Id, Platform, Protocol, tokens), source);
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
        //CommandEventBus.InvokeCommandEvent(invokeProperties.Implement(Id, Platform, Protocol, tokens), source);
    }
    /// <summary>
    /// Handle Custom Action Invoked by Command
    /// </summary>
    /// <param name="action">Action Name</param>
    /// <param name="paras">Parameters</param>
    /// <returns></returns>
    public abstract byte[]? HandleCustomAction(string action, byte[]? paras);
    /// <summary>
    /// Handle Default Action Invoked by Command
    /// </summary>
    /// <param name="action">Action Type</param>
    /// <param name="paras">Parameters</param>
    /// <returns></returns>
    public abstract byte[]? HandleDefaultAction(DefaultActionType action, byte[]? paras);
}

public enum DefaultActionType
{
    SendPrivateMsg = 0,
    SendGroupMsg = 1,
    RecallMessage = 2,
    GetMessage = 3,
    GetGroupMemberInfo = 4,
    GetGroupMemberList = 5,
    GroupMute = 6,
    GroupKick = 7,
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
    public List<long> GroupId { get; init; } = [];
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

/// <summary>
/// This class is for program record adapter's properties,include information defined in assembly
/// </summary>
[Obsolete("These Properties Is Already Included in IAdapterInstance", true)]
[Serializable]
public sealed class AdapterProperties(BaseAdapter baseInstance, AdapterConfigData originData)
{
    public string Id => baseInstance.Id;
    public string Name => baseInstance.Name;
    public string Platform => baseInstance.Platform;
    public string Protocol => baseInstance.Protocol;
    public List<long> GroupIds => originData.GroupId;
    public string Description => originData.Description;
    public string CommandPrefix => originData.CommandPrefix;
}

internal class AdapterLoadFailedException(string? message) : Exception(message);