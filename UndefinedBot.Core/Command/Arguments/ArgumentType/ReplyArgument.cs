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

    public static ReplyTokenContent GetReply(string key, CommandContext ctx)
    {
        return ctx.GetArgumentReference(key) is { TokenType: ParsedTokenTypes.Reply, Content: ReplyTokenContent reply }
            ? reply
            : throw new ArgumentInvalidException("Token Is Not Reply");
    }
}