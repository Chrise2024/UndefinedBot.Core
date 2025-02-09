using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.CommandSource;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command.Arguments.TokenContentType;

namespace Adapter.OneBot11;

internal sealed partial class MsgHandler(AdapterConfigData adapterConfig, ILogger parentLogger)
{
    private ILogger Logger => parentLogger.GetSubLogger("MsgHandler");
    private AdapterConfigData AdapterConfig => adapterConfig;

    public (CommandBackgroundEnvironment?, BaseCommandSource?, ParsedToken[]?) HandleMsg(JsonNode msgJson)
    {
        if (!IsGroupMessageToHandle(msgJson))
        {
            return (null, null, null);
        }

        MsgBody? msgBody = msgJson.Deserialize<MsgBody>();
        (string? cmdName, List<ParsedToken> tokens) = Tokenize(msgBody?.RawMessage ?? "");
        if (msgBody is null || cmdName is null)
        {
            return (null, null, null);
        }

        CommandBackgroundEnvironment cip =
            CommandBackgroundEnvironment.Group(cmdName, msgBody.UserId, msgBody.MessageId, msgBody.Time);
        UserCommandSource ucs = UserCommandSource.Group(msgBody.UserId.ToString(), msgBody.GroupId.ToString(), msgBody.Sender.Nickname, 0);
        return (cip, ucs, tokens.ToArray());
    }

    private bool IsGroupMessageToHandle(JsonNode msgJson)
    {
        long gid = msgJson["group_id"]?.GetValue<long>() ?? 0;
        return gid != 0 && (msgJson["post_type"]?.GetValue<string>() == "message" &&
                            msgJson["message_type"]?.GetValue<string>() == "group" &&
                            AdapterConfig.GroupId.Contains(gid));
    }

    /// <summary>
    /// Split message into tokens
    /// </summary>
    /// <param name="msgString">Raw CQMessage string</param>
    /// <returns>tokens</returns>
    private (string?, List<ParsedToken>) Tokenize(string msgString)
    {
        Logger.Info($"Processing: {msgString}");
        List<ParsedToken> unsortedTokens = SplitRawCqMessage(msgString);
        int commandTokenIndex = unsortedTokens.FindIndex(item =>
            item is { TokenType: ParsedTokenTypes.Text, Content: TextTokenContent text } &&
            text.Text.StartsWith(AdapterConfig.CommandPrefix)
        );
        if (commandTokenIndex is -1 or > 1)
        {
            return (null, []);
        }

        unsortedTokens.RemoveAt(commandTokenIndex);
        return (((TextTokenContent)unsortedTokens[commandTokenIndex].Content).Text[AdapterConfig.CommandPrefix.Length..],
            unsortedTokens);
    }

    private List<ParsedToken> SplitRawCqMessage(string cqString)
    {
        if (cqString.Length == 0)
        {
            return [];
        }

        return GetCommandTokenRegex()
            .Matches(GetCqEntityRegex().Replace(cqString, match => $" \r0CQ{match.Value} "))
            .Select(m =>
            {
                string tmp = m.Value.Trim('"');
                return tmp.StartsWith("\r0CQ[")
                    ? DecodeCqEntity(tmp[4..])
                    : new ParsedToken(ParsedTokenTypes.Text, new TextTokenContent{Text = tmp});
            })
            .OfType<ParsedToken>()
            .ToList();
    }

    private ParsedToken? DecodeCqEntity(string cqEntityString)
    {
        string[] cqPieces = cqEntityString[1..^1]
            .Replace(",", "\r$\r")
            .Replace("&amp;", "&")
            .Replace("&#91;", "[")
            .Replace("&#93;", "]")
            .Replace("&#44;", ",")
            .Split("\r$\r");
        string cqType = cqPieces[0][3..];
        Dictionary<string, string> cqContent = cqPieces[1..]
            .Select(item => item.Split("=", 2, StringSplitOptions.RemoveEmptyEntries))
            .Where(item => item.Length == 2)
            .ToDictionary(item => item[0], item => item[1]);
        return cqType switch
        {
            "at" => new ParsedToken(ParsedTokenTypes.User, new UserTokenContent{UserId = cqContent["qq"]}),
            "reply" => new ParsedToken(ParsedTokenTypes.Reply, new ReplyTokenContent{ReplyToId = cqContent["id"]}),
            "image" => new ParsedToken(ParsedTokenTypes.Image, new ImageTokenContent{ImageUrl = cqContent.TryGetValue("url", out string? u) ? u : cqContent["file"]}),
            "file" => new ParsedToken(ParsedTokenTypes.File,new FileTokenContent{FileUrl = cqContent["url"],FileUnique = cqContent["file_unique"],Size = uint.Parse(cqContent["file_size"])}),
            _ => null
        };
    }

    [GeneratedRegex(@"\[CQ:\S+\]")]
    private static partial Regex GetCqEntityRegex();

    [GeneratedRegex(@"[\""].+?[\""]|[^ ]+")]
    private static partial Regex GetCommandTokenRegex();
}

[Serializable]
public sealed class MsgSender
{
    [JsonPropertyName("user_id")] public long UserId { get; init; } = 0;
    [JsonPropertyName("nickname")] public string Nickname { get; init; } = "";
    [JsonPropertyName("sex")] public string Sex { get; init; } = "";
    [JsonPropertyName("age")] public int Age { get; init; } = 0;
}

[Serializable]
public sealed class MsgBody
{
    [JsonPropertyName("time")] public long Time { get; init; } = 0;
    [JsonPropertyName("self_id")] public long SelfId { get; init; } = 0;
    [JsonPropertyName("post_type")] public string PostType { get; init; } = "";
    [JsonPropertyName("message_type")] public string MessageType { get; init; } = "";
    [JsonPropertyName("sub_type")] public string SubType { get; init; } = "";
    [JsonPropertyName("message_id")] public int MessageId { get; init; } = 0;
    [JsonPropertyName("group_id")] public long GroupId { get; init; } = 0;
    [JsonPropertyName("user_id")] public long UserId { get; init; } = 0;
    [JsonPropertyName("message")] public List<JsonNode> Message { get; init; } = [];
    [JsonPropertyName("raw_message")] public string RawMessage { get; init; } = "";
    [JsonPropertyName("font")] public int Font { get; init; } = 0;
    [JsonPropertyName("sender")] public MsgSender Sender { get; init; } = new();
}