using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

public class StringArgument(IArgumentRange? range = null) : IArgumentType
{
    public ArgumentTypes ArgumentType => ArgumentTypes.String;
    public string ArgumentTypeName => "String";
    public IArgumentRange? Range => range;
    public bool IsValid(ParsedToken token)
    {
        return token.TokenType == RawTokenTypes.NormalContent &&
               !string.IsNullOrEmpty(token.Content) &&
               (Range?.InRange(token.Content) ?? true);
    }

    public object GetValue(ParsedToken token) => GetExactTypeValue(token);
    public static string GetString(string key,CommandContext ctx)
    {
        if (ctx._argumentReference.TryGetValue(key, out ParsedToken token))
        {
            return GetExactTypeValue(token);
        }
        throw new ArgumentInvalidException($"Undefined Argument: {key}");
    }
    private static string GetExactTypeValue(ParsedToken token)
    {
        return token.TokenType == RawTokenTypes.NormalContent
            ? token.Content
            : throw new ArgumentInvalidException("Invalid String Literal");
    }
}