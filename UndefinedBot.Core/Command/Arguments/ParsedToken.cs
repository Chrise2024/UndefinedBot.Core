using System.Text.RegularExpressions;

namespace UndefinedBot.Core.Command.Arguments;

[Serializable] public partial class ParsedToken
{
    public RawTokenTypes TokenType { get; private set; } = RawTokenTypes.NormalContent;
    public string Content { get; private set; }

    private ParsedToken(RawTokenTypes tokenType, string content)
    {
        if (tokenType == RawTokenTypes.NormalContent)
        {
            Content = content;
            return;
        }
        Content = content;
        TokenType = RawTokenTypes.CqCodeContent;
    }

    public static ParsedToken CreateCqToken(string content)
    {
        // if (!GetCqEntityRegex().IsMatch(content))
        // {
        //     throw new ArgumentException($"Invalid CQ Content: {content}");
        // }
        return new ParsedToken(RawTokenTypes.CqCodeContent,content);
    }
    public static ParsedToken CreateNormalToken(string content)
    {
        return new ParsedToken(RawTokenTypes.NormalContent,content);
    }
    public static ParsedToken CreateToken(RawTokenTypes tokenType,string content)
    {
        return tokenType == RawTokenTypes.NormalContent ? CreateNormalToken(content) : CreateCqToken(content);
    }
    [GeneratedRegex(@"\[CQ:\S+\]")]
    private static partial Regex GetCqEntityRegex();
}

public enum RawTokenTypes
{
    NormalContent = 0,
    CqCodeContent = 1,
}
