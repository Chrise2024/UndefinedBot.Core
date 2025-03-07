﻿using UndefinedBot.Core.Command.Arguments.ArgumentRange;
using UndefinedBot.Core.Command.Arguments.TokenContentType;
using UndefinedBot.Core.Command.CommandUtils;

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

    public static ImageTokenContent GetImage(string key, CommandContext ctx)
    {
        return ctx.GetArgumentReference(key) is { TokenType: ParsedTokenTypes.Image, Content: ImageTokenContent img }
            ? img
            : throw new ArgumentInvalidException("Token Is Not Image");
    }
}