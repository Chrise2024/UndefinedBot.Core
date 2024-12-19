
namespace UndefinedBot.Core.Command.Arguments;

[Serializable] public partial class ParsedToken(ParsedTokenTypes tokenType, string content)
{
    public ParsedTokenTypes TokenType { get; private set; } = tokenType;
    public string SerializedContent { get; private set; } = content;
}

public enum ParsedTokenTypes
{
    Normal = 0,//Text formatted content
    User = 1,
    Reply = 2,
    Image = 3,
    File = 4,
}
