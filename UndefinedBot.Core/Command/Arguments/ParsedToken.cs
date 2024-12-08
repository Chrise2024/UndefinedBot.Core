
namespace UndefinedBot.Core.Command.Arguments;

public struct ParsedToken(RawTokenTypes tokenType, string content)
{
    public readonly RawTokenTypes TokenType = tokenType;
    public readonly string Content = content;
}

public enum RawTokenTypes
{
    NormalContent = 0,
    CqCodeContent = 1,
}