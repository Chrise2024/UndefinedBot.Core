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
internal abstract class CommandHandler
{
    private static readonly List<long> s_workGroup =ConfigManager.GetGroupList();

    private static readonly GeneralLogger s_commandHandlerLogger = new("MsgHandler");
    private static readonly JsonSerializerOptions s_serializerOptions = new() { WriteIndented = true };
    internal static event CommandEventHandler? CommandEvent;
    public static void HandleMsg(JsonNode msgJson)
    {
        if (!IsGroupMessageToHandle(msgJson))
        {
            return;
        }

        MsgBody? msgBody = msgJson.Deserialize<MsgBody>();
        (string? cmdName, List<ParsedToken> tokens) = CommandResolver.Tokenize(msgBody?.RawMessage ?? "");
        if (msgBody == null || cmdName == null)
        {
            return;
        }

        CallingProperty cp = new()
        {
            Command = cmdName,
            CallerUin = msgBody.UserId,
            GroupId = msgBody.GroupId,
            MsgId = msgBody.MessageId,
            SubType = msgBody.SubType switch
            {
                "friend" => MessageSubType.Friend,
                "group" => MessageSubType.Group,
                _ => MessageSubType.Other,
            },
            Time = msgBody.Time
        };
        s_commandHandlerLogger.Info("Executing...\nProperties:");
        s_commandHandlerLogger.Info(JsonSerializer.Serialize(cp, s_serializerOptions));
        s_commandHandlerLogger.Info("Tokens:");
        s_commandHandlerLogger.Info(JsonSerializer.Serialize(tokens));
        CommandEvent?.Invoke(cp,tokens);
    }

    internal static void Trigger(CallingProperty cp, List<ParsedToken> tokens)
    {
        CommandEvent?.Invoke(cp,tokens);
    }

    private static bool IsGroupMessageToHandle(JsonNode msgJson)
    {
        long gid = msgJson["group_id"]?.GetValue<long>() ?? 0;
        return gid != 0 && (msgJson["post_type"]?.GetValue<string>() == "message" &&
                            msgJson["message_type"]?.GetValue<string>() == "group" &&
                            s_workGroup.Contains(gid));
    }
}

[Serializable] public class MsgSender
{
    [JsonPropertyName("user_id")] public long UserId { get; set; } = 0;
    [JsonPropertyName("nickname")] public string Nickname { get; set; } = "";
    [JsonPropertyName("sex")] public string Sex { get; set; } = "";
    [JsonPropertyName("age")] public int Age { get; set; } = 0;
}
[Serializable]  public class MsgBody
{
    [JsonPropertyName("time")] public long Time { get; set; } = 0;
    [JsonPropertyName("self_id")] public long SelfId { get; set; } = 0;
    [JsonPropertyName("post_type")] public string PostType { get; set; } = "";
    [JsonPropertyName("message_type")] public string MessageType { get; set; } = "";
    [JsonPropertyName("sub_type")] public string SubType { get; set; } = "";
    [JsonPropertyName("message_id")] public int MessageId { get; set; } = 0;
    [JsonPropertyName("group_id")] public long GroupId { get; set; } = 0;
    [JsonPropertyName("user_id")] public long UserId { get; set; } = 0;
    [JsonPropertyName("message")] public List<JsonNode> Message { get; set; } = [];
    [JsonPropertyName("raw_message")] public string RawMessage { get; set; } = "";
    [JsonPropertyName("font")] public int Font { get; set; } = 0;
    [JsonPropertyName("sender")] public MsgSender Sender { get; set; } = new();
}
public enum MessageSubType
{
    Friend = 0,
    Group = 1,
    Other = 2,
}
