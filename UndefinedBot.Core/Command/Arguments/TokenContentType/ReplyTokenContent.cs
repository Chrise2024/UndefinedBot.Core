namespace UndefinedBot.Core.Command.Arguments.TokenContentType;

public sealed class ReplyTokenContent : ITokenContent
{
    public required string ReplyToId;
    public byte[]? AdditionalInfo;
}