using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Adapter.ActionParam;
using UndefinedBot.Core.NetWork;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.Plugin.BasicMessage;

namespace UndefinedBot.Core.Command.CommandUtils;

/// <summary>
/// Context of command,containing apis, calling info and arguments
/// </summary>
public sealed class CommandContext : IDisposable
{
    public readonly string PluginName;
    public readonly string CommandName;
    public readonly string RootPath = Environment.CurrentDirectory;
    public readonly CommandInformation Information;
    public readonly ILogger Logger;
    public readonly CacheManager Cache;
    public readonly HttpRequest Request;
    public readonly ActionManager Action;

    private readonly Dictionary<string, ParsedToken> _argumentReference = [];

    internal void AddArgumentReference(string key, ParsedToken token)
    {
        _argumentReference[key] = token;
    }

    public ParsedToken GetArgumentReference(string key)
    {
        return _argumentReference.TryGetValue(key, out ParsedToken token)
            ? token
            : throw new ArgumentInvalidException($"Undefined Argument: {key}");
    }

    public async Task SendFeedbackAsync(string message)
    {
        //ActionInvoke.InvokeDefaultAction();
        await Action.InvokeAction(
            Information.SubType == MessageSubType.Group
                ? ActionType.SendGroupMsg
                : ActionType.SendPrivateMsg,
            Information.SourceId,
            new SendGroupMgsParam
            {
                MessageChain = [new TextMessageNode { Text = message }]
            }
        );
    }

    internal CommandContext(CommandInstance commandInstance, CommandInformation ip, ActionManager actionManager)
    {
        PluginName = commandInstance.PluginId;
        CommandName = commandInstance.Name;
        Information = ip;
        Logger = commandInstance.AcquireLogger();
        Cache = commandInstance.Cache;
        Request = new HttpRequest(Logger);
        Action = actionManager;
    }

    public void Dispose()
    {
        Request.Dispose();
        _argumentReference.Clear();
    }
}