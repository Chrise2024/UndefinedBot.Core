namespace UndefinedBot.Core.Command.Arguments.TokenContentType;

public sealed class TextContent : ITokenContent
{
    public required string Text { get; init; }
}