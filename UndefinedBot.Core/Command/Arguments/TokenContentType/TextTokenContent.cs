namespace UndefinedBot.Core.Command.Arguments.TokenContentType;

public sealed class TextTokenContent : ITokenContent
{
    public required string Text { get; init; }
}