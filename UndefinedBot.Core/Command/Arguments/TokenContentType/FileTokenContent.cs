namespace UndefinedBot.Core.Command.Arguments.TokenContentType;

public sealed class FileTokenContent : ITokenContent
{
    public required string FileUrl { get; init; }
    public string? FileUnique  { get; init; }
    public uint? Size { get; init; }
}