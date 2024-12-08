using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

public class DateArgument(IArgumentRange? range = null) : IArgumentType
{
    public ArgumentTypes ArgumentType => ArgumentTypes.Date;
    public string ArgumentTypeName => "Date";
    public IArgumentRange? Range => range;
    public bool IsValid(ParsedToken token)
    {
            return token.TokenType == RawTokenTypes.NormalContent &&
                   DateTime.TryParse(token.Content,out DateTime _);
        }
    public object GetValue(ParsedToken token) => GetExactTypeValue(token);
    public static DateTime GetDate(string key,CommandContext ctx)
    {
            if (ctx._argumentReference.TryGetValue(key,out ParsedToken token))
            {
                return GetExactTypeValue(token);
            }
            throw new ArgumentInvalidException($"Undefined Argument: {key}");
        }
    private static DateTime GetExactTypeValue(ParsedToken token)
    {
            return DateTime.TryParse(token.Content, out DateTime val)
                ? val
                : throw new ArgumentInvalidException($"{token} Is Not Valid Positive Integer");
        }
}