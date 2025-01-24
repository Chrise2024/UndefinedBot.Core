namespace UndefinedBot.Core.BasicMessage;

public class CustomMessageNode : IMessageNode
{
    public required byte[] Content { get; init; }
}