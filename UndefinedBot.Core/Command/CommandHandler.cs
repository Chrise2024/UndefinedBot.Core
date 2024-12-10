using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command.Arguments;

namespace UndefinedBot.Core.Command;

public delegate Task CommandEventHandler(CallingProperty cp,List<ParsedToken> tokens);
/**
 * <summary>
 * Process OneBot11 Format Message and Resolve to Broadcast Message Event
 * </summary>
 */
public abstract class CommandHandler
{
    private static readonly List<long> s_workGroup = new ConfigManager().GetGroupList();

    private static readonly GeneralLogger s_commandHandlerLogger = new("MsgHandler");
    private static readonly JsonSerializerOptions s_jsonOption = new(){ WriteIndented = true };
    internal static event CommandEventHandler? CommandEvent;
    //public static readonly CacheRefreshEvent CacheEvent = new();
    public static async Task HandleMsg(JsonNode msgJson)
    {
        if ((msgJson["post_type"]?.GetValue<string>().Equals("message") ?? false) &&
            (msgJson["message_type"]?.GetValue<string>().Equals("group") ?? false) &&
            s_workGroup.Contains(msgJson["group_id"]?.GetValue<long>() ?? 0)
           )
        {
            MsgBody msgBody = msgJson.Deserialize<MsgBody>();
            (string? cmdName, List<ParsedToken> tokens) = CommandResolver.Tokenize(msgBody.RawMessage ?? "");
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
                s_commandHandlerLogger.Info(JsonSerializer.Serialize(cp, s_jsonOption));
                s_commandHandlerLogger.Info("Tokens:");
                s_commandHandlerLogger.Info(JsonSerializer.Serialize(tokens));
                CommandEvent?.Invoke(cp,tokens);
            }
        }
    }

    public static void Trigger(CallingProperty cp, List<ParsedToken> tokens)
    {
        CommandEvent?.Invoke(cp,tokens);
    }
}
public struct MsgSender
{
    [JsonPropertyName("user_id")] public long UserId;
    [JsonPropertyName("nickname")] public string Nickname;
    [JsonPropertyName("sex")] public string Sex;
    [JsonPropertyName("age")] public int Age;
}
public struct MsgBody
{
    [JsonPropertyName("time")] public long Time;
    [JsonPropertyName("self_id")] public long SelfId;
    [JsonPropertyName("post_type")] public string PostType;
    [JsonPropertyName("message_type")] public string MessageType;
    [JsonPropertyName("sub_type")] public string SubType;
    [JsonPropertyName("message_id")] public int MessageId;
    [JsonPropertyName("group_id")] public long GroupId;
    [JsonPropertyName("user_id")] public long UserId;
    [JsonPropertyName("message")] public List<JsonNode> Message;
    [JsonPropertyName("raw_message")] public string RawMessage;
    [JsonPropertyName("font")] public int Font;
    [JsonPropertyName("sender")] public MsgSender Sender;
}
