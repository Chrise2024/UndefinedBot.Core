using System.Text.RegularExpressions;
using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

public class ReplyArgument(IArgumentRange? range = null) : IArgumentType
{
    public ArgumentTypes ArgumentType => ArgumentTypes.Reply;
    public string ArgumentTypeName => "Reply";
    public IArgumentRange? Range => range;
    public bool IsValid(ParsedToken token)
    {
        return token.TokenType == RawTokenTypes.CqCodeContent && QReply.CqReplyRegex().IsMatch(token.Content);
    }
    public object GetValue(ParsedToken token) => GetExactTypeValue(token);
    public static QReply GetQReply(string key,CommandContext ctx)
    {
        if (ctx._argumentReference.TryGetValue(key, out ParsedToken? token))
        {

            return GetExactTypeValue(token);
        }
        throw new ArgumentInvalidException($"Undefined Argument: {key}");
    }
    private static QReply GetExactTypeValue(ParsedToken token)
    {
        return QReply.Parse(token);
    }
}

public partial struct QReply
{
    public readonly int MsgId;

    private QReply(int msgId)
    {
        MsgId = msgId;
    }

    public static QReply Parse(ParsedToken token)
    {
        if (token.TokenType != RawTokenTypes.CqCodeContent || !CqReplyRegex().IsMatch(token.Content))
        {
            throw new ArgumentInvalidException($"{token} Is Not Valid Reply");
        }
        return CommandResolver
            .DecodeCqEntity(token.Content)
            .Properties
            .TryGetValue("id", out string? msgId)
            ? (int.TryParse(msgId, out int val)
                ? new QReply(val) :
                throw new ArgumentInvalidException($"{token} Is Not Valid Reply"))
            : throw new ArgumentInvalidException($"{token} Is Not Valid Reply");
    }
    [GeneratedRegex(@"^\[CQ:reply,id=[-]?\d+\]$")]
    public static partial Regex CqReplyRegex();
}
