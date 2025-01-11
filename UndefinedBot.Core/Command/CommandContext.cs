using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.NetWork;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command.Arguments;

namespace UndefinedBot.Core.Command;

/// <summary>
/// Context of command,containing apis, calling info and arguments
/// </summary>
public sealed class CommandContext
{
    public string PluginName { get; }
    public string CommandName { get; }
    public string RootPath { get; } = Environment.CurrentDirectory;
    public string CachePath { get; }
    public CommandInvokeProperties InvokeProperties { get; }
    public ILogger Logger { get; }
    public CacheManager Cache { get; }
    public HttpRequest Request { get; }
    public ActionInvokeManager ActionInvoke { get; }
    internal Dictionary<string, ParsedToken> ArgumentReference { get; set; } = [];

    internal CommandContext(CommandInstance commandInstance, CommandInvokeProperties ip)
    {
        PluginName = commandInstance.PluginId;
        CommandName = commandInstance.Name;
        CachePath = Path.Join(RootPath, "Cache", commandInstance.PluginId);
        InvokeProperties = ip;
        Logger = new BaseLogger(["Command",commandInstance.PluginId, commandInstance.Name]);
        Cache = commandInstance.Cache;
        Request = new HttpRequest(commandInstance.PluginId);
        ActionInvoke = new(ip, Logger);
    }
}

/// <summary>
/// For help command
/// </summary>
internal class HelpCommandContext(CommandInvokeProperties ip)
{
    public ILogger Logger => new BaseLogger("Help");
    public ActionInvokeManager ActionInvoke => new(ip, Logger);
}