using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

public class StringArgument(IArgumentRange? range = null) : IArgumentType
{
    public ArgumentTypes ArgumentType => ArgumentTypes.String;
    public string ArgumentTypeName => "String";
    public IArgumentRange? Range => range;
    public bool IsValid(ParsedToken token)
    {
        return token.TokenType == ParsedTokenTypes.Normal &&
               !string.IsNullOrEmpty(token.SerializedContent) &&
               (Range?.InRange(token.SerializedContent) ?? true);
    }

    public object GetValue(ParsedToken token) => GetExactTypeValue(token);
    public static string GetString(string key,CommandContext ctx)
    {
        if (ctx.ArgumentReference.TryGetValue(key, out ParsedToken? token))
        {
            return GetExactTypeValue(token);
        }
        throw new ArgumentInvalidException($"Undefined Argument: {key}");
    }
    private static string GetExactTypeValue(ParsedToken token)
    {
        return token.TokenType == ParsedTokenTypes.Normal
            ? token.SerializedContent
            : throw new ArgumentInvalidException("Token Is Not String");
    }
}
