using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Command
{
    public delegate Task CommandEventHandler(ArgSchematics args);
    public class TriggerEvent
    {
        public event CommandEventHandler? OnCommand;
        public void Trigger(ArgSchematics args)
        {
            OnCommand?.Invoke(args);
        }
    }
    public abstract class CommandHandler
    {
        private static readonly List<long> s_workGroup = Core.GetConfigManager().GetGroupList();

        private static readonly Logger s_commandHandlerLogger = new("MsgHandler");

        public static readonly TriggerEvent CommandEvent = new();
        //public static readonly CacheRefreshEvent CacheEvent = new();
        public static async Task HandleMsg(MsgBodySchematics msgBody)
        {
            if ((msgBody.PostType?.Equals("message") ?? false) &&
                (msgBody.MessageType?.Equals("group") ?? false) &&
                s_workGroup.Contains(msgBody.GroupId ?? 0)
                )
            {
                ArgSchematics args = CommandResolver.Parse(msgBody);
                if (args.Status)
                {
                    s_commandHandlerLogger.Info("Handle","Executing with arg:");
                    s_commandHandlerLogger.Info("Handle", JsonConvert.SerializeObject(args, Formatting.Indented));
                    //CacheEvent.Trigger();
                    CommandEvent.Trigger(args);
                }
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
