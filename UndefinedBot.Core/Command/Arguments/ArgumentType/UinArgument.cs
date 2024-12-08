using System.Text.RegularExpressions;
using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType;

public class UinArgument(IArgumentRange? range = null) : IArgumentType
{
    public ArgumentTypes ArgumentType => ArgumentTypes.Uin;
    public string ArgumentTypeName => "At";
    public IArgumentRange? Range => range;
    public bool IsValid(ParsedToken token)
    {
        return token.TokenType == RawTokenTypes.CqCodeContent && QUin.CqAtRegex().IsMatch(token.Content);
    }
    public object GetValue(ParsedToken token) => GetExactTypeValue(token);
    public static QUin GetQUin(string key,CommandContext ctx)
    {
        if (ctx._argumentReference.TryGetValue(key, out ParsedToken token))
        {

            return GetExactTypeValue(token);
        }
        throw new ArgumentInvalidException($"Undefined Argument: {key}");
    }
    private static QUin GetExactTypeValue(ParsedToken token)
    {
        return QUin.Parse(token);
    }
}

public partial struct QUin
{
    public readonly long Uin;
    private QUin(long uin)
    {
        Uin = uin;
    }
    public static QUin Parse(ParsedToken token)
    {
        if (token.TokenType != RawTokenTypes.CqCodeContent || !CqAtRegex().IsMatch(token.Content))
        {
            throw new ArgumentInvalidException($"{token} Is Not Valid Uin");
        }

        return CommandResolver
            .DecodeCqEntity(token.Content)
            .Properties
            .TryGetValue("qq", out string? uin)
            ? (long.TryParse(uin, out long val)
                ? new QUin(val) :
                throw new ArgumentInvalidException($"{token} Is Not Valid Uin"))
            : throw new ArgumentInvalidException($"{token} Is Not Valid Uin");
    }
    [GeneratedRegex(@"^\[CQ:at,qq=\d+\S*\]$")]
    public static partial Regex CqAtRegex();
}