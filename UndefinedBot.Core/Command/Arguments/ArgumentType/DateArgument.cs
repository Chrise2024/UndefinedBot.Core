using System.Text;
using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

public class DateArgument(IArgumentRange? range = null) : IArgumentType
{
    public ArgumentTypes ArgumentType => ArgumentTypes.Date;
    public string ArgumentTypeName => "Date";
    public IArgumentRange? Range => range;
    public bool IsValid(ParsedToken token)
    {
            return token.TokenType == ParsedTokenTypes.Normal &&
                   DateTime.TryParse(Encoding.UTF8.GetString(token.SerializedContent),out DateTime _);
        }
    public object GetValue(ParsedToken token) => GetExactTypeValue(token);
    public static DateTime GetDate(string key,CommandContext ctx)
    {
            if (ctx.ArgumentReference.TryGetValue(key,out ParsedToken? token))
            {
                return GetExactTypeValue(token);
            }
            throw new ArgumentInvalidException($"Undefined Argument: {key}");
        }
    private static DateTime GetExactTypeValue(ParsedToken token)
    {
            return token.TokenType == ParsedTokenTypes.Normal && DateTime.TryParse(Encoding.UTF8.GetString(token.SerializedContent), out DateTime val)
                ? val
                : throw new ArgumentInvalidException($"{token} Is Not Valid Positive Integer");
        }
}
