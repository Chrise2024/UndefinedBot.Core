namespace UndefinedBot.Core.BasicMessage;

public class ImageMessageNode : IMessageNode
{
    public required string Url { get; init; }
    public string? SubType { get;init; }
}