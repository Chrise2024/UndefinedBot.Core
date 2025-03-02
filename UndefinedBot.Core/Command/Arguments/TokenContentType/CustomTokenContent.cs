namespace UndefinedBot.Core.Command.Arguments.TokenContentType;

public sealed class CustomTokenContent : ITokenContent
{
    public required string Type { get; init; }
    public required byte[] Content { get; init; }
}