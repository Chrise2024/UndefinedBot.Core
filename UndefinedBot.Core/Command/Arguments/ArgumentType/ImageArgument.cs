using System.Text.Json;
using System.Text.RegularExpressions;
using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

public sealed class ImageArgument : IArgumentType
{
    public ArgumentTypes ArgumentType => ArgumentTypes.Image;
    public string ArgumentTypeName => "Image";
    public IArgumentRange? Range => null;

    public bool IsValid(ParsedToken token)
    {
        return token.TokenType == ParsedTokenTypes.Image;
    }

    public object GetValue(ParsedToken token) => GetExactTypeValue(token);

    public static ImageContent GetImage(string key, CommandContext ctx)
    {
        if (ctx.ArgumentReference.TryGetValue(key, out ParsedToken token))
        {
            return GetExactTypeValue(token);
        }

        throw new ArgumentInvalidException($"Undefined Argument: {key}");
    }

    private static ImageContent GetExactTypeValue(ParsedToken token)
    {
        return token.TokenType == ParsedTokenTypes.Image
            ? JsonSerializer.Deserialize<ImageContent>(token.SerializedContent)!
            : throw new ArgumentInvalidException("Token Is Not Image");
    }
}

public sealed class ImageContent(string imageUrl, string imageUnique, int? width = null, int? height = null)
{
    public string ImageUrl => imageUrl;
    public string ImageUnique => imageUnique;
    public int? Width => width;
    public int? Height => height;
}