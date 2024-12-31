using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

public sealed class IntegerArgument(IArgumentRange? range = null) : IArgumentType
{
    public ArgumentTypes ArgumentType => ArgumentTypes.Integer;
    public string ArgumentTypeName => "Integer";
    public IArgumentRange? Range => range;
    public bool IsValid(ParsedToken token)
    {
        return token.TokenType == ParsedTokenTypes.Normal &&
               long.TryParse(token.SerializedContent, out long val) &&
               (Range?.InRange(val) ?? true);
    }

    public object GetValue(ParsedToken token) => GetExactTypeValue(token);
    public static long GetInteger(string key,CommandContext ctx)
    {
        if (ctx.ArgumentReference.TryGetValue(key, out ParsedToken? token))
        {
            return GetExactTypeValue(token);
        }
        throw new ArgumentInvalidException($"Undefined Argument: {key}");
    }

    private static long GetExactTypeValue(ParsedToken token)
    {
        return token.TokenType == ParsedTokenTypes.Normal && long.TryParse(token.SerializedContent, out long val)
            ? val
            : throw new ArgumentInvalidException("Token Is Not Integer");
    }
}
