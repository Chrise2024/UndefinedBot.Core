using UndefinedBot.Core.Command.Arguments.ArgumentRange;
using UndefinedBot.Core.Command.Arguments.TokenContentType;
using UndefinedBot.Core.Command.CommandUtils;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

public sealed class DateArgument(IArgumentRange? range = null) : IArgumentType
{
    public ArgumentTypes ArgumentType => ArgumentTypes.Date;
    public string ArgumentTypeName => "Date";
    public IArgumentRange? Range => range;

    public bool IsValid(ParsedToken token)
    {
        return token is { TokenType: ParsedTokenTypes.Text, Content: TextTokenContent content } &&
               DateTime.TryParse(content.Text, out DateTime _);
    }

    public object GetValue(ParsedToken token)
    {
        return GetExactTypeValue(token);
    }

    public static DateTime GetDate(string key, CommandContext ctx)
    {
        if (ctx.ArgumentReference.TryGetValue(key, out ParsedToken token)) return GetExactTypeValue(token);

        throw new ArgumentInvalidException($"Undefined Argument: {key}");
    }

    private static DateTime GetExactTypeValue(ParsedToken token)
    {
        return token is { TokenType: ParsedTokenTypes.Text, Content: TextTokenContent content } &&
               DateTime.TryParse(content.Text, out DateTime val)
            ? val
            : throw new ArgumentInvalidException($"{token} Is Not Valid Positive Integer");
    }
}