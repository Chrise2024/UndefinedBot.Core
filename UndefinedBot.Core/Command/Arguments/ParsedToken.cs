namespace UndefinedBot.Core.Command.Arguments;

/// <summary>
/// Prepare to use 'ProtoBuf' or string byte array to transmit communication content
/// </summary>
public readonly struct ParsedToken(ParsedTokenTypes tokenType, byte[] content)
{
    public ParsedTokenTypes TokenType => tokenType;
    public byte[] SerializedContent => content;
}

public enum ParsedTokenTypes
{
    //Text formatted content(Serialize to byte array in UTF-8 encoding),including string,date and number
    //number and date can be seen as string
    Normal = 0,
    User = 1,
    Reply = 2,
    Image = 3,
    File = 4,
}