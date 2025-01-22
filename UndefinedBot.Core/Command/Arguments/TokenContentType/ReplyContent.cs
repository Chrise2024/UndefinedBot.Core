namespace UndefinedBot.Core.Command.Arguments.TokenContentType;

public sealed class ReplyContent : ITokenContent
{
    public required string ReplyToId { get; init; }
    public object? AdditionalInfo { get; init; }
}