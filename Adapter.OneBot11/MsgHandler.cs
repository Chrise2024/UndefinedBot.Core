// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.Arguments;
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
            item.TokenType == RawTokenTypes.NormalContent && item.Content.StartsWith(AdapterConfig.CommandPrefix)
        );
        if (commandTokenIndex is -1 or > 1)
        {
            return (null, []);
        }
        unsortedTokens.RemoveAt(commandTokenIndex);
        return (unsortedTokens[commandTokenIndex].Content[AdapterConfig.CommandPrefix.Length..],unsortedTokens);

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
                    ? ParsedToken.CreateCqToken(item[4..])
                    : ParsedToken.CreateNormalToken(item)
            ).ToList();
    }
    internal CqEntity DecodeCqEntity(string cqEntityString)
    {
        string[] cqPieces = cqEntityString[1..^1]
            .Replace(",", "\r$\r")
            .Replace("&amp;", "&")
            .Replace("&#91;", "[")
            .Replace("&#93;", "]")
            .Replace("&#44;", ",")
            .Split("\r$\r");
        return new CqEntity(
            cqPieces[0][3..],
            cqPieces[1..].Select(item => item.Split("=", 2, StringSplitOptions.RemoveEmptyEntries))
                .Where(item => item.Length == 2)
                .ToDictionary(item => item[0], item => item[1])
            );
    }
    [GeneratedRegex(@"\[CQ:\S+\]")]
    private static partial Regex GetCqEntityRegex();
}
