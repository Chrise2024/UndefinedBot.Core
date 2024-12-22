
namespace UndefinedBot.Core.Command.Arguments;

/// <summary>
/// Prepare to use 'ProtoBuf' to transmit communication content
/// </summary>
[Serializable] public class ParsedToken(ParsedTokenTypes tokenType, byte[] content)
{
    public ParsedTokenTypes TokenType { get; private set; } = tokenType;
    public byte[] SerializedContent { get; private set; } = content;
}

public enum ParsedTokenTypes
{
    Normal = 0,//Text formatted content(Serialize to byte array in UTF-8 encoding)
    User = 1,
    Reply = 2,
    Image = 3,
    File = 4,
}
