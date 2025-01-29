namespace UndefinedBot.Core.Command.Arguments.TokenContentType;

public sealed class FileTokenContent : ITokenContent
{
    public required string FileUrl;
    public string? FileUnique;
    public uint? Size;
}