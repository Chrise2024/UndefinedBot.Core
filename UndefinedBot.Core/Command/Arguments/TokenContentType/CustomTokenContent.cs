namespace UndefinedBot.Core.Command.Arguments.TokenContentType;

public sealed class CustomTokenContent : ITokenContent
{
    public required string Type;
    public required byte[] Content;
}