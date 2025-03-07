﻿using UndefinedBot.Core.Command.Arguments.ArgumentRange;
using UndefinedBot.Core.Command.Arguments.TokenContentType;
using UndefinedBot.Core.Command.CommandUtils;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

public sealed class NumberArgument(IArgumentRange? range = null) : IArgumentType
{
    public ArgumentTypes ArgumentType => ArgumentTypes.Number;
    public string ArgumentTypeName => "Number";
    public IArgumentRange? Range => range;

    public bool IsValid(ParsedToken token)
    {
        return token is { TokenType: ParsedTokenTypes.Text, Content: TextTokenContent content } &&
               double.TryParse(content.Text, out double val) &&
               (Range?.InRange(val) ?? true);
    }

    public static double GetNumber(string key, CommandContext ctx)
    {
        return ctx.GetArgumentReference(key) is
                   { TokenType: ParsedTokenTypes.Text, Content: TextTokenContent content } &&
               double.TryParse(content.Text, out double val)
            ? val
            : throw new ArgumentInvalidException("Token Is Not Number");
    }
}