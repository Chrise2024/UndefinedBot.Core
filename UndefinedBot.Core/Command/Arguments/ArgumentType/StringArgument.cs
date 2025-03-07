﻿using UndefinedBot.Core.Command.Arguments.ArgumentRange;
using UndefinedBot.Core.Command.Arguments.TokenContentType;
using UndefinedBot.Core.Command.CommandUtils;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

public sealed class StringArgument(IArgumentRange? range = null) : IArgumentType
{
    public ArgumentTypes ArgumentType => ArgumentTypes.String;
    public string ArgumentTypeName => "String";
    public IArgumentRange? Range => range;

    public bool IsValid(ParsedToken token)
    {
        return token is { TokenType: ParsedTokenTypes.Text, Content: TextTokenContent content } &&
               content.Text.Length != 0 &&
               (Range?.InRange(token.Content) ?? true);
    }

    public static string GetString(string key, CommandContext ctx)
    {
        return ctx.GetArgumentReference(key) is { TokenType: ParsedTokenTypes.Text, Content: TextTokenContent content }
            ? content.Text
            : throw new ArgumentInvalidException("Token Is Not String");
    }
}