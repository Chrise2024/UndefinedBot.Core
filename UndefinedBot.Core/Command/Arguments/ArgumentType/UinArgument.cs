using System.Text.RegularExpressions;
using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType
{
    public class UinArgument(IArgumentRange? range = null) : IArgumentType
    {
        public string TypeName => "At";
        public IArgumentRange? Range => range;
        public bool IsValid(string token)
        {
            return QUin.CqAtRegex().IsMatch(token);
        }
        public object GetValue(string token)
        {
            return QUin.Parse(token);
        }
        public static QUin GetQUin(string key,CommandContext ctx)
        {
            string token = ctx.ArgumentReference.GetValueOrDefault(key) ??
                           throw new ArgumentInvalidException($"Undefined Argument: {key}");
            return QUin.Parse(token);
        }
    }

    public partial class QUin
    {
        public readonly long Uin;
        private QUin(long uin)
        {
            Uin = uin;
        }
        public static QUin Parse(string token)
        {
            if (CqAtRegex().IsMatch(token))
            {
                return CommandResolver
                    .DecodeCqEntity(token)
                    .Properties
                    .TryGetValue("qq", out string? uin)
                    ? (Int64.TryParse(uin, out long val)
                        ? new QUin(val) :
                        throw new ArgumentInvalidException($"{token} Is Not Valid Uin"))
                    : throw new ArgumentInvalidException($"{token} Is Not Valid Uin");
            }
            else
            {
                throw new ArgumentInvalidException($"{token} Is Not Valid Uin");
            }
        }
        [GeneratedRegex(@"^\[CQ:at,qq=\d+\S*\]$")]
        public static partial Regex CqAtRegex();
    }
}
