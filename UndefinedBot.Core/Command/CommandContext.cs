using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.NetWork;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command.Arguments;

namespace UndefinedBot.Core.Command;

/// <summary>
/// Context of command,containing apis, calling info and arguments
/// </summary>
public class CommandContext(string commandName,UndefinedApi baseApi,CommandInvokeProperties ip)
{
    public string PluginName => baseApi.PluginName;
    public string CommandName => commandName;
    public string RootPath => baseApi.RootPath;
    public string CachePath => baseApi.CachePath;
    public CommandInvokeProperties InvokeProperties => ip;
    public CommandLogger Logger => new(baseApi.PluginName,commandName);
    public Config MainConfigData => UndefinedApi.MainConfigData;
    public CacheManager Cache => baseApi.Cache;
    public HttpRequest Request => baseApi.Request;
    public ActionManager Action => new();
    //public readonly HttpApi Api = baseApi.Api;
    public MsgBuilder GetMessageBuilder() => MsgBuilder.GetInstance();
    public ForwardMessageBuilder GetForwardBuilder() => ForwardMessageBuilder.GetInstance();
    internal Dictionary<string, ParsedToken> ArgumentReference { get; set; } = [];
}
