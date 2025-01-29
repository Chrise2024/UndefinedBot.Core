using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Adapter.ActionParam;
using UndefinedBot.Core.NetWork;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Plugin.BasicMessage;

namespace UndefinedBot.Core.Command;

/// <summary>
/// Context of command,containing apis, calling info and arguments
/// </summary>
public sealed class CommandContext : IDisposable
{
    public readonly string PluginName;
    public readonly string CommandName;
    public readonly string RootPath = Environment.CurrentDirectory;
    public readonly string CachePath;
    public readonly CommandInvokeProperties InvokeProperties;
    public readonly ExtendableLogger Logger;
    public readonly CacheManager Cache;
    public readonly HttpRequest Request;
    public readonly ActionInvokeManager ActionInvoke;
    public readonly MessageBuilder MessageBuilder;
    internal readonly Dictionary<string, ParsedToken> ArgumentReference = [];
    public void SendFeedback(string message)
    {
        //ActionInvoke.InvokeDefaultAction();
        ActionInvoke.InvokeDefaultAction(
            InvokeProperties.SubType == MessageSubType.Group
                ? DefaultActionType.SendGroupMsg
                : DefaultActionType.SendPrivateMsg,
            DefaultActionParameterWrapper.Common(InvokeProperties.SourceId.ToString(),
                new SendGroupMgsParam
                {
                    MessageChain = [new TextMessageNode{Text = message}]
                })
            );
    }

    internal CommandContext(CommandInstance commandInstance, CommandInvokeProperties ip)
    {
        PluginName = commandInstance.PluginId;
        CommandName = commandInstance.Name;
        CachePath = Path.Join(RootPath, "Cache", commandInstance.PluginId);
        InvokeProperties = ip;
        Logger = new(["Command", commandInstance.PluginId, commandInstance.Name]);
        Cache = commandInstance.Cache;
        Request = new HttpRequest(commandInstance.PluginId);
        ActionInvoke = new(ip, Logger);
        MessageBuilder = new();
    }

    public void Dispose()
    {
        Logger.Dispose();
        Request.Dispose();
        ActionInvoke.Dispose();
        ArgumentReference.Clear();
    }
}

/// <summary>
/// For help command
/// </summary>
internal class HelpCommandContext(CommandInvokeProperties ip)
{
    public ExtendableLogger Logger => new("Help");
    public ActionInvokeManager ActionInvoke => new(ip, Logger);
}