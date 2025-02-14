namespace UndefinedBot.Core.Plugin.BasicMessage;

public sealed class ImageMessageNode : IMessageNode
{
    public required string Url;
    public string? SubType;
}