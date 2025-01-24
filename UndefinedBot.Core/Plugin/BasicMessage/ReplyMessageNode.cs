namespace UndefinedBot.Core.BasicMessage;

public class ReplyMessageNode : IMessageNode
{
    public required string ReplyToId { get; init; }
}