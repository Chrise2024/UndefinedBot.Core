using UndefinedBot.Core.Command.Arguments.ArgumentRange;
using UndefinedBot.Core.Command.Arguments.TokenContentType;
using UndefinedBot.Core.Command.CommandUtils;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

public sealed class ReplyArgument(IArgumentRange? range = null) : IArgumentType
{
    public ArgumentTypes ArgumentType => ArgumentTypes.Reply;
    public string ArgumentTypeName => "Reply";
    public IArgumentRange? Range => range;

    public bool IsValid(ParsedToken token)
    {
        return token.TokenType == ParsedTokenTypes.Reply;
    }

    public object GetValue(ParsedToken token)
    {
        return GetExactTypeValue(token);
    }

    public static ReplyTokenContent GetReply(string key, CommandContext ctx)
    {
        if (ctx.ArgumentReference.TryGetValue(key, out ParsedToken token)) return GetExactTypeValue(token);

        throw new ArgumentInvalidException($"Undefined Argument: {key}");
    }

    /// <summary>
    /// 'Reply' may be a string of message id ?
    /// </summary>
    private static ReplyTokenContent GetExactTypeValue(ParsedToken token)
    {
        return token is { TokenType: ParsedTokenTypes.Reply, Content: ReplyTokenContent reply }
            ? reply
            : throw new ArgumentInvalidException("Token Is Not Reply");
    }
}