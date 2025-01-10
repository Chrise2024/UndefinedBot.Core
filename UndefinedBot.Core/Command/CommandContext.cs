using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.NetWork;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command.Arguments;

namespace UndefinedBot.Core.Command;

/// <summary>
/// Context of command,containing apis, calling info and arguments
/// </summary>
public sealed class CommandContext(CommandInstance commandInstance,CommandInvokeProperties ip)
{
    public string PluginName => commandInstance.PluginId;
    public string CommandName => commandInstance.Name;
    public string RootPath => Environment.CurrentDirectory;
    public string CachePath => Path.Join(RootPath, "Cache", commandInstance.PluginId);
    public CommandInvokeProperties InvokeProperties => ip;
    public ILogger Logger => new BaseLogger(["Command",commandInstance.PluginId, commandInstance.Name]);
    public CacheManager Cache => commandInstance.Cache;
    public HttpRequest Request => new(commandInstance.PluginId);
    public ActionInvokeManager ActionInvoke => new(ip, Logger);
    internal Dictionary<string, ParsedToken> ArgumentReference { get; set; } = [];
}