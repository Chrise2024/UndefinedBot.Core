using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

public class PosIntArgument(IArgumentRange? range = null) : IArgumentType
{
    public ArgumentTypes ArgumentType => ArgumentTypes.PosInt;
    public string ArgumentTypeName => "Positive Integer";
    public IArgumentRange? Range => range;
    public bool IsValid(ParsedToken token)
    {
        return token.TokenType == RawTokenTypes.NormalContent &&
               ulong.TryParse(token.Content, out ulong val) &&
               (Range?.InRange(val) ?? true);
    }
    public object GetValue(ParsedToken token) => GetExactTypeValue(token);
    public static ulong GetPosInt(string key,CommandContext ctx)
    {
        if (ctx._argumentReference.TryGetValue(key, out ParsedToken token))
        {
            return GetExactTypeValue(token);
        }
        throw new ArgumentInvalidException($"Undefined Argument: {key}");
    }
    private static ulong GetExactTypeValue(ParsedToken token)
    {
        return ulong.TryParse(token.Content, out ulong val)
            ? val
            : throw new ArgumentInvalidException($"{token} Is Not Valid Positive Integer");
    }
}