// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.Command.CommandSource;
using UndefinedBot.Core.Utils;

namespace Adapter.OneBot11;

internal partial class MsgHandler(AdapterLogger logger,AdapterConfigData adapterConfig)
{
    private AdapterLogger Logger => logger;
    private AdapterConfigData AdapterConfig => adapterConfig;
    public (PrimeInvokeProperties?,BaseCommandSource?, List<ParsedToken>?) HandleMsg(JsonNode msgJson)
    {
        if (!IsGroupMessageToHandle(msgJson))
        {
            return (null,null,null);
        }

        MsgBody? msgBody = msgJson.Deserialize<MsgBody>();
        (string? cmdName, List<ParsedToken> tokens) = Tokenize(msgBody?.RawMessage ?? "");
        if (msgBody == null || cmdName == null)
        {
            return (null,null,null);
        }
        PrimeInvokeProperties pip = PrimeInvokeProperties.Group(cmdName,msgBody.UserId,msgBody.MessageId,msgBody.Time);
        UserCommandSource ucs = UserCommandSource.Group(msgBody.UserId, msgBody.GroupId, msgBody.Sender.Nickname, 0);
        return (pip, ucs, tokens);
    }

    private bool IsGroupMessageToHandle(JsonNode msgJson)
    {
        long gid = msgJson["group_id"]?.GetValue<long>() ?? 0;
        return gid != 0 && (msgJson["post_type"]?.GetValue<string>() == "message" &&
                            msgJson["message_type"]?.GetValue<string>() == "group" &&
                            AdapterConfig.GroupIds.Contains(gid));
    }

    /// <summary>
    /// Split message into tokens
    /// </summary>
    /// <param name="msgString">Raw CQMessage string</param>
    /// <returns>tokens</returns>
    private (string?,List<ParsedToken>) Tokenize(string msgString)
    {
        List<ParsedToken> unsortedTokens = SplitRawCqMessage(msgString);
        int commandTokenIndex = unsortedTokens.FindIndex(item =>
            item.TokenType == ParsedTokenTypes.Normal && item.SerializedContent.StartsWith(AdapterConfig.CommandPrefix)
        );
        if (commandTokenIndex is -1 or > 1)
        {
            return (null, []);
        }
        unsortedTokens.RemoveAt(commandTokenIndex);
        return (unsortedTokens[commandTokenIndex].SerializedContent[AdapterConfig.CommandPrefix.Length..],unsortedTokens);

    }
    private List<ParsedToken> SplitRawCqMessage(string cqString)
    {
        if (cqString.Length == 0)
        {
            return [];
        }

        return GetCqEntityRegex()
            .Replace(cqString, match => $" \r0CQ{match.Value} ")
            .Split(" ", StringSplitOptions.RemoveEmptyEntries)
            .Select(item =>
                item.StartsWith("\r0CQ[")
                    ? DecodeCqEntity(item[4..])
                    : new ParsedToken(ParsedTokenTypes.Normal,item)
            )
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
        switch (cqType)
        {
            case "at":
                return new ParsedToken(ParsedTokenTypes.User,cqContent["qq"]);
            case "reply":
                return new ParsedToken(ParsedTokenTypes.Reply,cqContent["id"]);
            case "image":
                return new ParsedToken(ParsedTokenTypes.Image,
                    JsonSerializer.Serialize(
                        new ImageContent(
                            cqContent.TryGetValue("url", out string? u) ? u : cqContent["file"],
                            cqContent["file_unique"])
                        )
                    );
            case "file":
                return new ParsedToken(
                    ParsedTokenTypes.File,
                    JsonSerializer.Serialize(
                        new FileContent(
                            cqContent["url"],
                            cqContent["file_unique"],
                            int.Parse(cqContent["file_size"])
                            )
                        )
                    );
            default:
                return null;
        }
    }
    [GeneratedRegex(@"\[CQ:\S+\]")]
    private static partial Regex GetCqEntityRegex();
}
[Serializable] public class MsgSender
{
    [JsonPropertyName("user_id")] public long UserId { get; set; } = 0;
    [JsonPropertyName("nickname")] public string Nickname { get; set; } = "";
    [JsonPropertyName("sex")] public string Sex { get; set; } = "";
    [JsonPropertyName("age")] public int Age { get; set; } = 0;
}
[Serializable] public class MsgBody
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
