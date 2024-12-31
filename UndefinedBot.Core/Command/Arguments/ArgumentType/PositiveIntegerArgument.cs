using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

public sealed class PositiveIntegerArgument(IArgumentRange? range = null) : IArgumentType
{
    public ArgumentTypes ArgumentType => ArgumentTypes.PositiveInteger;
    public string ArgumentTypeName => "Positive Integer";
    public IArgumentRange? Range => range;
    public bool IsValid(ParsedToken token)
    {
        return token.TokenType == ParsedTokenTypes.Normal &&
               ulong.TryParse(token.SerializedContent, out ulong val) &&
               (Range?.InRange(val) ?? true);
    }
    public object GetValue(ParsedToken token) => GetExactTypeValue(token);
    public static ulong GetPositiveInteger(string key,CommandContext ctx)
    {
        if (ctx.ArgumentReference.TryGetValue(key, out ParsedToken? token))
        {
            return GetExactTypeValue(token);
        }
        throw new ArgumentInvalidException($"Undefined Argument: {key}");
    }
    private static ulong GetExactTypeValue(ParsedToken token)
    {
        return token.TokenType == ParsedTokenTypes.Normal && ulong.TryParse(token.SerializedContent, out ulong val)
            ? val
            : throw new ArgumentInvalidException("Token Is Not Valid Positive Integer");
    }
}
