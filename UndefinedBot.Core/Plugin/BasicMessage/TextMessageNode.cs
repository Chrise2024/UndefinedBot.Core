namespace UndefinedBot.Core.BasicMessage;

public sealed class TextMessageNode : IMessageNode
{
    public required string Text { get; init; }
}