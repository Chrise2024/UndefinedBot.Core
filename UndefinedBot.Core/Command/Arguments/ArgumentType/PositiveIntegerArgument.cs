using UndefinedBot.Core.Command.Arguments.ArgumentRange;
using UndefinedBot.Core.Command.Arguments.TokenContentType;
using UndefinedBot.Core.Command.CommandUtils;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

public sealed class PositiveIntegerArgument(IArgumentRange? range = null) : IArgumentType
{
    public ArgumentTypes ArgumentType => ArgumentTypes.PositiveInteger;
    public string ArgumentTypeName => "Positive Integer";
    public IArgumentRange? Range => range;

    public bool IsValid(ParsedToken token)
    {
        return token is { TokenType: ParsedTokenTypes.Text, Content: TextTokenContent content } &&
               ulong.TryParse(content.Text, out ulong val) &&
               (Range?.InRange(val) ?? true);
    }

    public static ulong GetPositiveInteger(string key, CommandContext ctx)
    {
        return ctx.GetArgumentReference(key) is
                   { TokenType: ParsedTokenTypes.Text, Content: TextTokenContent content } &&
               ulong.TryParse(content.Text, out ulong val)
            ? val
            : throw new ArgumentInvalidException("Token Is Not Valid Positive Integer");
    }
}