using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Adapter.ActionParam;
using UndefinedBot.Core.NetWork;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command.Arguments;
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
    public readonly string CachePath;
    public readonly CommandInformation Information;
    public readonly ILogger Logger;
    public readonly CacheManager Cache;
    public readonly HttpRequest Request;
    public readonly IActionManager Action;
    public readonly MessageBuilder MessageBuilder;
    internal readonly Dictionary<string, ParsedToken> ArgumentReference = [];

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

    internal CommandContext(CommandInstance commandInstance, CommandInformation ip, IActionManager actionManager)
    {
        PluginName = commandInstance.PluginId;
        CommandName = commandInstance.Name;
        CachePath = Path.Join(RootPath, "Cache", commandInstance.PluginId);
        Information = ip;
        Logger = commandInstance.AcquireLogger();
        Cache = commandInstance.Cache;
        Request = new HttpRequest(commandInstance.PluginId,Logger);
        Action = actionManager;
        MessageBuilder = new();
    }

    public void Dispose()
    {
        Request.Dispose();
        ArgumentReference.Clear();
    }
}