using System.Text;
using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

public sealed class StringArgument(IArgumentRange? range = null) : IArgumentType
{
    public ArgumentTypes ArgumentType => ArgumentTypes.String;
    public string ArgumentTypeName => "String";
    public IArgumentRange? Range => range;

    public bool IsValid(ParsedToken token)
    {
        return token is { TokenType: ParsedTokenTypes.Normal, SerializedContent.Length: > 0 } &&
               (Range?.InRange(token.SerializedContent) ?? true);
    }

    public object GetValue(ParsedToken token) => GetExactTypeValue(token);

    public static string GetString(string key, CommandContext ctx)
    {
        if (ctx.ArgumentReference.TryGetValue(key, out ParsedToken token))
        {
            return GetExactTypeValue(token);
        }

        throw new ArgumentInvalidException($"Undefined Argument: {key}");
    }

    private static string GetExactTypeValue(ParsedToken token)
    {
        return token.TokenType == ParsedTokenTypes.Normal
            ? Encoding.UTF8.GetString(token.SerializedContent)
            : throw new ArgumentInvalidException("Token Is Not String");
    }
}