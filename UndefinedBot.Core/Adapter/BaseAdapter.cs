﻿using System.Reflection;
using System.Text.Json.Nodes;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.CommandSource;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Adapter;

public abstract class BaseAdapter(AdapterConfigData adapterConfig)
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract string Platform { get; }
    public abstract string Protocol { get; }
    protected AdapterLogger Logger => new(Name);
    protected AdapterConfigData AdapterConfig => adapterConfig;
    /// <summary>
    /// After processing message, use it to submit this event
    /// </summary>
    /// <param name="invokeProperties">Command's basic information</param>
    /// <param name="source">Command Source</param>
    /// <param name="tokens">Tokens, the body of the command</param>
    protected void SubmitCommandEvent(
        CommandInvokeProperties invokeProperties,
        BaseCommandSource source,
        List<ParsedToken> tokens
        )
    {
        CommandEventBus.InvokeCommandEvent(invokeProperties.Implement(Id,Platform, Protocol, tokens), source);
    }
    /// <summary>
    /// Handle custom action invoked by command
    /// </summary>
    public abstract byte[]? HandleCustomAction(string action, byte[]? paras);
    /// <summary>
    /// Handle default action invoked by command
    /// </summary>
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
[Serializable] public class AdapterConfigData : IEquatable<AdapterConfigData>
{
    public string EntryFile { get; set; } = "";
    public string Description { get; set; } = "";
    public string CommandPrefix { get; set; } = "!";
    public List<long> GroupIds { get; set; } = [];
    public JsonNode OriginalConfig { get; set; } = JsonNode.Parse("{}")!;

    public bool Equals(AdapterConfigData? other)
    {
        return EntryFile == other?.EntryFile &&
               Description == other.Description &&
               CommandPrefix == other.CommandPrefix;
    }
}
/// <summary>
/// This class is for program record adapter's properties,include information defined in assembly
/// </summary>
[Serializable] public class AdapterProperties(BaseAdapter baseInstance,AdapterConfigData originData)
{
    public string Id => baseInstance.Id;
    public string Name => baseInstance.Name;
    public string Platform => baseInstance.Platform;
    public string Protocol => baseInstance.Protocol;
    public List<long> GroupIds => originData.GroupIds;
    public string Description => originData.Description;
    public string CommandPrefix => originData.CommandPrefix;
}
internal class AdapterInstance(object instance,MethodInfo dah,MethodInfo cah)
{
    private object Instance => instance;
    private MethodInfo DefaultActionHandler => dah;
    private MethodInfo CustomActionHandler => cah;
    public byte[]? InvokeAction(DefaultActionType action, byte[]? paras = null)
    {
        return DefaultActionHandler.Invoke(Instance, [action, paras]) as byte[];
    }
    public byte[]? InvokeAction(string action, byte[]? paras = null)
    {
        return CustomActionHandler.Invoke(Instance, [action, paras]) as byte[];
    }
}
