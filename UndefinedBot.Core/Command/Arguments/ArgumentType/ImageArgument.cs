using System.Text.RegularExpressions;
using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

public class ImageArgument : IArgumentType
{
    public ArgumentTypes ArgumentType => ArgumentTypes.Image;
    public string ArgumentTypeName => "Image";
    public IArgumentRange? Range => null;
    public bool IsValid(ParsedToken token)
    {
        return token.TokenType == RawTokenTypes.CqCodeContent && QImage.CqImageRegex().IsMatch(token.Content);
    }

    public object GetValue(ParsedToken token) => GetExactTypeValue(token);
    public static QImage GetImage(string key,CommandContext ctx)
    {
        if (ctx._argumentReference.TryGetValue(key, out ParsedToken? token))
        {

            return GetExactTypeValue(token);
        }
        throw new ArgumentInvalidException($"Undefined Argument: {key}");
    }

    private static QImage GetExactTypeValue(ParsedToken token)
    {
        return QImage.Parse(token);
    }
}
public partial struct QImage
{
    public readonly string File;
    public readonly string? Url;
    public readonly string? Type;
    private QImage(string fl,string? url = null,string? type = null)
    {
        File = fl;
        Url = url;
        Type = type;
    }

    public static QImage Parse(ParsedToken token)
    {
        if (token.TokenType != RawTokenTypes.CqCodeContent || !CqImageRegex().IsMatch(token.Content))
        {
            throw new ArgumentInvalidException($"{token} Is Not Valid Image");
        }

        CqEntity entity = CommandResolver.DecodeCqEntity(token.Content);
        if (entity.Properties.TryGetValue("file", out string? fl))
        {
            return new QImage(fl, entity.Properties.GetValueOrDefault("url"),
                entity.Properties.GetValueOrDefault("type"));
        }

        throw new ArgumentInvalidException($"{token} Is Not Valid Image");
    }
    [GeneratedRegex(@"^\[CQ:image\S*\]$")]
    public static partial Regex CqImageRegex();
}
