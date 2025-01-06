// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.CommandSource;
using UndefinedBot.Core.Utils;
using Google.Protobuf;
using Ob11Adapter;

namespace Adapter.OneBot11;

internal sealed partial class MsgHandler(AdapterConfigData adapterConfig,ILogger parentLogger)
{
    private ILogger Logger =>parentLogger.GetSubLogger("MsgHandler");
    private AdapterConfigData AdapterConfig => adapterConfig;

    public (CommandInvokeProperties?, BaseCommandSource?, List<ParsedToken>?) HandleMsg(JsonNode msgJson)
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

        CommandInvokeProperties cip =
            CommandInvokeProperties.Group(cmdName, msgBody.UserId, msgBody.MessageId, msgBody.Time);
        UserCommandSource ucs = UserCommandSource.Group(msgBody.UserId, msgBody.GroupId, msgBody.Sender.Nickname, 0);
        return (cip, ucs, tokens);
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
            item.TokenType == ParsedTokenTypes.Normal && Encoding.UTF8.GetString(item.SerializedContent)
                .StartsWith(AdapterConfig.CommandPrefix)
        );
        if (commandTokenIndex is -1 or > 1)
        {
            return (null, []);
        }

        unsortedTokens.RemoveAt(commandTokenIndex);
        return (
            Encoding.UTF8.GetString(unsortedTokens[commandTokenIndex].SerializedContent)[
                AdapterConfig.CommandPrefix.Length..], unsortedTokens);
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
                    : new ParsedToken(ParsedTokenTypes.Normal, Encoding.UTF8.GetBytes(tmp));
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
            "at" => new ParsedToken(ParsedTokenTypes.User, Encoding.UTF8.GetBytes(cqContent["qq"])),
            "reply" => new ParsedToken(ParsedTokenTypes.Reply, Encoding.UTF8.GetBytes(cqContent["id"])),
            "image" => new ParsedToken(ParsedTokenTypes.Image,
                new OneBot11ReceiveImage
                {
                    FileUnique = cqContent["file_unique"],
                    Url = cqContent.TryGetValue("url", out string? u) ? u : cqContent["file"]
                }.ToByteArray()),
            "file" => new ParsedToken(ParsedTokenTypes.File,
                new OneBot11ReceiveImage
                {
                    Url = cqContent["url"],
                    FileUnique = cqContent["file_unique"],
                    FileSize = uint.Parse(cqContent["file_size"])
                }.ToByteArray()),
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