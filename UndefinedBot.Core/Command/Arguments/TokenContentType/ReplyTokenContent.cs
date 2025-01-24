namespace UndefinedBot.Core.Command.Arguments.TokenContentType;

public sealed class ReplyTokenContent : ITokenContent
{
    public required string ReplyToId { get; init; }
    public object? AdditionalInfo { get; init; }
}