namespace UndefinedBot.Core.Command.Arguments.TokenContentType;

public sealed class ImageTokenContent : ITokenContent
{
    public required string ImageUrl;
    public string? ImageUnique;
    public ImageSize? Size;
}

public record ImageSize(int Width, int Height);