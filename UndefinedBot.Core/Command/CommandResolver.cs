using System.Text.RegularExpressions;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command.Arguments;

namespace UndefinedBot.Core.Command;

internal abstract partial class CommandResolver
{
    private static readonly string s_commandPrefix = new ConfigManager().GetCommandPrefix();

    /// <summary>
    /// Split message into tokens
    /// </summary>
    /// <param name="msgString">Raw CQMessage string</param>
    /// <returns>tokens</returns>
    public static (string?,List<ParsedToken>) Tokenize(string msgString)
    {
        List<ParsedToken> unsortedTokens = SplitRawCqMessage(msgString);
        int commandTokenIndex = unsortedTokens.FindIndex(item =>
            item.TokenType == RawTokenTypes.NormalContent && item.Content.StartsWith(s_commandPrefix)
        );
        if (commandTokenIndex is -1 or > 1)
        {
            return (null, []);
        }
        unsortedTokens.RemoveAt(commandTokenIndex);
        return (unsortedTokens[commandTokenIndex].Content[s_commandPrefix.Length..],unsortedTokens);

    }
    private static List<ParsedToken> SplitRawCqMessage(string cqString)
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
                    ? new ParsedToken(RawTokenTypes.CqCodeContent, item[4..])
                    : new ParsedToken(RawTokenTypes.NormalContent, item)
            ).ToList();
    }
    public static CqEntity DecodeCqEntity(string cqEntityString)
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
public readonly struct CallingProperty(
    string command,
    long callerUin,
    long groupId,
    int msgId,
    MessageSubType subType,
    long time
)
{
    public readonly string Command = command;
    public readonly long CallerUin = callerUin;
    public readonly long GroupId = groupId;
    public readonly int MsgId = msgId;
    public readonly MessageSubType SubType = subType;
    public readonly long Time = time;
}
public readonly struct CqEntity(string cqType,Dictionary<string, string> properties)
{
    public readonly string CqType = cqType;
    public readonly Dictionary<string, string> Properties = properties;
}
