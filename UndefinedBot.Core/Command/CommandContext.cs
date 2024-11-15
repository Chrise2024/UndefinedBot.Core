﻿using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.NetWork;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Command
{
    public class CommandContext(string commandName,List<string> tokens,UndefinedAPI baseApi,CallingProperty cp,Dictionary<string,string> ar)
    {
        public readonly string PluginName = baseApi.PluginName;
        public readonly string CommandName = commandName;
        public readonly string RootPath = baseApi.RootPath;
        public readonly string CachePath = baseApi.CachePath;
        public readonly List<string> Tokens = tokens;
        public readonly CallingProperty CallingProperties = cp;
        public readonly Dictionary<string, string> ArgumentReference = ar;
        public readonly CommandLogger Logger = new(baseApi.PluginName,commandName);
        public readonly ConfigManager Config = baseApi.Config;
        public readonly CacheManager Cache = baseApi.Cache;
        public readonly HttpRequest Request = baseApi.Request;
        public readonly HttpApi Api = baseApi.Api;
        public MsgBuilder GetMessageBuilder()
        {
            return MsgBuilder.GetInstance();
        }
        public ForwardBuilder GetForwardBuilder()
        {
            return ForwardBuilder.GetInstance();
        }
    }
}