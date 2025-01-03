using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.NetWork;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command.Arguments;

namespace UndefinedBot.Core.Command;

/// <summary>
/// Context of command,containing apis, calling info and arguments
/// </summary>
public sealed class CommandContext(string commandName,string pluginId,CommandInvokeProperties ip)
{
    public string PluginName => pluginId;
    public string CommandName => commandName;
    public string RootPath => Environment.CurrentDirectory;
    public string CachePath => Path.Join(RootPath,"Cache",pluginId);
    public CommandInvokeProperties InvokeProperties => ip;
    public ILogger Logger => new CommandLogger(pluginId,commandName);
    public Config MainConfigData => UndefinedApi.MainConfigData;
    //public CacheManager Cache => new(pluginName);
    public HttpRequest Request => new(pluginId);
    public ActionInvokeManager ActionInvoke => new(ip,Logger);
    //public readonly HttpApi Api = baseApi.Api;
    public MsgBuilder GetMessageBuilder() => MsgBuilder.GetInstance();
    public ForwardMessageBuilder GetForwardBuilder() => ForwardMessageBuilder.GetInstance();
    internal Dictionary<string, ParsedToken> ArgumentReference { get; set; } = [];
}
