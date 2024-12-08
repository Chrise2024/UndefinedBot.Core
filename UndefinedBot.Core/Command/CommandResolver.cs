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
        if (commandTokenIndex == -1)
        {
            return (null, []);
        }
        string cmd = unsortedTokens[commandTokenIndex].Content.Replace(s_commandPrefix, "");
        unsortedTokens.RemoveAt(commandTokenIndex);
        return (cmd,unsortedTokens);

    }
    private static List<ParsedToken> SplitRawCqMessage(string cqString)
    {
        if (cqString.Length == 0)
        {
            return [];
        }

        return GetCqEntityRegex()
            .Replace(cqString, match => $" {match.Value} ")
            .Split(" ", StringSplitOptions.RemoveEmptyEntries)
            .Select(item =>
                GetCqEntityRegex().IsMatch(item)
                    ? new ParsedToken(RawTokenTypes.CqCodeContent, item)
                    : new ParsedToken(RawTokenTypes.NormalContent, item)
            ).ToList();
    }
    public static CqEntity DecodeCqEntity(string cqEntityString)
    {
        Dictionary<string,string> properties = [];
        string[] cqPieces = cqEntityString[1..^1]
            .Replace(",", "\r$\r")
            .Replace("&amp;", "&")
            .Replace("&#91;", "[")
            .Replace("&#93;", "]")
            .Replace("&#44;", ",")
            .Split("\r$\r");
        foreach (string cqPiece in cqPieces[1..])
        {
            string[] temp = cqPiece.Split("=",2,StringSplitOptions.RemoveEmptyEntries);
            if (temp.Length > 1)
            {
                properties[temp[0]] = temp[1];
            }
        }
        return new CqEntity(cqPieces[0][3..],properties);
    }
    [GeneratedRegex(@"\[CQ:\S+\]")]
    private static partial Regex GetCqEntityRegex();
}
public struct CallingProperty(
    string command,
    long callerUin,
    long groupId,
    int msgId,
    string subType,
    long time
)
{
    public readonly string Command = command;
    public readonly long CallerUin = callerUin;
    public readonly long GroupId = groupId;
    public readonly int MsgId = msgId;
    public readonly string SubType = subType;
    public readonly long Time = time;
}
public struct CqEntity(string cqType,Dictionary<string, string> properties)
{
    public string CqType = cqType;
    public Dictionary<string, string> Properties = properties;
}