namespace UndefinedBot.Core.Command.Arguments.TokenContentType;

public sealed class UserContent : ITokenContent
{
    public required string UserId { get; init; }
    public object? AdditionalInfo { get; init; }
}