using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

public class NumberArgument(IArgumentRange? range = null) : IArgumentType
{
    public ArgumentTypes ArgumentType => ArgumentTypes.Number;
    public string ArgumentTypeName => "Number";
    public IArgumentRange? Range => range;
    public bool IsValid(ParsedToken token)
    {
        return token.TokenType == RawTokenTypes.NormalContent &&
               double.TryParse(token.Content, out double val) &&
               (Range?.InRange(val) ?? true);
    }

    public object GetValue(ParsedToken token) => GetExactTypeValue(token);
    public static double GetNumber(string key,CommandContext ctx)
    {
        if (ctx._argumentReference.TryGetValue(key, out ParsedToken token))
        {
            return GetExactTypeValue(token);
        }
        throw new ArgumentInvalidException($"Undefined Argument: {key}");
    }
    private static double GetExactTypeValue(ParsedToken token)
    {
        return double.TryParse(token.Content, out double val)
            ? val
            : throw new ArgumentInvalidException($"{token} Is Not Valid Number");
    }
}