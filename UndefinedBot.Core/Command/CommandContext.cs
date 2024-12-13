﻿using UndefinedBot.Core.NetWork;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command.Arguments;

namespace UndefinedBot.Core.Command;

/// <summary>
/// Context of command,containing apis, calling info and arguments
/// </summary>
public class CommandContext(string commandName,UndefinedApi baseApi,CallingProperty cp)
{
    public readonly string PluginName = baseApi.PluginName;
    public readonly string CommandName = commandName;
    public readonly string RootPath = baseApi.RootPath;
    public readonly string CachePath = baseApi.CachePath;
    //public readonly List<string> Tokens = tokens;
    public readonly CallingProperty CallingProperties = cp;
    internal readonly Dictionary<string, ParsedToken> _argumentReference = [];
    public readonly CommandLogger Logger = new(baseApi.PluginName,commandName);
    public readonly Config ConfigData = baseApi.ConfigData;
    public readonly CacheManager Cache = baseApi.Cache;
    public readonly HttpRequest Request = baseApi.Request;
    public readonly HttpApi Api = baseApi.Api;
    public MsgBuilder GetMessageBuilder() => MsgBuilder.GetInstance();
    public ForwardMessageBuilder GetForwardBuilder() => ForwardMessageBuilder.GetInstance();
}
