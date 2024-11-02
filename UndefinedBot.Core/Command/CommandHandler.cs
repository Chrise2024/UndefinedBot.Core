using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UndefinedBot.Core.NetWork;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Command
{
    public delegate Task CommandEventHandler(ArgSchematics CommandArg);
    public class CommandEvent
    {
        public event CommandEventHandler? OnCommand;
        public void Trigger(ArgSchematics args)
        {
            OnCommand?.Invoke(args);
        }
    }
    public class CommandHandler
    {
        private static readonly HttpApi s_httpApiI = new(Core.GetConfigManager().GetHttpPostUrl());

        private static readonly List<long> s_workGRoup = Core.GetConfigManager().GetGroupList();

        private static readonly List<string> s_commandList = [];

        private static readonly Logger s_commandHandlerLogger = new("MsgHandler");

        public static readonly CommandEvent Event = new();
        public static async Task HandleMsg(MsgBodySchematics MsgBody)
        {
            if ((MsgBody.PostType?.Equals("message") ?? false) &&
                (MsgBody.MessageType?.Equals("group") ?? false) &&
                s_workGRoup.Contains(MsgBody.GroupId ?? 0)
                )
            {
                ArgSchematics args = CommandResolver.Parse(MsgBody);
                if (args.Status)
                {
                    if (s_commandList.Contains(args.Command))
                    {
                        s_commandHandlerLogger.Info("Executing with arg:");
                        s_commandHandlerLogger.Info(JsonConvert.SerializeObject(args,Formatting.Indented));
                        Event.Trigger(args);
                    }
                    else
                    {
                        await s_httpApiI.SendGroupMsg(
                                args.GroupId,
                                MsgBuilder
                                    .GetInstance()
                                    .Text($"这发的什么东西: <{args.Command}>").Build()
                            );
                    }
                }
            }
        }
        public static void UpdateCommandList(List<string> CL)
        {
            foreach (var item in CL)
            {
                s_commandList.Add(item);
            }
        }
    }
    public struct MsgSenderSchematics
    {
        [JsonProperty("user_id")] public long? UserId;
        [JsonProperty("nickname")] public string? Nickname;
        [JsonProperty("sex")] public string? Sex;
        [JsonProperty("age")] public int? Age;
    }
    public struct MsgBodySchematics
    {
        [JsonProperty("time")] public long? Time;
        [JsonProperty("self_id")] public long? SelfId;
        [JsonProperty("post_type")] public string? PostType;
        [JsonProperty("message_type")] public string? MessageType;
        [JsonProperty("sub_type")] public string? SubType;
        [JsonProperty("message_id")] public int? MessageId;
        [JsonProperty("group_id")] public long? GroupId;
        [JsonProperty("user_id")] public long? UserId;
        [JsonProperty("message")] public List<JObject>? Message;
        [JsonProperty("raw_message")] public string? RawMessage;
        [JsonProperty("font")] public int? Font;
        [JsonProperty("sender")] public MsgSenderSchematics? Sender;
    }
}
