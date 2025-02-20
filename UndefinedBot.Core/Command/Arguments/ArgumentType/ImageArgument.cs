﻿using UndefinedBot.Core.Command.Arguments.ArgumentRange;
using UndefinedBot.Core.Command.Arguments.TokenContentType;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

public sealed class ImageArgument : IArgumentType
{
    public ArgumentTypes ArgumentType => ArgumentTypes.Image;
    public string ArgumentTypeName => "Image";
    public IArgumentRange? Range => null;

    public bool IsValid(ParsedToken token)
    {
        return token.TokenType == ParsedTokenTypes.Image;
    }

    public object GetValue(ParsedToken token) => GetExactTypeValue(token);

    public static ImageTokenContent GetImage(string key, CommandContext ctx)
    {
        if (ctx.ArgumentReference.TryGetValue(key, out ParsedToken token))
        {
            return GetExactTypeValue(token);
        }

        throw new ArgumentInvalidException($"Undefined Argument: {key}");
    }

    private static ImageTokenContent GetExactTypeValue(ParsedToken token)
    {
        return token is { TokenType: ParsedTokenTypes.Image, Content: ImageTokenContent img }
            ? img
            : throw new ArgumentInvalidException("Token Is Not Image");
    }
}