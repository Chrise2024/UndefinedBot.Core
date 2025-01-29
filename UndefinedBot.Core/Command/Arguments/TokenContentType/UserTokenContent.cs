namespace UndefinedBot.Core.Command.Arguments.TokenContentType;

public sealed class UserTokenContent : ITokenContent
{
    public required string UserId;
    public byte[]? AdditionalInfo;
}