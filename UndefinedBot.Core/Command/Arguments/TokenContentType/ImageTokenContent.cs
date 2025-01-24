namespace UndefinedBot.Core.Command.Arguments.TokenContentType;

public sealed class ImageTokenContent : ITokenContent
{
    public required string ImageUrl { get; init; }
    public string? ImageUnique { get; init; }
    public ImageSize? Size { get; init; }
}
public record ImageSize(int Width, int Height);