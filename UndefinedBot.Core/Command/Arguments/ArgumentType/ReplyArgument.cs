using System.Text.RegularExpressions;
using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType
{
    public class ReplyArgument(IArgumentRange? range = null) : IArgumentType
    {
        public IArgumentRange? Range { get; } = range;
        public bool IsValid(string token)
        {
            return QReply.CqReplyRegex().IsMatch(token);
        }
        public object GetValue(string token)
        {
            return QReply.Parse(token);
        }
        public static QReply GetQReply(string key,CommandContext ctx)
        {
            string token = ctx.ArgumentReference.GetValueOrDefault(key) ??
                           throw new ArgumentInvalidException($"Undefined Argument: {key}");
            return QReply.Parse(token);
        }
    }

    public partial class QReply
    {
        public readonly int MsgId;

        private QReply(int msgId)
        {
            MsgId = msgId;
        }

        public static QReply Parse(string token)
        {
            if (CqReplyRegex().IsMatch(token))
            {
                return CommandResolver
                    .DecodeCqEntity(token)
                    .Properties
                    .TryGetValue("id", out string? msgId)
                    ? (Int32.TryParse(msgId, out int val) 
                        ? new QReply(val) :
                        throw new ArgumentInvalidException($"{token} Is Not Valid Reply"))
                    : throw new ArgumentInvalidException($"{token} Is Not Valid Reply");
            }
            else
            {
                throw new ArgumentInvalidException($"{token} Is Not Valid Reply");
            }
        }
        [GeneratedRegex(@"^\[CQ:reply,id=[-]?\d+\]$")]
        public static partial Regex CqReplyRegex();
    }
}