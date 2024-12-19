﻿using System.Text.RegularExpressions;
using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

public class ReplyArgument(IArgumentRange? range = null) : IArgumentType
{
    public ArgumentTypes ArgumentType => ArgumentTypes.Reply;
    public string ArgumentTypeName => "Reply";
    public IArgumentRange? Range => range;
    public bool IsValid(ParsedToken token)
    {
        return token.TokenType == ParsedTokenTypes.Reply;
    }
    public object GetValue(ParsedToken token) => GetExactTypeValue(token);
    public static string GetReply(string key,CommandContext ctx)
    {
        if (ctx.ArgumentReference.TryGetValue(key, out ParsedToken? token))
        {
            return GetExactTypeValue(token);
        }
        throw new ArgumentInvalidException($"Undefined Argument: {key}");
    }
    private static string GetExactTypeValue(ParsedToken token)
    {
        return token.TokenType == ParsedTokenTypes.Reply ? token.SerializedContent : throw new ArgumentInvalidException("Token Is Not Reply");
    }
}
