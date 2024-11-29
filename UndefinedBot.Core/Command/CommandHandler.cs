using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Command
{
    public delegate Task CommandEventHandler(CallingProperty cp,List<string> tokens);
    /**
     * <summary>
     * Process OneBot11 Format Message and Resolve to Broadcast Message Event
     * </summary>
     */
    public abstract class CommandHandler
    {
        private static readonly List<long> s_workGroup = new ConfigManager().GetGroupList();

        private static readonly Logger s_commandHandlerLogger = new("MsgHandler");
        public static event CommandEventHandler? CommandEvent;
        //public static readonly CacheRefreshEvent CacheEvent = new();
        public static async Task HandleMsg(JObject msgJson)
        {
            if ((msgJson.Value<string>("post_type")?.Equals("message") ?? false) &&
                (msgJson.Value<string>("message_type")?.Equals("group") ?? false) &&
                s_workGroup.Contains(msgJson.Value<long>("group_id"))
                )
            {
                MsgBody msgBody = msgJson.ToObject<MsgBody>();
                (string? cmdName, List<string> tokens) = CommandResolver.Tokenize(msgBody.RawMessage ?? "");
                if (cmdName != null)
                {
                    CallingProperty cp = new(
                        cmdName,
                        msgBody.UserId,
                        msgBody.GroupId,
                        msgBody.MessageId,
                        msgBody.SubType ?? "group",
                        msgBody.Time
                        );
                    s_commandHandlerLogger.Info("Executing...\nProperties:");
                    s_commandHandlerLogger.Info(JsonConvert.SerializeObject(cp, Formatting.Indented));
                    s_commandHandlerLogger.Info("Tokens:");
                    s_commandHandlerLogger.Info(JsonConvert.SerializeObject(tokens));
                    CommandEvent?.Invoke(cp,tokens);
                }
            }
        }

        public static void Trigger(CallingProperty cp, List<string> tokens)
        {
            CommandEvent?.Invoke(cp,tokens);
        }
    }
    public struct MsgSender
    {
        [JsonProperty("user_id")] public long UserId;
        [JsonProperty("nickname")] public string Nickname;
        [JsonProperty("sex")] public string Sex;
        [JsonProperty("age")] public int Age;
    }
    public struct MsgBody
    {
        [JsonProperty("time")] public long Time;
        [JsonProperty("self_id")] public long SelfId;
        [JsonProperty("post_type")] public string PostType;
        [JsonProperty("message_type")] public string MessageType;
        [JsonProperty("sub_type")] public string SubType;
        [JsonProperty("message_id")] public int MessageId;
        [JsonProperty("group_id")] public long GroupId;
        [JsonProperty("user_id")] public long UserId;
        [JsonProperty("message")] public List<JObject> Message;
        [JsonProperty("raw_message")] public string RawMessage;
        [JsonProperty("font")] public int Font;
        [JsonProperty("sender")] public MsgSender Sender;
    }
}
