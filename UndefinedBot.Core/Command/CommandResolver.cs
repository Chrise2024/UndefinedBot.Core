using System.Text.RegularExpressions;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command.Arguments;

namespace UndefinedBot.Core.Command;

internal abstract partial class CommandResolver
{
    private static readonly string s_commandPrefix = ConfigManager.GetCommandPrefix();

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
                    ? ParsedToken.CreateCqToken(item[4..])
                    : ParsedToken.CreateNormalToken(item)
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
[Serializable] public class CallingProperty
{
    public string Command { get; set; } = "";
    public long CallerUin { get; set; }
    public long GroupId { get; set; }
    public int MsgId { get; set; }
    public MessageSubType SubType { get; set; } = MessageSubType.Group;
    public long Time { get; set; }
}
public readonly struct CqEntity(string cqType,Dictionary<string, string> properties)
{
    public readonly string CqType = cqType;
    public readonly Dictionary<string, string> Properties = properties;
}
